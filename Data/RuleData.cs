namespace Snowman.Data;

/**
 * maxFrequency - the largest number of events occurring simultaneously at any one frame from the whole sequence after applying this one rule
 */
public class RuleData(int id, string name, int maxFrequency)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public int MaxFrequency { get; } = maxFrequency;
}
