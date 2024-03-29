﻿using MudBlazor;

namespace ContinuousPrimate.Blazor.Pages;

/// <summary>
/// MudBlazor Themes
/// </summary>
public static class Themes
{
    /// <summary>
    /// The Default Theme
    /// </summary>
    public static readonly MudTheme DefaultTheme = new();

    /// <summary>
    /// The Dark Mode Theme
    /// </summary>
    public static readonly MudTheme DarkTheme = new()
    {
        Palette = new Palette()
        {
            Black = "#0e1013",
            Background = "#32333d",
            BackgroundGrey = "#38424f",
            Surface = "#373740",
            DrawerBackground = "#27272f",
            DrawerText = "rgba(255,255,255, 0.50)",
            DrawerIcon = "rgba(255,255,255, 0.50)",
            AppbarBackground = "#27272f",
            AppbarText = "rgba(255,255,255, 0.70)",
            TextPrimary = "rgba(255,255,255, 0.70)",
            TextSecondary = "rgba(255,255,255, 0.50)",
            ActionDefault = "#adadb1",
            ActionDisabled = "rgba(255,255,255, 0.26)",
            ActionDisabledBackground = "rgba(255,255,255, 0.12)",
            Divider = "rgba(255,255,255, 0.12)",
            DividerLight = "rgba(255,255,255, 0.06)",
            TableLines = "rgba(255,255,255, 0.12)",
            LinesDefault = "rgba(255,255,255, 0.12)",
            LinesInputs = "rgba(255,255,255, 0.3)",
            TextDisabled = "rgba(255,255,255, 0.2)",
            Primary = "#34b38a",
            Secondary = "#76d7b8"
        }
    };

    //Colors:
    // #232931
    // #4ecca3
    // #38424f
    // #0e1013
    // #76d7b8
    // #34b38a
}