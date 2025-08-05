using System;
using System.Windows;
using MklinkUI.Core.Settings;

namespace MklinkUI.App;

public class ThemeManager
{
    private readonly ResourceDictionary _light = new() { Source = new Uri("Themes/Light.xaml", UriKind.Relative) };
    private readonly ResourceDictionary _dark = new() { Source = new Uri("Themes/Dark.xaml", UriKind.Relative) };

    public void Apply(ThemeOption theme)
    {
        var app = Application.Current;
        if (app == null) return;
        var dictionaries = app.Resources.MergedDictionaries;
        dictionaries.Remove(_light);
        dictionaries.Remove(_dark);
        dictionaries.Add(theme == ThemeOption.Dark ? _dark : _light);
    }
}
