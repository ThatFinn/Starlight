using Starlight.Enums;
using UnityEngine.UI;

namespace Starlight.UI;

public class UITheme
{
    internal static UITheme GetTheme(StarlightMenuTheme theme)
    {
        switch (theme)
        {
            case StarlightMenuTheme.Native: return NativeTheme();
            case StarlightMenuTheme.Black: return BlackTheme();
            case StarlightMenuTheme.Melon: return MelonTheme();
        }
        return StarlightTheme();
    }

    static UITheme StarlightTheme()
    {
        var theme = new UITheme();
        theme.ThemeName = "Starlight";
        return theme;
    }
    
    static UITheme MelonTheme()
    {
        var theme = new UITheme();
        theme.ThemeName = "Starlight";
        theme.AccentColor = theme.AccentAlternateColor;
        theme.TextCategoryColor = theme.TextCategoryAlternateColor;
        theme.ButtonColors = theme.AlternativeButtonColors;
        theme.BadgeColor = new(0.149f, 0.7176f, 0.3961f, 1);
        return theme;
    }
    static UITheme NativeTheme()
    {
        var theme = new UITheme();
        theme.ThemeName = "Native";
        theme.PrimaryColor = new(0.941f, 0.882f, 0.784f, 1f);
        theme.SecondaryColor = Color.white;
        theme.AccentColor = new(0.824f, 0.702f, 0.580f, 1f);
        theme.TextGeneralColor = Color.black;
        
        return theme;
    }
    
    static UITheme BlackTheme()
    {
        var theme = new UITheme();
        theme.ThemeName = "Black";
        theme.PrimaryColor = new(0f, 0f, 0f, 0.6f);
        theme.SecondaryColor = new(0f, 0f, 0f, 0.35f);
        theme.AccentColor = new(0f, 0f, 0f, 0.25f);
        theme.AutoCompleteBackgroundColor = new(0f, 0f, 0f, 0.098f);
        theme.AutoCompleteSelectedColor = new(1f, 0.827f, 0f, 0.47f);
        return theme;
    }
    
    public string ThemeName;
    public Color PrimaryColor = new(0.106f, 0.106f, 0.114f, 1f);
    public Color SecondaryColor = new(0.188f, 0.220f, 0.275f, 1f);
    public Color AccentColor = new(0.173f, 0.431f, 0.784f, 1f);
    public Color AccentAlternateColor = new(0.1098f, 0.5314f, 0.2157f, 1f);
    public Color Space3DBackgroundColor = new(0.12f, 0.12f, 0.12f, 1f);
    public Color BadgeColor = new (0.149f, 0.7176f, 0.7961f, 1f);
    public Color TextCategoryColor = new(0.3506f, 0.4996f, 1f, 1f);
    public Color TextCategoryAlternateColor = new(0.1098f, 0.5314f, 0.2157f, 1f);
    public Color TextWarningColor = new(1,0,0,1);
    public Color TextGeneralColor = Color.white;
    public Color TextButtonColor = Color.white;
    public Color AutoCompleteBackgroundColor = new(0f, 0f, 0f, 0.098f);
    public Color AutoCompleteSelectedColor = new(1f, 0.827f, 0f, 0.47f);

