using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Platform.Storage;
using Snowman.Core.Services;
using Snowman.Data;
using Snowman.Utilities;
using Ursa.Controls;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.DataSource.Variables;

public partial class DatasetSelector : GenericVariableWrapper<DatasetData>
{
    private readonly IStorageProviderService _storageProviderService;
    private readonly IProjectService _projectService;

    public bool IsCustomPathSelected
    {
        get;
        set
        {
            field = value;
            
            if (!value)
            {
                CustomDatasetPath = string.Empty;
                TypedValue = _projectService.GetDatasetData();
            }
            
            OnPropertyChanged();
        }
    }

    public string CustomDatasetPath
    {
        get;
        set
        {
            field = value;

            if (!string.IsNullOrWhiteSpace(field))
            {
                LoadSelectedDataset();
            }
            
            OnPropertyChanged();
        }
    }

    private DatasetSelector(string name, Group group, string friendlyName, IServiceProvider serviceProvider) : base(name, group, friendlyName)
    {
        _storageProviderService = serviceProvider.GetService<IStorageProviderService>();
        _projectService = serviceProvider.GetService<IProjectService>();
        IsCustomPathSelected = false;
        CustomDatasetPath = string.Empty;
    }

    public async Task PickCustomDataset()
    {
        var filePickerResult = await _storageProviderService.GetStorageProvider().OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [AdditionalFilePickerFileTypes.Xml],
            Title = "Open Dataset XML File"
        });

        if (!filePickerResult.Any()) return;

        try
        {
            CustomDatasetPath = filePickerResult[0].Path.LocalPath;
        }

        catch (Exception e)
        {
            await MessageBox.ShowAsync($"Unable to load selected file.\nMessage:\n{e.Message}",  "Error", MessageBoxIcon.Error);
        }
    }

    private void LoadSelectedDataset()
    {
        var fileStream = new FileStream(CustomDatasetPath, FileMode.Open);
        using var reader = new StreamReader(fileStream);
        var fileContent = reader.ReadToEnd();
        
        TypedValue = DatasetData.Deserialize(fileContent) ?? throw new Exception("Xml data could not be deserialized");
    }

    public override Variable Copy(IServiceProvider serviceProvider)
    {
        return new DatasetSelector(Name, Group, FriendlyName, serviceProvider)
        {
            IsCustomPathSelected = IsCustomPathSelected,
            CustomDatasetPath = CustomDatasetPath
        };
    }

    public override void SetPropertiesFromXml(XmlElement xml)
    {
        // no properties atm
    }
}
