using Snowman.Controls;
using Snowman.Core.Scripting.DataSource.Variables;
using Snowman.Core.Services;

namespace Snowman.Core.Scripting.UserInterface.Controls;

public partial class DatasetSelectorControl : UserControlWrapper<DatasetSelector>
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
