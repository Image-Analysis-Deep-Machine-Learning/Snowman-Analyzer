using System;
using Snowman.Core.Scripting.Nodes;

namespace Snowman.Core.Scripting.DataSource;

public interface IDataSource
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public Group Group { get; set; }
    public string FriendlyName { get; set; }
}
