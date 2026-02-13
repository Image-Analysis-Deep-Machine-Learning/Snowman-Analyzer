using Snowman.Core.Scripting.DataSource.Variables;

// ReSharper disable once CheckNamespace
namespace Snowman.Controls
{
    public partial class EventTimeline
    {

    }

    public partial class NodeControl
    {
        public NodeControl()
        {
            _nodeService = null!;
        }
    }
}

namespace Snowman.Core.Scripting.UserInterface.Controls
{
    public partial class EntitySelectorControl
    {
        public EntitySelectorControl() : this(new EntitySelector())
        {
            
        }
    }

    public partial class NumberVariableControl
    {
        public NumberVariableControl() : this(new NumberVariable())
        {
            
        }
    }

    public partial class DatasetSelectorControl
    {
        public DatasetSelectorControl() : this(new DatasetSelector())
        {
            
        }
    }
}
