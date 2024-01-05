using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace SteamVR_ExConfig;

public enum ThemeKeys
{
    Background,
    Foreground,
    DisabledButton
}

public partial class MainForm
{
    private static Dictionary<ThemeKeys, Color> s_LightThemeColors = new Dictionary<ThemeKeys, Color>
    {
        { ThemeKeys.Background, SystemColors.Control },
        { ThemeKeys.Foreground, SystemColors.ControlText },
        { ThemeKeys.DisabledButton, SystemColors.ControlDark },
    };

    private static Dictionary<ThemeKeys, Color> s_DarkThemeColors = new Dictionary<ThemeKeys, Color>
    {
        { ThemeKeys.Background, SystemColors.ControlText },
        { ThemeKeys.Foreground, SystemColors.Control },
        { ThemeKeys.DisabledButton, SystemColors.ControlDark },
    };

    private void SetTheme( bool dark )
    {
        var themeColors = dark ? s_DarkThemeColors : s_LightThemeColors;

        Queue<Control> controls = new();

        controls.Enqueue( this );

        while ( controls.TryDequeue( out Control? control ) )
        {
            if ( control is null )
                continue;

            // Change this current control's theme
            control.BackColor = themeColors[ThemeKeys.Background];
            control.ForeColor = themeColors[ThemeKeys.Foreground];

            // Walk down the tree
            if ( control.Controls is null )
                continue;

            foreach ( Control child in control.Controls )
            {
                controls.Enqueue( child );
            }
        }
    }

    private void SetButtonTheme( Button button, bool dark, bool activated )
    {
        var themeColors = dark ? s_DarkThemeColors : s_LightThemeColors;
        var color = activated ? themeColors[ThemeKeys.Foreground] : themeColors[ThemeKeys.DisabledButton];

        button.ForeColor = color;
    }
}
