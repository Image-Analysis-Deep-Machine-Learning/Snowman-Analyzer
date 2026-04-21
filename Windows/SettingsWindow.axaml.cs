using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Snowman.Core.Settings;

namespace Snowman.Windows;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        CreateSettingsControls();
    }

    private void CreateSettingsControls()
    {
        foreach (var setting in SettingsRegistry.GetSettingsList())
        {
            if (setting.Value is ISetting<string> stringSetting)
            {
                if (stringSetting.AllowedValues is not null) {}
                CreateStringSettingControl(setting.Key, stringSetting);
            }
        }
    }

    private void CreateStringSettingControl(string settingKey, ISetting<string> stringSetting)
    {
        var newControl = new StackPanel();
        newControl.Children.Add(new TextBlock { Text = settingKey });
        Control control;
        
        if (stringSetting.AllowedValues is not null)
        {
            control = CreateComboBox(stringSetting);
        }

        else
        {
            control = CreateTextBox(stringSetting);
        }

        control.Margin = new Thickness(5);
        newControl.Children.Add(control);
        SettingsPanel.Children.Add(newControl);
    }
    
    private static TextBox CreateTextBox(ISetting<string> stringSetting)
    {
        var textBox = new TextBox();
    
        textBox.Bind(
            TextBox.TextProperty,
            new Binding
            {
                Source = stringSetting,
                Path = nameof(stringSetting.Value),
                Mode = BindingMode.TwoWay
            });
    
        return textBox;
    }
    
    private static ComboBox CreateComboBox(ISetting<string> stringSetting)
    {
        var comboBox = new ComboBox();

        comboBox.Bind(
            ItemsControl.ItemsSourceProperty,
            new Binding
            {
                Source = stringSetting,
                Path = nameof(stringSetting.AllowedValues)
            });

        comboBox.Bind(
            SelectingItemsControl.SelectedItemProperty,
            new Binding
            {
                Source = stringSetting,
                Path = nameof(stringSetting.Value),
                Mode = BindingMode.TwoWay
            });

        return comboBox;
    }
}
