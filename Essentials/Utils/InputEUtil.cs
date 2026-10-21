using Starlight.Components;
using Starlight.Enums;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Starlight.Utils;

public static class InputEUtil
{
    
    /// <summary>
    /// Returns true if keycode is currently pressed
    /// </summary>
    public static bool OnKey(this KeyCode key)
    {
        if (!KeyCodeInputAcquirer.Instance) return false;
        return KeyCodeInputAcquirer.Instance.OnKey(key);
    }
    /// <summary>
    /// Returns true if key is currently pressed
    /// </summary>
    public static bool OnKey(this LKey key)
    {
        if (!LKeyInputAcquirer.Instance) return false;
        return LKeyInputAcquirer.Instance.OnKey(key);
    }
    /// <summary>
    /// Returns true if all keys are currently pressed
    /// </summary>
    public static bool OnKey(this LMultiKey multiKey)
    {
        if (!LKeyInputAcquirer.Instance) return false;
        return LKeyInputAcquirer.Instance.OnKey(multiKey);
    }
    
    
    /// <summary>
    /// Returns true the **first frame** when the keycode become pressed
    /// </summary>
    public static bool OnKeyDown(this KeyCode key)
    {
        if (!KeyCodeInputAcquirer.Instance) return false;
        return KeyCodeInputAcquirer.Instance.OnKeyDown(key);
    }
    /// <summary>
    /// Returns true the **first frame** when the key become pressed
    /// </summary>
    public static bool OnKeyDown(this LKey key)
    {
        if (!LKeyInputAcquirer.Instance) return false;
        return LKeyInputAcquirer.Instance.OnKeyDown(key);
    }
    /// <summary>
    /// Returns true the **first frame** when all keys become pressed
    /// </summary>
    public static bool OnKeyDown(this LMultiKey multiKey)
    {
        if (!LKeyInputAcquirer.Instance) return false;
        return LKeyInputAcquirer.Instance.OnKeyDown(multiKey);
    }

    
    
    
    /// <summary>
    /// Returns true the **first frame** when the keycode is released it was pressed
    /// </summary>
    public static bool OnKeyUp(this KeyCode key)
    {
        if (!KeyCodeInputAcquirer.Instance) return false;
        return KeyCodeInputAcquirer.Instance.OnKeyUp(key);
    }
    /// <summary>
    /// Returns true the **first frame** when the key is released it was pressed
    /// </summary>
    public static bool OnKeyUp(this LKey key)
    {
        if (!LKeyInputAcquirer.Instance) return false;
        return LKeyInputAcquirer.Instance.OnKeyUp(key);
    }
    /// <summary>
    /// Returns true the **first frame** when any key is released after the combination was pressed
    /// </summary>
    public static bool OnKeyUp(this LMultiKey multiKey)
    {
        if (!LKeyInputAcquirer.Instance) return false;
        return LKeyInputAcquirer.Instance.OnKeyUp(multiKey);
    }
    
    

