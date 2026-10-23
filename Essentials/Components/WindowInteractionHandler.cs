using UnityEngine.InputSystem;
using Starlight.Storage;

namespace Starlight.Components;

[InjectIntoIL]
public class WindowInteractionHandler : MonoBehaviour
{
    public RectTransform windowRect;
    public RectTransform titleBar;
    public RectTransform resizeHandle;

    public bool draggable = true;
    public bool resizable = true;
    public Vector2 minSize = new (200, 150);

    private bool _isDragging;
    private bool _isResizing;
    private Vector2 _dragOffset;

    void Update()
    {
        if (Mouse.current == null) return;

        var mousePos = Mouse.current.position.ReadValue();
        var pressedThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
        var isPressed = Mouse.current.leftButton.isPressed;
        var releasedThisFrame = Mouse.current.leftButton.wasReleasedThisFrame;

        if (releasedThisFrame)
        {
            _isDragging = false;
            _isResizing = false;
        }

        if (pressedThisFrame)
        {
            if (resizable && resizeHandle && RectTransformUtility.RectangleContainsScreenPoint(resizeHandle, mousePos, null))
            {
                _isResizing = true;
            }
            else if (draggable && titleBar && RectTransformUtility.RectangleContainsScreenPoint(titleBar, mousePos, null))
            {
                _isDragging = true;
                var parentRect = windowRect.parent?.GetComponent<RectTransform>();
                if (parentRect)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mousePos, null, out Vector2 localPos);
                    _dragOffset = windowRect.anchoredPosition - localPos;
                }
            }
        }

        if (_isDragging && isPressed)
        {
            var parentRectDrag = windowRect.parent?.GetComponent<RectTransform>();
            if (parentRectDrag)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectDrag, mousePos, null, out Vector2 localPos);
                windowRect.anchoredPosition = localPos + _dragOffset;
            }
        }

        if (_isResizing && isPressed)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(windowRect, mousePos, null, out Vector2 localPos);

            float newWidth = Mathf.Max(minSize.x, localPos.x);
            float newHeight = Mathf.Max(minSize.y, -localPos.y);

            windowRect.sizeDelta = new Vector2(newWidth, newHeight);
        }
    }
}