using Snowman.Core.Scripting.DataSource.Variables;

namespace Snowman.Core.Scripting.UserInterface.Controls;

public partial class EntitySelectorControl : VariableControl<EntitySelector>
{
    public EntitySelectorControl(EntitySelector entitySelector)
    {
        InitializeComponent();
        DataContext = entitySelector;
    }
}
