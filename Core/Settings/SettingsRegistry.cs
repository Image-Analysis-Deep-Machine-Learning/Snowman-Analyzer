using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Threading;
using Snowman.Core.MachineLearning.Providers;
using Snowman.Utilities;
using Snowman.Windows;
using Ursa.Controls;

namespace Snowman.Core.Settings;

public static class SettingsRegistry
{
    private const string SettingsPath = "settings.json";
    
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
    private static readonly Dictionary<string, ISetting> AllSettings = [];
    private static SettingsWindow? _settingsWindow;
    private static bool _loadingSettings;
    
    // Do NOT change the names of the settings fields after they have been in the production without a GOOD reason.
    // The settings file uses them as keys. Changing them will cause losing current saved values.
    public static readonly ISetting<string> SelectedLlmProvider = new Setting<string>("Ollama")
    {
        AllowedValues = ["Anthropic", "Gemini", "Ollama", "OpenAI"]
    };
    
    public static readonly ISetting<string> SelectedLlmModel = new Setting<string>(string.Empty)
    {
        AllowedValues = []
    };
    
    public static readonly ISetting<string> AnthropicApiKey = new Setting<string>(string.Empty);
    public static readonly ISetting<string> GeminiApiKey = new Setting<string>(string.Empty);
    public static readonly ISetting<string> OllamaUri = new Setting<string>("http://localhost:11434");
    public static readonly ISetting<string> OpenAiApiKey = new Setting<string>(string.Empty);
    public static readonly ISetting<string> PythonLibraryPath = new Setting<string>(Path.Combine(Environment.CurrentDirectory, "python_win64", "python312.dll"));
    public static readonly ISetting<string> PythonExecutablePath = new Setting<string>(Path.Combine(Environment.CurrentDirectory, "python_win64", "python.exe"));
    public static readonly ISetting<string> PythonHomeDirectory = new Setting<string>(Path.Combine(Environment.CurrentDirectory, "python_win64"));

    static SettingsRegistry()
    {
        LoadSettings();
        BindEvents();
    }

    public static Dictionary<string, ISetting> GetSettingsList()
    {
        return new Dictionary<string, ISetting>(AllSettings);
    }

    public static void OpenSettingsWindow()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow();
            
            if (string.IsNullOrWhiteSpace(SelectedLlmModel.Value))
            {
                OnSelectedLlmProviderValueChanged(SelectedLlmProvider.Value, true);
            }
            
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        }
        
        if (_settingsWindow.WindowState == WindowState.Minimized)
        {
            _settingsWindow.WindowState = WindowState.Normal;
        }

        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    private static void LoadSettings()
    {
        if (!File.Exists(SettingsPath)) return;

        _loadingSettings = true;
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(File.ReadAllText(SettingsPath));
        
        if (dict is null) return;

        foreach (var setting in AllSettings)
        {
            if (!dict.TryGetValue(setting.Key, out var jsonElement)) continue;

            var boxedValueType = AllSettings[setting.Key].BoxedValue.GetType();
            AllSettings[setting.Key].BoxedValue = jsonElement.GetProperty("BoxedValue").Deserialize(boxedValueType) ?? new object();

            if (!jsonElement.TryGetProperty("BoxedAllowedValues", out var boxedAllowedValues) || boxedAllowedValues.ValueKind is not JsonValueKind.Array) continue;

            var boxedAllowedValuesCollection = AllSettings[setting.Key].BoxedAllowedValues;

            if (boxedAllowedValuesCollection is null) continue;
            
            boxedAllowedValuesCollection.Clear();
            
            foreach (var item in boxedAllowedValues.EnumerateArray())
            {
                var deserializedItem = item.Deserialize(boxedValueType) ?? new object();
                boxedAllowedValuesCollection.Add(deserializedItem);
            }
        }

        _loadingSettings = false;
    }
    
    private static void SaveSettings()
    {
        if (_loadingSettings) return; // avoid unnecessary writes
        
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(AllSettings, JsonSerializerOptions));
    }

    private static void BindEvents()
    {
        SelectedLlmProvider.ValueChanged += newProvider => OnSelectedLlmProviderValueChanged(newProvider);
    }

    private static void OnSelectedLlmProviderValueChanged(string newProvider, bool ignoreException = false)
    {
        var kernelProvider = KernelProvider.GetProviderFromName(newProvider);

        Dispatcher.UIThread.Post(async void () =>
        {
            if (SelectedLlmModel.BoxedAllowedValues is null || SelectedLlmModel.AllowedValues is null) return; // it is not null, but the compiler is clueless

            SelectedLlmModel.BoxedAllowedValues.Clear();
            SelectedLlmModel.BoxedAllowedValues.Add("Loading...");
            SelectedLlmModel.Value = SelectedLlmModel.AllowedValues[0];
            IEnumerable<string> models;

            try
            {
                models = await kernelProvider.GetAvailableModels();
            }

            catch (Exception e)
            {
                SelectedLlmModel.Value = string.Empty;
                
                if (!ignoreException && _settingsWindow is not null)
                {
                    await MessageBox.ShowAsync(_settingsWindow,
                        $"Cannot get available models for selected provider.\nError:\n{e.Message}\nStacktrace:\n{e.StackTrace}",
                        "Error", MessageBoxIcon.Error);
                }

                return;
            }

            finally
            {
                SelectedLlmModel.BoxedAllowedValues.Clear();
            }

            SelectedLlmModel.BoxedAllowedValues.AddRange(models);
            SelectedLlmModel.Value = SelectedLlmModel.AllowedValues[0];
        });
    }

    private class Setting<T> : ISetting<T>, INotifyPropertyChanged where T : notnull
    {
        private T _value;
        
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Events.EventHandler<T>? ValueChanged;
        
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                SaveSettings();
                ValueChanged?.Invoke(value);
                OnPropertyChanged();
            }
        }

        public object BoxedValue
        {
            get => Value;
            set => Value = (T)Convert.ChangeType(value, typeof(T));
        }

        public ObservableCollection<object>? BoxedAllowedValues { get; init; }

        public ObservableCollection<T>? AllowedValues
        {
            get;
            init
            {
                field = value;
                field?.CollectionChanged += (_, _) => SaveSettings();
                
                BoxedAllowedValues = [];

                if (field is not null)
                {
                    foreach (var item in field)
                    {
                        BoxedAllowedValues.Add(item);
                    }
                }
                
                BoxedAllowedValues.CollectionChanged += (_, e) => AllowedValues?.SyncWithObservableCollection(e);
            }
        }

        public Setting(T defaultValue, [CallerMemberName] string key = "")
        {
            _value = defaultValue;
            AllSettings[key] = this;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<TField>(ref TField field, TField value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<TField>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