    internal static bool TryConvertEventToKey(Event e, out LKey key)
    {
        key = LKey.None;

        if (e.character != '\0')
        {
            char c = e.character;

            switch (c)
            {
                // Letters A–Z
                case 'A': case 'a': key = LKey.A; return true;
                case 'B': case 'b': key = LKey.B; return true;
                case 'C': case 'c': key = LKey.C; return true;
                case 'D': case 'd': key = LKey.D; return true;
                case 'E': case 'e': key = LKey.E; return true;
                case 'F': case 'f': key = LKey.F; return true;
                case 'G': case 'g': key = LKey.G; return true;
                case 'H': case 'h': key = LKey.H; return true;
                case 'I': case 'i': key = LKey.I; return true;
                case 'J': case 'j': key = LKey.J; return true;
                case 'K': case 'k': key = LKey.K; return true;
                case 'L': case 'l': key = LKey.L; return true;
                case 'M': case 'm': key = LKey.M; return true;
                case 'N': case 'n': key = LKey.N; return true;
                case 'O': case 'o': key = LKey.O; return true;
                case 'P': case 'p': key = LKey.P; return true;
                case 'Q': case 'q': key = LKey.Q; return true;
                case 'R': case 'r': key = LKey.R; return true;
                case 'S': case 's': key = LKey.S; return true;
                case 'T': case 't': key = LKey.T; return true;
                case 'U': case 'u': key = LKey.U; return true;
                case 'V': case 'v': key = LKey.V; return true;
                case 'W': case 'w': key = LKey.W; return true;
                case 'X': case 'x': key = LKey.X; return true;
                case 'Y': case 'y': key = LKey.Y; return true;
                case 'Z': case 'z': key = LKey.Z; return true;

                // Numbers
                case '0': key = LKey.Alpha0; return true;
                case '1': key = LKey.Alpha1; return true;
                case '2': key = LKey.Alpha2; return true;
                case '3': key = LKey.Alpha3; return true;
                case '4': key = LKey.Alpha4; return true;
                case '5': key = LKey.Alpha5; return true;
                case '6': key = LKey.Alpha6; return true;
                case '7': key = LKey.Alpha7; return true;
                case '8': key = LKey.Alpha8; return true;
                case '9': key = LKey.Alpha9; return true;

                // Symbols
                case '-': key = LKey.Minus; return true;
                case '=': key = LKey.Equals; return true;
                case '[': key = LKey.LeftBracket; return true;
                case ']': key = LKey.RightBracket; return true;
                case ';': key = LKey.Semicolon; return true;
                case '\'': key = LKey.Quote; return true;
                case ',': key = LKey.Comma; return true;
                case '.': key = LKey.Period; return true;
                case '/': key = LKey.Slash; return true;
                case '\\': key = LKey.Backslash; return true;
                case '+': key = LKey.Plus; return true;
                case '_': key = LKey.Underscore; return true;
                case ':': key = LKey.Colon; return true;
                case '!': key = LKey.Exclamation; return true;
                case '?': key = LKey.Question; return true;
                case '@': key = LKey.At; return true;
                case '#': key = LKey.Hash; return true;
                case '$': key = LKey.Dollar; return true;
                case '%': key = LKey.Percent; return true;
                case '^': key = LKey.Caret; return true;
                case '&': key = LKey.Ampersand; return true;
                case '*': key = LKey.Asterisk; return true;
                case '(': key = LKey.LeftParen; return true;
                case ')': key = LKey.RightParen; return true;
                case '{': key = LKey.LeftCurly; return true;
                case '}': key = LKey.RightCurly; return true;
                case '§': key = LKey.Paragraph; return true;
                case '°': key = LKey.Degree; return true;
                case '€': key = LKey.Euro; return true;
                case '<': key = LKey.Less; return true;
                case '>': key = LKey.Greater; return true;
                case '|': key = LKey.Pipe; return true;
                case '"': key = LKey.DoubleQuote; return true;
                case '`': key = LKey.BackQuote; return true;
                case '~': key = LKey.Tilde; return true;

                // Extended Latin
                case 'À': case 'à': key = LKey.À; return true;
                case 'Á': case 'á': key = LKey.Á; return true;
                case 'Â': case 'â': key = LKey.Â; return true;
                case 'Ã': case 'ã': key = LKey.Ã; return true;
                case 'Ä': case 'ä': key = LKey.Ä; return true;
                case 'Å': case 'å': key = LKey.Å; return true;
                case 'Æ': case 'æ': key = LKey.Æ; return true;
                case 'Ç': case 'ç': key = LKey.Ç; return true;
                case 'È': case 'è': key = LKey.È; return true;
                case 'É': case 'é': key = LKey.É; return true;
                case 'Ê': case 'ê': key = LKey.Ê; return true;
                case 'Ë': case 'ë': key = LKey.Ë; return true;
                case 'Ì': case 'ì': key = LKey.Ì; return true;
                case 'Í': case 'í': key = LKey.Í; return true;
                case 'Î': case 'î': key = LKey.Î; return true;
                case 'Ï': case 'ï': key = LKey.Ï; return true;
                case 'Ñ': case 'ñ': key = LKey.Ñ; return true;
                case 'Ò': case 'ò': key = LKey.Ò; return true;
                case 'Ó': case 'ó': key = LKey.Ó; return true;
                case 'Ô': case 'ô': key = LKey.Ô; return true;
                case 'Õ': case 'õ': key = LKey.Õ; return true;
                case 'Ö': case 'ö': key = LKey.Ö; return true;
                case 'Ø': case 'ø': key = LKey.Ø; return true;
                case 'Ù': case 'ù': key = LKey.Ù; return true;
                case 'Ú': case 'ú': key = LKey.Ú; return true;
                case 'Û': case 'û': key = LKey.Û; return true;
                case 'Ü': case 'ü': key = LKey.Ü; return true;
                case 'Ý': case 'ý': key = LKey.Ý; return true;
                case 'Ÿ': key = LKey.Ÿ; return true;
                case 'ß': key = LKey.ß; return true;

                // Cyrillic
                case 'А': case 'а': key = LKey.А; return true;
                case 'Б': case 'б': key = LKey.Б; return true;
                case 'В': case 'в': key = LKey.В; return true;
                case 'Г': case 'г': key = LKey.Г; return true;
                case 'Д': case 'д': key = LKey.Д; return true;
                case 'Е': case 'е': key = LKey.Е; return true;
                case 'Ё': case 'ё': key = LKey.Ё; return true;
                case 'Ж': case 'ж': key = LKey.Ж; return true;
                case 'З': case 'з': key = LKey.З; return true;
                case 'И': case 'и': key = LKey.И; return true;
                case 'Й': case 'й': key = LKey.Й; return true;
                case 'К': case 'к': key = LKey.К; return true;
                case 'Л': case 'л': key = LKey.Л; return true;
                case 'М': case 'м': key = LKey.М; return true;
                case 'Н': case 'н': key = LKey.Н; return true;
                case 'О': case 'о': key = LKey.О; return true;
                case 'П': case 'п': key = LKey.П; return true;
                case 'Р': case 'р': key = LKey.Р; return true;
                case 'С': case 'с': key = LKey.С; return true;
                case 'Т': case 'т': key = LKey.Т; return true;
                case 'У': case 'у': key = LKey.У; return true;
                case 'Ф': case 'ф': key = LKey.Ф; return true;
                case 'Х': case 'х': key = LKey.Х; return true;
                case 'Ц': case 'ц': key = LKey.Ц; return true;
                case 'Ч': case 'ч': key = LKey.Ч; return true;
                case 'Ш': case 'ш': key = LKey.Ш; return true;
                case 'Щ': case 'щ': key = LKey.Щ; return true;
                case 'Ъ': case 'ъ': key = LKey.Ъ; return true;
                case 'Ы': case 'ы': key = LKey.Ы; return true;
                case 'Ь': case 'ь': key = LKey.Ь; return true;
                case 'Э': case 'э': key = LKey.Э; return true;
                case 'Ю': case 'ю': key = LKey.Ю; return true;
                case 'Я': case 'я': key = LKey.Я; return true;

                // Hiragana
                // The fallback font doesnt support it
                
                 /*
                case 'あ': key = LKey.あ; return true; case 'い': key = LKey.い; return true;
                case 'う': key = LKey.う; return true; case 'え': key = LKey.え; return true;
                case 'お': key = LKey.お; return true; case 'か': key = LKey.か; return true;
                case 'き': key = LKey.き; return true; case 'く': key = LKey.く; return true;
                case 'け': key = LKey.け; return true; case 'こ': key = LKey.こ; return true;
                case 'さ': key = LKey.さ; return true; case 'し': key = LKey.し; return true;
                case 'す': key = LKey.す; return true; case 'せ': key = LKey.せ; return true;
                case 'そ': key = LKey.そ; return true; case 'た': key = LKey.た; return true;
                case 'ち': key = LKey.ち; return true; case 'つ': key = LKey.つ; return true;
                case 'て': key = LKey.て; return true; case 'と': key = LKey.と; return true;
                case 'な': key = LKey.な; return true; case 'に': key = LKey.に; return true;
                case 'ぬ': key = LKey.ぬ; return true; case 'ね': key = LKey.ね; return true;
                case 'の': key = LKey.の; return true; case 'は': key = LKey.は; return true;
                case 'ひ': key = LKey.ひ; return true; case 'ふ': key = LKey.ふ; return true;
                case 'へ': key = LKey.へ; return true; case 'ほ': key = LKey.ほ; return true;
                case 'ま': key = LKey.ま; return true; case 'み': key = LKey.み; return true;
                case 'む': key = LKey.む; return true; case 'め': key = LKey.め; return true;
                case 'も': key = LKey.も; return true; case 'や': key = LKey.や; return true;
                case 'ゆ': key = LKey.ゆ; return true; case 'よ': key = LKey.よ; return true;
                case 'ら': key = LKey.ら; return true; case 'り': key = LKey.り; return true;
                case 'る': key = LKey.る; return true; case 'れ': key = LKey.れ; return true;
                case 'ろ': key = LKey.ろ; return true; case 'わ': key = LKey.わ; return true;
                case 'を': key = LKey.を; return true; case 'ん': key = LKey.ん; return true;
                */
                
            }
        }

        // Non-character keys
        switch (e.keyCode)
        {
            case KeyCode.Space: key = LKey.Space; return true;
            case KeyCode.Return: key = LKey.Return; return true;
            case KeyCode.Escape: key = LKey.Escape; return true;
            case KeyCode.Tab: key = LKey.Tab; return true;
            case KeyCode.Backspace: key = LKey.Backspace; return true;

            case KeyCode.UpArrow: key = LKey.UpArrow; return true;
            case KeyCode.DownArrow: key = LKey.DownArrow; return true;
            case KeyCode.LeftArrow: key = LKey.LeftArrow; return true;
            case KeyCode.RightArrow: key = LKey.RightArrow; return true;
            case KeyCode.PageUp: key = LKey.PageUp; return true;
            case KeyCode.PageDown: key = LKey.PageDown; return true;
            case KeyCode.Home: key = LKey.Home; return true;
            case KeyCode.End: key = LKey.End; return true;
            case KeyCode.Insert: key = LKey.Insert; return true;
            case KeyCode.Delete: key = LKey.Delete; return true;

            case KeyCode.LeftShift: key = LKey.LeftShift; return true;
            case KeyCode.RightShift: key = LKey.RightShift; return true;
            case KeyCode.LeftControl: key = LKey.LeftControl; return true;
            case KeyCode.RightControl: key = LKey.RightControl; return true;
            case KeyCode.LeftAlt: key = LKey.LeftAlt; return true;
            case KeyCode.RightAlt: key = LKey.RightAlt; return true;

            // Function keys
            case KeyCode.F1: key = LKey.F1; return true;
            case KeyCode.F2: key = LKey.F2; return true;
            case KeyCode.F3: key = LKey.F3; return true;
            case KeyCode.F4: key = LKey.F4; return true;
            case KeyCode.F5: key = LKey.F5; return true;
            case KeyCode.F6: key = LKey.F6; return true;
            case KeyCode.F7: key = LKey.F7; return true;
            case KeyCode.F8: key = LKey.F8; return true;
            case KeyCode.F9: key = LKey.F9; return true;
            case KeyCode.F10: key = LKey.F10; return true;
            case KeyCode.F11: key = LKey.F11; return true;
            case KeyCode.F12: key = LKey.F12; return true;

            // Numpad
            case KeyCode.Keypad0: key = LKey.Keypad0; return true;
            case KeyCode.Keypad1: key = LKey.Keypad1; return true;
            case KeyCode.Keypad2: key = LKey.Keypad2; return true;
            case KeyCode.Keypad3: key = LKey.Keypad3; return true;
            case KeyCode.Keypad4: key = LKey.Keypad4; return true;
            case KeyCode.Keypad5: key = LKey.Keypad5; return true;
            case KeyCode.Keypad6: key = LKey.Keypad6; return true;
            case KeyCode.Keypad7: key = LKey.Keypad7; return true;
            case KeyCode.Keypad8: key = LKey.Keypad8; return true;
            case KeyCode.Keypad9: key = LKey.Keypad9; return true;
            case KeyCode.KeypadDivide: key = LKey.KeypadDivide; return true;
            case KeyCode.KeypadMultiply: key = LKey.KeypadMultiply; return true;
            case KeyCode.KeypadMinus: key = LKey.KeypadMinus; return true;
            case KeyCode.KeypadPlus: key = LKey.KeypadPlus; return true;
            case KeyCode.KeypadEnter: key = LKey.KeypadEnter; return true;
            case KeyCode.KeypadPeriod: key = LKey.KeypadPeriod; return true;

            // Backquote key fallback
            case KeyCode.BackQuote:
                key = e.shift ? LKey.Tilde : LKey.BackQuote;
                return true;
        }

        return false;
    }
    

