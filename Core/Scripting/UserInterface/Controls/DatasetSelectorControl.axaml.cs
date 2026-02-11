using Snowman.Core.Scripting.DataSource.Variables;

namespace Snowman.Core.Scripting.UserInterface.Controls;

public partial class DatasetSelectorControl : VariableControl<DatasetSelector>
{
    public DatasetSelectorControl(DatasetSelector datasetSelector)
    {
        DataContext = datasetSelector;
        InitializeComponent();
    }
}
