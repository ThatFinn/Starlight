using Starlight.Enums;
using Starlight.Storage;
using UnityEngine.InputSystem;

namespace Starlight.EmberMode.Components;

[InjectIntoIL]
internal class EmberModeTransformGizmo : MonoBehaviour
{
    public enum TransformTool { Move, Rotate, Scale }
    public static TransformTool CurrentTool = TransformTool.Move;

    public static GameObject SelectedObject => _instance ? _instance._selected : null;
    private static EmberModeTransformGizmo _instance;

    private GameObject _selected;
    private Color _originalColor;
    private bool _hadOriginalColor;

    private GameObject _gizmoRoot;
    private GameObject _xHandle;
    private GameObject _yHandle;
    private GameObject _zHandle;

    private bool _isDragging;
    private int _dragAxis = -1;
    private Vector3 _dragStartWorldPos;
    private Vector3 _dragStartObjPos;
    private Plane _dragPlane;

    private const float HANDLE_COLLIDER_RADIUS = 0.18f;
    private const float MIN_ARROW_LENGTH = 1.0f;
    private const float ARROW_SCALE_FACTOR = 0.15f;
    private const float TIP_LENGTH = 0.18f;
    
    private const int GIZMO_LAYER = 2;

    private int _hoveredAxis = -1;

    void OnEnable()
    {
        _instance = this;
    }

    void OnDestroy()
    {
        Deselect();
    }

    void Update()
    {
        if (EmberModeMode.Current != EmberModeMode.Mode.Transform)
        {
            if (_selected) Deselect();
            return;
        }

        var cam = MiscEUtil.GetActiveCamera();
        if (!cam) return;

        if (Mouse.current == null) return;

        if (LKey.Escape.OnKeyDown())
        {
            if (_selected) Deselect();
            return;
        }

        HandleHover(cam);

        if (Mouse.current.leftButton.wasPressedThisFrame)
            HandleClick(cam);

        if (_isDragging && Mouse.current.leftButton.isPressed)
            HandleDrag(cam);

        if (_isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
            StopDrag();

        if ((CurrentTool == TransformTool.Move || CurrentTool == TransformTool.Scale) && _selected && _gizmoRoot)
        {
            _gizmoRoot.transform.position = _selected.transform.position;
            _gizmoRoot.transform.rotation = _selected.transform.rotation;
            float dist = Vector3.Distance(cam.transform.position, _selected.transform.position);
            float scale = Mathf.Max(MIN_ARROW_LENGTH, dist * ARROW_SCALE_FACTOR);
            _gizmoRoot.transform.localScale = Vector3.one * scale;
        }

        UpdateToolInfo();

        if (_selected)
        {
            if (CurrentTool == TransformTool.Move)
                RenderMoveGizmo(cam);
            else if (CurrentTool == TransformTool.Scale)
                RenderScaleGizmo(cam);
            else if (CurrentTool == TransformTool.Rotate)
                RenderRotateGizmo(cam);
        }
    }

    private void RenderMoveGizmo(Camera camera)
    {
        var dist = Vector3.Distance(camera.transform.position, _selected.transform.position);
        var scale = Mathf.Max(MIN_ARROW_LENGTH, dist * ARROW_SCALE_FACTOR);
        GizmosEUtil.DrawMoveGizmo(_selected.transform.position, _selected.transform.rotation, scale, _hoveredAxis, camera);
    }

    private void RenderScaleGizmo(Camera camera)
    {
        var dist = Vector3.Distance(camera.transform.position, _selected.transform.position);
        var scale = Mathf.Max(MIN_ARROW_LENGTH, dist * ARROW_SCALE_FACTOR);
        GizmosEUtil.DrawScaleGizmo(_selected.transform.position, _selected.transform.rotation, scale, _hoveredAxis, camera);
    }

    private void RenderRotateGizmo(Camera camera)
    {
        var dist = Vector3.Distance(camera.transform.position, _selected.transform.position);
        var scale = Mathf.Max(MIN_ARROW_LENGTH, dist * ARROW_SCALE_FACTOR);
        GizmosEUtil.DrawRotateGizmo(_selected.transform.position, _selected.transform.rotation, scale, _hoveredAxis, camera);
    }

    private void HandleHover(Camera cam)
    {
        if (_isDragging || !_gizmoRoot) { return; }

        if (UnityEngine.EventSystems.EventSystem.current && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            _hoveredAxis = -1;
            return;
        }

        var newHover = -1;
        var mousePos = Mouse.current.position.ReadValue();
        var ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));