    /// <summary>
    /// Returns true if mouse button is currently pressed
    /// </summary>
    public static bool OnMouse(this MouseButton button)
    {
        if (!KeyCodeInputAcquirer.Instance) return false;
        return KeyCodeInputAcquirer.Instance.OnMouse((int)button);
    }
    
    /// <summary>
    /// Returns true if mouse button was pressed this frame
    /// </summary>
    public static bool OnMouseDown(this MouseButton button)
    {
        if (!KeyCodeInputAcquirer.Instance) return false;
        return KeyCodeInputAcquirer.Instance.OnMouseDown((int)button);
    }
    
    /// <summary>
    /// Returns true if mouse button was released this frame
    /// </summary>
    public static bool OnMouseUp(this MouseButton button)
    {
        if (!KeyCodeInputAcquirer.Instance) return false;
        return KeyCodeInputAcquirer.Instance.OnMouseUp((int)button);
    }
    
    /// <summary>
    /// Returns true if key is currently pressed
    /// </summary>
    public static bool OnKey(this Key key) => key.ToKeyCode().OnKey();
    
    /// <summary>
    /// Returns true if key was pressed this frame
    /// </summary>
    public static bool OnKeyDown(this Key key) => key.ToKeyCode().OnKeyDown();
    
    /// <summary>
    /// Returns true if key was released this frame
    /// </summary>
    public static bool OnKeyUp(this Key key) => key.ToKeyCode().OnKeyUp();
}

            