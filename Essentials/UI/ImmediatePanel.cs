namespace Starlight.UI;

/// <summary>
/// A layout for panels drawn with GUI
/// </summary>
public sealed class ImmediatePanel
{
    /// <summary>
    /// The focus control id of the field being typed into
    /// </summary>
    public static string Focused { get; set; } = string.Empty;

    /// <summary>
    /// True when anything anywhere at all has the keyboard
    /// </summary>
    public static bool Typing => !string.IsNullOrEmpty(Focused);

    /// <summary>
    /// Unfocuses the currently focused thing 
    /// </summary>
    /// <remarks> This only clears the focus on the panel </remarks>
    public static void Unfocus()
    {
        Focused = string.Empty;
        framesUnclaimed = 0;
    }

    /// <summary>
    /// Focuses a field by id
    /// </summary>
    public static void Focus(string id)
    {
        Focused         = id ?? string.Empty;
        focusClaimed    = true;
        framesUnclaimed = 0;
    }

    private static bool held;
    private static bool focusClaimed;
    private static int framesUnclaimed;

    /// <summary>
    /// How many update passes a focused field can go undrawn before focus is dropped
    /// </summary>
    private const int UnclaimedFrameLimit = 3;

    /// <summary>
    /// Prevents the game from readding the keyboard while a field has focus
    /// </summary>
    public static void HoldGameInput()
    {
        if (Typing)
        {
            if (focusClaimed) framesUnclaimed = 0;
            else framesUnclaimed++;

            focusClaimed = false;

            if (framesUnclaimed > UnclaimedFrameLimit) Unfocus();
        }
        else framesUnclaimed = 0;

        try
        {
            var map = gameContext?.InputDirector?._mainGame?.Map;

            if (map == null)
            {
                held = false;
                return;
            }

            if (Typing && map.enabled)
            {
                map.Disable();
                held = true;
            }
            else if (!Typing && held)
            {
                map.Enable();
                held = false;
            }
        }
        catch
        {
            held = false;
        }
    }

    /// <summary>
    /// Height of one label row
    /// </summary>
    public float RowHeight { get; set; } = 22f;

    /// <summary>
    /// Height of one button
    /// </summary>
    public float ButtonHeight { get; set; } = 24f;
    
    // I think padding is always on the inside and margin on the outside of boxes
    /// <summary>
    /// Padding (inside the box)
    /// </summary>
    public float Inset { get; set; } = 16f;

    private float left;
    private float top;
    private float width;
    private float cursor;

    /// <summary>
    /// How tall the content drawn is
    /// </summary>
    /// <remarks> since <see cref="Begin"/> only </remarks>
    public float Height => cursor - top;

    /// <summary>
    /// The usable width inside the box
    /// </summary>
    public float Width => width - Inset;

    /// <summary>
    /// Starts laying out inside a box, six pixels in from its corner
    /// </summary>
    public void Begin(Rect box)
    {
        left   = box.x + 6f;
        top    = box.y + 6f;
        width  = box.width;
        cursor = top;
    }

    /// <summary>
    /// Claims a rectangle of the given height and jumps past it
    /// </summary>
    public Rect Row(float height)
    {
        var rect = new Rect(left, cursor, Width, height);

        cursor += height + 2f;

        return rect;
    }

    /// <summary>
    /// Draws a line of text
    /// </summary>
    public void Line(string text)
    {
        GUI.Label(new Rect(left, cursor, Width, RowHeight), text);
        cursor += RowHeight;
    }

    /// <summary>
    /// Leaves a specified amount of space
    /// </summary>
    public void Space(float by) => cursor += by;

    /// <summary>
    /// Splits the next row into evenly spaced "cells"
    /// </summary>
    /// <remarks> Useful for buttons </remarks>
    public Rect[] Cells(int count, float height, float gap = 4f)
    {
        if (count <= 0) return [];

        var row = Row(height);
        var cell = (row.width - gap * (count - 1)) / count;

        var cells = new Rect[count];

        for (var i = 0; i < count; i++)
            cells[i] = new Rect(row.x + i * (cell + gap), row.y, cell, height);

        return cells;
    }

    /// <summary>
    /// A button that shows whether it is on or not
    /// </summary>
    /// <returns> True when it was pressed </returns>
    public static bool Toggle(Rect at, string label, bool on, Color highlight)
    {
        var was = GUI.color;

        if (on) GUI.color = highlight;

        var pressed = GUI.Button(at, label);

        GUI.color = was;

        return pressed;
    }

    /// <summary>
    /// A text field that works without <c>GUILayout</c>.
    /// </summary>
    /// <param name="id"> Unique identity for focus control </param>
    public static string Field(string id, Rect at, string value, string placeholder)
    {
        var focused = Focused == id;

        if (focused) focusClaimed = true;

        var shown = string.IsNullOrEmpty(value) && !focused ? placeholder : value;
        var was = GUI.color;

        if (focused) GUI.color = new Color(1f, 1f, 0.72f);

        GUI.Box(at, shown);
        GUI.color = was;

        var current = Event.current;

        if (current.type == EventType.MouseDown)
        {
            if (at.Contains(current.mousePosition))
            {
                Focused = id;
                current.Use();
            }
            else if (focused)
                Unfocus();

            return value;
        }

        if (!focused || current.type != EventType.KeyDown) return value;

        switch (current.keyCode)
        {
            case KeyCode.Backspace:
                current.Use();
                return value.Length > 0 ? value[..^1] : value;

            case KeyCode.Return:
            case KeyCode.KeypadEnter:
            case KeyCode.Escape:
                Unfocus();
                current.Use();
                return value;

            case KeyCode.V when current.control:
                current.Use();
                return value + GUIUtility.systemCopyBuffer;
        }
        
        if (current.character < ' ' || current.character == (char)0x7F) return value;

        current.Use();

        return value + current.character;
    }
}