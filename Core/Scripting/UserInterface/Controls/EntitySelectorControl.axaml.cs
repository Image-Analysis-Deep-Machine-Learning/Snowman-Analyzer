using Snowman.Core.Scripting.DataSource.Variables;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.UserInterface.Controls;

public partial class EntitySelectorControl : VariableControl<EntitySelector>
{
    public EntitySelectorControl(EntitySelector entitySelector)
    {
        InitializeComponent();
        DataContext = entitySelector;
    }

    protected override EntitySelector GetDataContext(IServiceProvider serviceProvider)
    {
        return DataContext;
    }
}
