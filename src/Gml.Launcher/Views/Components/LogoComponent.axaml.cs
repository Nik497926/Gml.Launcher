using Avalonia;
using Avalonia.Controls.Primitives;
using Gml.Launcher.Core.Services;

namespace Gml.Launcher.Views.Components;

public class LogoComponent : TemplatedControl
{
    public static readonly StyledProperty<double> SizeProperty = AvaloniaProperty.Register<LogoComponent, double>(
        nameof(Size), 32);

    public static readonly StyledProperty<string> LogoPathProperty =
        AvaloniaProperty.Register<LogoComponent, string>(nameof(LogoPath), ThemeService.DefaultLogoPath);

    public static readonly StyledProperty<string> BrandNameProperty =
        AvaloniaProperty.Register<LogoComponent, string>(nameof(BrandName), ThemeService.DefaultBrandName);

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public string LogoPath
    {
        get => GetValue(LogoPathProperty);
        set => SetValue(LogoPathProperty, value);
    }

    public string BrandName
    {
        get => GetValue(BrandNameProperty);
        set => SetValue(BrandNameProperty, value);
    }
}