        if (CurrentTool == TransformTool.Move || CurrentTool == TransformTool.Scale)
        {
            var closest = float.MaxValue;
            for (int axis = 0; axis < 3; axis++)
            {
                var handle = GetHandle(axis);
                if (!handle) continue;
                var col = handle.GetComponent<Collider>();
                if (!col) continue;
                if (col.Raycast(ray, out var hit, 1000f))
                {
                    if (hit.distance < closest)
                    {
                        closest = hit.distance;
                        newHover = axis;
                    }
                }
            }
        }
        else if (CurrentTool == TransformTool.Rotate && _selected)
        {
            var distToTarget = Vector3.Distance(cam.transform.position, _selected.transform.position);
            var scale = Mathf.Max(MIN_ARROW_LENGTH, distToTarget * ARROW_SCALE_FACTOR);
            
            var closest = float.MaxValue;
            for (int axis = 0; axis < 3; axis++)
            {
                var axisDir = GetAxisDirection(axis);
                var plane = new Plane(axisDir, _selected.transform.position);
                
                if (plane.Raycast(ray, out float enter))
                {
                    var hitPoint = ray.GetPoint(enter);
                    var dFromCenter = Vector3.Distance(hitPoint, _selected.transform.position);
                    var thickness = scale * 0.15f;
                    
                    if (Mathf.Abs(dFromCenter - scale) < thickness)
                    {
                        if (enter < closest)
                        {
                            closest = enter;
                            newHover = axis;
                        }
                    }
                }
            }
        }

