using MudBlazor;

namespace Vista.Models;

/// <summary>
/// Theme Resource
/// ref→[Default Theme](https://mudblazor.com/customization/default-theme)
/// </summary>
internal static class CustomThemeResource
{
  public static readonly MudTheme DefaultTheme = new MudTheme()
  {
    PaletteLight = new PaletteLight()
    {
      Primary = "#BF4690",
      PrimaryDarken = "#802F61",
      PrimaryLighten = "#ED58B4", //"#CC4B9B",
      Secondary = "#CA87B0", //"#939BFF", // "#47B8BF", // "#CA87B0",
      Tertiary = "#EFD1E4",
      TertiaryContrastText = "#424242",
      HoverOpacity = 0.16, // default:0.06,
      Info = "#36A2EB",    // default:"#2196F3",
      Success = "#70C699", //"#4BC0C0", // default:"#00C853",
      Warning = "#FF9F40", // default:"#FF9800",
      Error = "#FF6384",   // default:"#F44336",
      Dark = "#575757",
      Surface = "#FAFAFA",
      DrawerBackground = "#FAFAFA",
      AppbarBackground = "#BF4690", // Primary
      AppbarText = "#FAFAFA", // Surface
      TableHover = "#FCFCE3", // 亮黃色
    },
    ZIndex = new ZIndex()
    {
      Drawer = 1100,
      Popover = 1300, // defaut:1200;
      AppBar = 1200, // defaut:1300;
      Dialog = 1400,
      Snackbar = 1500,
      Tooltip = 1600,
    },
  };
}
