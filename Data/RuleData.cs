namespace Snowman.Data;

public class RuleData(int id, string name)
{
    public int Id { get; set; } = id;
    public string Name {get; set;} = name;
}