        _hoveredAxis = newHover;
    }

    private void HandleClick(Camera cam)
    {
        if (UnityEngine.EventSystems.EventSystem.current && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        var mousePos = Mouse.current.position.ReadValue();
        var ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));

        if (CurrentTool == TransformTool.Move || CurrentTool == TransformTool.Scale)
        {
            if (_gizmoRoot)
            {
                var closest = float.MaxValue;
                var hitAxis = -1;
                for (int axis = 0; axis < 3; axis++)
                {
                    var handle = GetHandle(axis);
                    if (!handle) continue;
                    var col = handle.GetComponent<Collider>();
                    if (!col) continue;
                    if (col.Raycast(ray, out var hit, 1000f))
                    {
                        if (hit.distance < closest)
                        {
                            closest = hit.distance;
                            hitAxis = axis;
                        }
                    }
                }

                if (hitAxis >= 0)
                {
                    StartDrag(hitAxis, cam, mousePos);
                    return;
                }
            }
        }
        else if (CurrentTool == TransformTool.Rotate && _selected)
        {
            var distToTarget = Vector3.Distance(cam.transform.position, _selected.transform.position);
            var scale = Mathf.Max(MIN_ARROW_LENGTH, distToTarget * ARROW_SCALE_FACTOR);
            
            var closest = float.MaxValue;
            int hitAxis = -1;
            for (int axis = 0; axis < 3; axis++)
            {
                var axisDir = GetAxisDirection(axis);
                var plane = new Plane(axisDir, _selected.transform.position);
                
                if (plane.Raycast(ray, out float enter))
                {
                    var hitPoint = ray.GetPoint(enter);
                    var dFromCenter = Vector3.Distance(hitPoint, _selected.transform.position);
                    var thickness = scale * 0.15f;
                    
                    if (Mathf.Abs(dFromCenter - scale) < thickness)
                    {
                        if (enter < closest)
                        {
                            closest = enter;
                            hitAxis = axis;
                        }
                    }
                }
            }

            if (hitAxis >= 0)
            {
                StartDrag(hitAxis, cam, mousePos);
                return;
            }
        }

        if (Physics.Raycast(ray, out var worldHit, Mathf.Infinity, MiscEUtil.defaultMask))
            Select(worldHit.transform.gameObject);
        else Deselect();
        
    }

    private void Select(GameObject obj)
    {
        if (_selected == obj) return;
        Deselect();

        _selected = obj;

        var renderer = _selected.GetComponent<Renderer>();
        if (renderer && renderer.material)
        {
            _hadOriginalColor = true;
            _originalColor = renderer.material.color;
            renderer.material.color = Color.Lerp(_originalColor, Color.cyan, 0.3f);
        }
        BuildGizmo();
    }

    private void Deselect()
    {
        if (_selected)
        {
            if (_hadOriginalColor)
            {
                var renderer = _selected.GetComponent<Renderer>();
                if (renderer && renderer.material)
                    renderer.material.color = _originalColor;
            }
            _hadOriginalColor = false;
            _selected = null;
        }
        DestroyGizmo();
        EmberModeUIInfo.SetToolInfo("Transform Mode — Click to select");
    }

    private void BuildGizmo()
    {
        DestroyGizmo();
        if (!_selected) return;

        _gizmoRoot = new GameObject("EmberModeGizmoRoot");
        DontDestroyOnLoad(_gizmoRoot);
        _gizmoRoot.transform.position = _selected.transform.position;

        _xHandle = BuildAxisCollider(Vector3.right, "X");
        _yHandle = BuildAxisCollider(Vector3.up, "Y");
        _zHandle = BuildAxisCollider(Vector3.forward, "Z");
    }

    private GameObject BuildAxisCollider(Vector3 direction, string name)
    {
        var axisObj = new GameObject($"GizmoCollider_{name}");
        axisObj.transform.SetParent(_gizmoRoot.transform, false);
        axisObj.layer = GIZMO_LAYER;
        
        var col = axisObj.AddComponent<CapsuleCollider>();
        col.isTrigger = true;
        col.radius = HANDLE_COLLIDER_RADIUS;
        col.height = 1f + TIP_LENGTH * 2f;

        if (direction == Vector3.right) { col.direction = 0; col.center = direction * 0.5f; }
        else if (direction == Vector3.up) { col.direction = 1; col.center = direction * 0.5f; }
        else { col.direction = 2; col.center = direction * 0.5f; }

        return axisObj;
    }

    private void DestroyGizmo()
    {
        if (_gizmoRoot) Destroy(_gizmoRoot);
        _gizmoRoot = null;
        _xHandle = null;
        _yHandle = null;
        _zHandle = null;
        _hoveredAxis = -1;
    }

    private Vector3 _dragStartObjScale;
    private Quaternion _dragStartObjRot;
    private Vector3 _dragStartHitDir;

    private void StartDrag(int axis, Camera cam, Vector2 mousePos)
    {
        if (!_selected) return;
        _isDragging = true;
        _dragAxis = axis;
        _dragStartObjPos = _selected.transform.position;
        _dragStartObjScale = _selected.transform.localScale;
        _dragStartObjRot = _selected.transform.rotation;

        var axisDir = GetAxisDirection(axis);
        var ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));

        if (CurrentTool == TransformTool.Rotate)
        {
            _dragPlane = new Plane(axisDir, _dragStartObjPos);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                _dragStartHitDir = (hitPoint - _dragStartObjPos).normalized;
            }
        }
        else
        {
            var camForward = cam.transform.forward;
            var cross = Vector3.Cross(axisDir, camForward);
            if (cross.sqrMagnitude < 0.001f)
                cross = Vector3.Cross(axisDir, cam.transform.up);
            var planeNormal = Vector3.Cross(axisDir, cross).normalized;

            _dragPlane = new Plane(planeNormal, _dragStartObjPos);

            if (_dragPlane.Raycast(ray, out float enter))
                _dragStartWorldPos = ray.GetPoint(enter);
            else
                _dragStartWorldPos = _dragStartObjPos;
        }
    }

    private void HandleDrag(Camera cam)
    {
        if (!_selected || _dragAxis < 0) return;

        var mousePos = Mouse.current.position.ReadValue();
        var ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));

        if (CurrentTool == TransformTool.Rotate)
        {
            if (_dragPlane.Raycast(ray, out float enter))
            {
                var hitPoint = ray.GetPoint(enter);
                var currentDir = (hitPoint - _dragStartObjPos).normalized;
                var axisDir = GetAxisDirection(_dragAxis);
                var angle = Vector3.SignedAngle(_dragStartHitDir, currentDir, axisDir);
                _selected.transform.rotation = Quaternion.AngleAxis(angle, axisDir) * _dragStartObjRot;
            }
        }
        else
        {
            if (_dragPlane.Raycast(ray, out float enter))
            {
                var currentWorldPos = ray.GetPoint(enter);
                var delta = currentWorldPos - _dragStartWorldPos;

                var axisDir = GetAxisDirection(_dragAxis);
                var projectedDelta = Vector3.Dot(delta, axisDir) * axisDir;

                if (CurrentTool == TransformTool.Move)
                {
                    var newPos = _dragStartObjPos + projectedDelta;
                    var dist = Vector3.Distance(cam.transform.position, newPos);
                    var maxDist = cam.farClipPlane * 0.95f;
                    
                    if (dist > maxDist)
                    {
                        newPos = cam.transform.position + (newPos - cam.transform.position).normalized * maxDist;
                    }

                    _selected.transform.position = newPos;
                }
                else if (CurrentTool == TransformTool.Scale)
                {
                    var dragDist = Vector3.Dot(delta, axisDir);
                    var scaleDelta = Vector3.zero;
                    if (_dragAxis == 0) scaleDelta = new Vector3(dragDist, 0, 0);
                    if (_dragAxis == 1) scaleDelta = new Vector3(0, dragDist, 0);
                    if (_dragAxis == 2) scaleDelta = new Vector3(0, 0, dragDist);
                    
                    _selected.transform.localScale = _dragStartObjScale + scaleDelta;
                }
            }
        }
    }

    private void StopDrag()
    {
        _isDragging = false;
        _dragAxis = -1;
    }

    private void UpdateToolInfo()
    {
        if (!_selected)
        {
            EmberModeUIInfo.SetToolInfo("Transform Mode — Click to select");
            return;
        }

        var pos = _selected.transform.position;
        var info = $"Selected: {_selected.name}\nPosition: {pos.x:F2} {pos.y:F2} {pos.z:F2}";
        if (_isDragging)
            info += $"\nDragging: {GetAxisName(_dragAxis)}";
        EmberModeUIInfo.SetToolInfo(info);
    }

    private GameObject GetHandle(int axis) => axis switch
    {
        0 => _xHandle,
        1 => _yHandle,
        2 => _zHandle,
        _ => null
    };

    private Vector3 GetAxisDirection(int axis)
    {
        if (!_selected) return axis switch { 0 => Vector3.right, 1 => Vector3.up, 2 => Vector3.forward, _ => Vector3.zero };
        return axis switch
        {
            0 => _selected.transform.right,
            1 => _selected.transform.up,
            2 => _selected.transform.forward,
            _ => Vector3.zero
        };
    }

    private static string GetAxisName(int axis) => axis switch
    {
        0 => "X",
        1 => "Y",
        2 => "Z",
        _ => "?"
    };
}