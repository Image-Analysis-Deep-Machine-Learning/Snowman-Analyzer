using System.Collections.Generic;
using Snowman.Core.Entities;

namespace Snowman.Data;

public class EventData(List<int> frameIndices, int objectId, int ruleId, Entity entity)
{
    public List<int> FrameIndices { get; set;} = frameIndices;
    public int ObjectId { get; set;} = objectId;
    public int RuleId { get; set;} = ruleId;
    public Entity Entity { get; set;} = entity;
}