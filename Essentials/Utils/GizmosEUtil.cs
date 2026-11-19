namespace Starlight.Utils;

public static class GizmosEUtil
{
    private static System.Action<UnityEngine.Rendering.ScriptableRenderContext, Camera> _onEndCameraRendering;
    private static readonly List<System.Action<Camera>> _drawCommands = new();
    private static int _lastFrame = -1;

    private static readonly Color XColor = new(2f, 0f, 0f, 1f);
    private static readonly Color YColor = new(0f, 2f, 0f, 1f);
    private static readonly Color ZColor = new(0f, 0f, 2f, 1f);
    private static readonly Color XColorHover = new(1f, 0.4f, 0.4f, 1f);
    private static readonly Color YColorHover = new(0.4f, 1f, 0.4f, 1f);
    private static readonly Color ZColorHover = new(0.4f, 0.4f, 1f, 1f);
    
    private static void EnsureHook()
    {
        if (_onEndCameraRendering == null)
        {
            _onEndCameraRendering = (context, camera) =>
            {
                foreach (var cmd in _drawCommands)
                    cmd?.Invoke(camera);
            };
            UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += _onEndCameraRendering;
        }

        if (Time.frameCount != _lastFrame)
        {
            _drawCommands.Clear();
            _lastFrame = Time.frameCount;
        }
    }

    private static Material _glMaterial;

