using Snowman.Controls;
using Snowman.Core.Scripting.DataSource.Variables;
using Snowman.Core.Services;

namespace Snowman.Core.Scripting.UserInterface.Controls;

public partial class EntitySelectorControl : UserControlWrapper<EntitySelector>
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
