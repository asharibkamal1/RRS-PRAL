using MudBlazor;

namespace PralPer.Web.Theme;

/// <summary>Central MudBlazor theme tuned to the PRAL PER Figma palette.</summary>
public static class PralTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#2563EB",
            Secondary = "#0EA5E9",
            Tertiary = "#7C3AED",
            Success = "#16A34A",
            Warning = "#D97706",
            Error = "#DC2626",
            Info = "#2563EB",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#0F172A",
            Background = "#F8FAFC",
            Surface = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#334155",
            TextPrimary = "#0F172A",
            TextSecondary = "#64748B"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px",
            DrawerWidthLeft = "260px"
        }
    };
}