    public static void EnsureGLMaterial()
    {
        if (!_glMaterial)
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader)
            {
                _glMaterial = new Material(shader);
                _glMaterial.hideFlags = HideFlags.HideAndDontSave;
                _glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _glMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _glMaterial.SetInt("_ZWrite", 0);
                _glMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            }
        }
    }

    public static bool BeginGizmoPass(Camera camera)
    {
        EnsureGLMaterial();
        if (!_glMaterial) return false;

        GL.PushMatrix();
        GL.LoadProjectionMatrix(camera.projectionMatrix);
        GL.modelview = camera.worldToCameraMatrix;

        _glMaterial.SetPass(0);
        return true;
    }

    public static void EndGizmoPass()
    {
        GL.PopMatrix();
    }

    public static void DrawMoveGizmo(Vector3 pos, Quaternion rot, float scale, int hoveredAxis, Camera camera)
    {
        EnsureHook();
        _drawCommands.Add((cam) => {
            if (camera && cam != camera) return;
            if (!BeginGizmoPass(cam)) return;

            var right = rot * Vector3.right;
            var up = rot * Vector3.up;
            var forward = rot * Vector3.forward;

            DrawSolidArrow(pos, right, hoveredAxis == 0 ? XColorHover : XColor, scale, cam);
            DrawSolidArrow(pos, up, hoveredAxis == 1 ? YColorHover : YColor, scale, cam);
            DrawSolidArrow(pos, forward, hoveredAxis == 2 ? ZColorHover : ZColor, scale, cam);

            EndGizmoPass();
        });
    }

    public static void DrawScaleGizmo(Vector3 pos, Quaternion rot, float scale, int hoveredAxis, Camera camera)
    {
        EnsureHook();
        _drawCommands.Add((cam) => {
            if (camera && cam != camera) return;
            if (!BeginGizmoPass(cam)) return;

            var right = rot * Vector3.right;
            var up = rot * Vector3.up;
            var forward = rot * Vector3.forward;

            DrawSolidScale(pos, right, hoveredAxis == 0 ? XColorHover : XColor, scale, cam);
            DrawSolidScale(pos, up, hoveredAxis == 1 ? YColorHover : YColor, scale, cam);
            DrawSolidScale(pos, forward, hoveredAxis == 2 ? ZColorHover : ZColor, scale, cam);

            EndGizmoPass();
        });
    }

    public static void DrawRotateGizmo(Vector3 pos, Quaternion rot, float scale, int hoveredAxis, Camera camera)
    {
        EnsureHook();
        _drawCommands.Add((cam) => {
            if (camera && cam != camera) return;
            if (!BeginGizmoPass(cam)) return;

            var right = rot * Vector3.right;
            var up = rot * Vector3.up;
            var forward = rot * Vector3.forward;

            DrawSolidRing(pos, right, hoveredAxis == 0 ? XColorHover : XColor, scale, cam);
            DrawSolidRing(pos, up, hoveredAxis == 1 ? YColorHover : YColor, scale, cam);
            DrawSolidRing(pos, forward, hoveredAxis == 2 ? ZColorHover : ZColor, scale, cam);

            EndGizmoPass();
        });
    }

    public static void DrawSolidArrow(Vector3 pos, Vector3 dir, Color color, float size, Camera cam)
    {
        var lineLength = size;
        var lineWidth = size * 0.035f;
        var headLength = size * 0.18f;
        var headWidth = size * 0.08f;
        
        var end = pos + dir * lineLength;
        var basePos = end - dir * headLength;
        
        // line
        GL.Begin(GL.QUADS);
        GL.Color(color);
        var camDir = (cam.transform.position - pos).normalized;
        var cross = Vector3.Cross(dir, camDir).normalized;
        var offset = cross * lineWidth;
        
        GL.Vertex(pos - offset);
        GL.Vertex(pos + offset);
        GL.Vertex(basePos + offset);
        GL.Vertex(basePos - offset);
        GL.End();
        
        // arrow
        GL.Begin(GL.TRIANGLES);
        GL.Color(color);
        
        var right = Vector3.Cross(dir, Vector3.up).normalized;
        if (right.sqrMagnitude < 0.01f) right = Vector3.Cross(dir, Vector3.right).normalized;
        var up = Vector3.Cross(dir, right).normalized;
        
        var p1 = basePos + right * headWidth;
        var p2 = basePos - right * headWidth;
        var p3 = basePos + up * headWidth;
        var p4 = basePos - up * headWidth;
        
        // pyramid sides
        GL.Vertex(end); GL.Vertex(p1); GL.Vertex(p3);
        GL.Vertex(end); GL.Vertex(p3); GL.Vertex(p2);
        GL.Vertex(end); GL.Vertex(p2); GL.Vertex(p4);
        GL.Vertex(end); GL.Vertex(p4); GL.Vertex(p1);
        
        // pyramid base
        GL.Vertex(basePos); GL.Vertex(p3); GL.Vertex(p1);
        GL.Vertex(basePos); GL.Vertex(p2); GL.Vertex(p3);
        GL.Vertex(basePos); GL.Vertex(p4); GL.Vertex(p2);
        GL.Vertex(basePos); GL.Vertex(p1); GL.Vertex(p4);
        
        GL.End();
    }

    public static void DrawSolidScale(Vector3 pos, Vector3 dir, Color color, float size, Camera cam)
    {
        var lineLength = size;
        var lineWidth = size * 0.035f;
        var cubeSize = size * 0.12f;
        
        var end = pos + dir * lineLength;
        var basePos = end - dir * cubeSize;
        
        GL.Begin(GL.QUADS);
        GL.Color(color);
        var camDir = (cam.transform.position - pos).normalized;
        var cross = Vector3.Cross(dir, camDir).normalized;
        var offset = cross * lineWidth;
        
        GL.Vertex(pos - offset);
        GL.Vertex(pos + offset);
        GL.Vertex(basePos + offset);
        GL.Vertex(basePos - offset);
        GL.End();
        
        var right = Vector3.Cross(dir, Vector3.up).normalized;
        if (right.sqrMagnitude < 0.01f) right = Vector3.Cross(dir, Vector3.right).normalized;
        var up = Vector3.Cross(dir, right).normalized;
        
        var halfRight = right * cubeSize * 0.5f;
        var halfUp = up * cubeSize * 0.5f;
        
        var c1 = basePos + halfRight + halfUp;
        var c2 = basePos - halfRight + halfUp;
        var c3 = basePos - halfRight - halfUp;
        var c4 = basePos + halfRight - halfUp;
        
        var e1 = end + halfRight + halfUp;
        var e2 = end - halfRight + halfUp;
        var e3 = end - halfRight - halfUp;
        var e4 = end + halfRight - halfUp;
        
        GL.Begin(GL.QUADS);
        GL.Color(color);
        GL.Vertex(c1); GL.Vertex(c2); GL.Vertex(c3); GL.Vertex(c4);
        GL.Vertex(e4); GL.Vertex(e3); GL.Vertex(e2); GL.Vertex(e1);
        GL.Vertex(c1); GL.Vertex(e1); GL.Vertex(e2); GL.Vertex(c2);
        GL.Vertex(c2); GL.Vertex(e2); GL.Vertex(e3); GL.Vertex(c3);
        GL.Vertex(c3); GL.Vertex(e3); GL.Vertex(e4); GL.Vertex(c4);
        GL.Vertex(c4); GL.Vertex(e4); GL.Vertex(e1); GL.Vertex(c1);
        GL.End();
    }

    public static void DrawSolidRing(Vector3 pos, Vector3 normal, Color color, float radius, Camera cam)
    {
        GL.Begin(GL.QUADS);
        GL.Color(color);
        
        var right = Vector3.Cross(normal, Vector3.up).normalized;
        if (right.sqrMagnitude < 0.01f) right = Vector3.Cross(normal, Vector3.right).normalized;
        var up = Vector3.Cross(normal, right).normalized;

        var segments = 64;
        var thickness = radius * 0.04f;
        
        var prevInner = Vector3.zero;
        var prevOuter = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            var angle = i * Mathf.PI * 2f / segments;
            var dir = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle));
            var pInner = pos + dir * (radius - thickness);
            var pOuter = pos + dir * (radius + thickness);

            if (i > 0)
            {
                GL.Vertex(prevInner);
                GL.Vertex(prevOuter);
                GL.Vertex(pOuter);
                GL.Vertex(pInner);
            }
            prevInner = pInner;
            prevOuter = pOuter;
        }
        GL.End();
    }
}