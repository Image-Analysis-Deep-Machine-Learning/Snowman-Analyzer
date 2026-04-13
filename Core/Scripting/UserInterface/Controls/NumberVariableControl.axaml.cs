using Snowman.Controls;
using Snowman.Core.Scripting.DataSource.Variables;
using IServiceProvider = Snowman.Core.Services.IServiceProvider;

namespace Snowman.Core.Scripting.UserInterface.Controls;

public partial class NumberVariableControl : UserControlWrapper<NumberVariable>
{
    public NumberVariableControl(NumberVariable variable)
    {
        InitializeComponent();
        DataContext = variable;
    }

    protected override NumberVariable GetDataContext(IServiceProvider serviceProvider)
    {
        return DataContext;
    }
}
