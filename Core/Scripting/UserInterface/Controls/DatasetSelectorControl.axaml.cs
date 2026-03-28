using Snowman.Core.Scripting.DataSource.Variables;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.UserInterface.Controls;

public partial class DatasetSelectorControl : VariableControl<DatasetSelector>
{
    public DatasetSelectorControl(DatasetSelector datasetSelector)
    {
        DataContext = datasetSelector;
        InitializeComponent();
    }

    protected override DatasetSelector GetDataContext(IServiceProvider serviceProvider)
    {
        return DataContext;
    }
}
