using Snowman.Designer;

// ReSharper disable once CheckNamespace
namespace Snowman.Core.Scripting.DataSource.Variables;

public partial class NumberVariable
{
    public NumberVariable() : this("sample_name", Group.Default, "Sample Name")
    {
        
    }
}

public partial class EntitySelector
{
    public EntitySelector() : this("sample_name", Group.Default, "Sample Name")
    {
        
    }
}

public partial class DatasetSelector
{
    public DatasetSelector() : this("dataset", Group.Default, "Selected Dataset", DummyServiceProvider.Instance)
    {
        
    }
}
