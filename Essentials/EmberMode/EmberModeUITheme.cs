using Starlight.UI;
using UnityEngine.UI;

namespace Starlight.EmberMode;

internal static class EmberModeUITheme
{
    internal static readonly Color BarBackground = new(0.118f, 0.118f, 0.118f, 1f);
    internal static readonly Color PanelBackground = new(0.145f, 0.145f, 0.149f, 1f);
    internal static readonly Color HoverBackground = new(0.176f, 0.176f, 0.188f, 1f);
    internal static readonly Color PressedBackground = new(0.035f, 0.278f, 0.443f, 1f);
    
    internal static readonly Color SeparatorColor = new(0.247f, 0.247f, 0.275f, 1f);
    
    internal static readonly Color TextColor = new(0.831f, 0.831f, 0.831f, 1f);
    internal static readonly Color TextDimColor = new(0.612f, 0.863f, 0.996f, 1f);
    
    internal static readonly Color AccentColor = new(0.337f, 0.612f, 0.839f, 1f);
    internal static readonly Color AccentAltColor = new(0.306f, 0.788f, 0.690f, 1f);
    
    internal static readonly Color WarningColor = new(0.957f, 0.278f, 0.278f, 1f);
    internal static readonly Color BadgeColor = new(0.000f, 0.478f, 0.800f, 1f);

    internal static UITheme Create()
    {
        return new UITheme
        {
            ThemeName = "EmberModeDark",
            PrimaryColor = BarBackground,
            SecondaryColor = PanelBackground,
            AccentColor = AccentColor,
            AccentAlternateColor = AccentAltColor,
            Space3DBackgroundColor = new Color(0.08f, 0.08f, 0.08f, 1f),
            BadgeColor = BadgeColor,
            TextCategoryColor = AccentColor,
            TextCategoryAlternateColor = AccentAltColor,
            TextWarningColor = WarningColor,
            TextGeneralColor = TextColor,
            TextButtonColor = TextColor,

            ButtonColors = new ColorBlock
            {
                fadeDuration = 0.08f,
                colorMultiplier = 1f,
                normalColor = HoverBackground,
                highlightedColor = new Color(0.220f, 0.220f, 0.247f, 1f),
                pressedColor = PressedBackground,
                selectedColor = PressedBackground,
                disabledColor = new Color(0.247f, 0.247f, 0.275f, 0.5f),
            },

            AlternativeButtonColors = new ColorBlock
            {
                fadeDuration = 0.08f,
                colorMultiplier = 1f,
                normalColor = new Color(0.180f, 0.310f, 0.430f, 1f),
                highlightedColor = new Color(0.210f, 0.370f, 0.520f, 1f),
                pressedColor = PressedBackground,
                selectedColor = PressedBackground,
                disabledColor = new Color(0.200f, 0.200f, 0.220f, 0.5f),
            },

            GrayButtonColors = new ColorBlock
            {
                fadeDuration = 0.08f,
                colorMultiplier = 1f,
                normalColor = new Color(0.250f, 0.250f, 0.250f, 1f),
                highlightedColor = new Color(0.320f, 0.320f, 0.320f, 1f),
                pressedColor = new Color(0.190f, 0.190f, 0.190f, 1f),
                selectedColor = new Color(0.190f, 0.190f, 0.190f, 1f),
                disabledColor = new Color(0.200f, 0.200f, 0.200f, 0.5f),
            },

            UnityButtonColors = new ColorBlock
            {
                fadeDuration = 0.08f,
                colorMultiplier = 1f,
                normalColor = Color.white,
                highlightedColor = Color.white,
                pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f),
                selectedColor = Color.white,
                disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.5f),
            }
        };
    }
}