    public ColorBlock ButtonColors = new ()
    {
        fadeDuration = 0.1f, colorMultiplier = 1f,
        disabledColor = new (0.8706f,0.3098f,0.5216f,1f),
        selectedColor = new (0.8706f, 0.3098f, 0.5216f, 1f),
        pressedColor = new  (0.1371f, 0.5248f, 0.6792f, 1f),
        highlightedColor = new (0.1098f, 0.2314f,0.4157f,1f),
        normalColor = new (0.149f, 0.7176f, 0.7961f, 1f)
    };
    public ColorBlock AlternativeButtonColors = new ()
    {
        fadeDuration = 0.1f, colorMultiplier = 1f,
        disabledColor = new (0.5571f, 0.0548f, 0.1192f, 1f),
        selectedColor = new (0.5571f, 0.0548f, 0.1192f, 1f),
        pressedColor = new (0.1371f, 0.7248f, 0.3792f, 1f),
        highlightedColor = new (0.1098f, 0.6314f, 0.2157f, 1),
        normalColor = new (0.149f, 0.7176f, 0.3961f, 1)
    };
    public ColorBlock GrayButtonColors = new ()
    {
        fadeDuration = 0.1f, colorMultiplier = 1f,
        disabledColor = new (0.6f, 0.6f, 0.6f, 1),
        selectedColor = new (0.6f, 0.6f, 0.6f, 1),
        pressedColor = new (0.3f, 0.3f, 0.3f, 1),
        highlightedColor = new Color(0.7f, 0.7f, 0.7f, 1),
        normalColor = new (0.5f, 0.5f, 0.5f, 1),
    };
    public ColorBlock UnityButtonColors = new ()
    {
        fadeDuration = 0.1f, colorMultiplier = 1f,
        disabledColor = new Color32(200,200,200, 128),
        selectedColor = Color.white,
        pressedColor = new Color32(200,200,200, 255),
        highlightedColor = Color.white,
        normalColor = Color.white,
    };
    public Color GetColor(UIColor color)
    {
        switch (color)
        {
            case UIColor.Primary: return PrimaryColor;
            case UIColor.Secondary: return SecondaryColor;
            case UIColor.Accent: return AccentColor;
            case UIColor.AccentAlternate: return AccentAlternateColor;
            case UIColor.TextGeneral: return TextGeneralColor;
            case UIColor.TextButton: return TextButtonColor;
            case UIColor.TextCategory: return TextCategoryColor;
            case UIColor.TextCategoryAlternate: return TextCategoryAlternateColor;
            case UIColor.TextWarning: return TextWarningColor;
            case UIColor.Badge: return BadgeColor;
            case UIColor.Space3DBackground: return Space3DBackgroundColor;
            case UIColor.AutoCompleteBackground: return AutoCompleteBackgroundColor;
            case UIColor.AutoCompleteSelected: return AutoCompleteSelectedColor;
            case UIColor.None: return Color.white;
            case UIColor.Transparent: return new Color(0f, 0f, 0f, 0f);
            default: return PrimaryColor;
        }
    }
    public ColorBlock GetColorBlock(UIColorBlock color)
    {
        switch (color)
        {
            case UIColorBlock.Buttons: return ButtonColors;
            case UIColorBlock.AlternativeButtons: return AlternativeButtonColors;
            case UIColorBlock.UnityButtons: return UnityButtonColors;
            case UIColorBlock.GrayButtons: return GrayButtonColors;
            case UIColorBlock.White:
            {
                var block = ColorBlock.defaultColorBlock;
                block.disabledColor = Color.white;
                block.selectedColor = Color.white;
                block.pressedColor = Color.white;
                block.highlightedColor = Color.white;
                block.normalColor = Color.white;
                return block;
            }
            case UIColorBlock.None:
            {
                var block = ColorBlock.defaultColorBlock;
                block.disabledColor = new Color(0.78431372549f,0.78431372549f,0.78431372549f,0.501960784314f);
                block.selectedColor = Color.white;
                block.pressedColor = new Color(0.78431372549f,0.78431372549f,0.78431372549f,1f);
                block.highlightedColor = Color.white;
                block.normalColor = Color.white;
                return block;
            }
            case UIColorBlock.Transparent:
            {
                var block = ColorBlock.defaultColorBlock;
                block.disabledColor = new Color(0f, 0f, 0f, 0f);
                block.selectedColor = new Color(0f, 0f, 0f, 0f);
                block.pressedColor = new Color(0f, 0f, 0f, 0f);
                block.highlightedColor = new Color(0f, 0f, 0f, 0f);
                block.normalColor = new Color(0f, 0f, 0f, 0f);
                return block;
            }
            default: return ButtonColors;
        }
    }
}

public enum UIColor
{
    None=0,
    Transparent=10,
    Primary=20,
    Secondary=30,
    Accent=40,
    AccentAlternate=41,
    TextGeneral=50,
    TextButton=60,
    TextCategory=70,
    TextCategoryAlternate=71,
    TextWarning=80,
    Badge=90,
    Space3DBackground=100,
    AutoCompleteBackground=110,
    AutoCompleteSelected=120,
}
public enum UIColorBlock
{
    None=0,
    Transparent=10,
    Buttons=20,
    AlternativeButtons=30,
    GrayButtons=40,
    White=50,
    UnityButtons=60,
}