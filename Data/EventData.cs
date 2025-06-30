using System.Text;
using Snowman.Core;
using Snowman.Core.Entities;

namespace Snowman.Data;

// lists of EventData objects can be stored in 3 dictionaries:
// Dictionary<int ruleId, List<EventData> eventsTriggeredByTheSameRule>
    // to access all the events triggered by the same rule .. to display them on separate timelines
// Dictionary<int trackId, List<EventData> eventsRelatedToTheSameObject>
    // to access all the events related to the same object (e.g. to the same cell or the same car) .. to highlight them all together on hover
// Dictionary<int frameIndex, List<EventData> eventsOccurringInTheSameFrame>
    // to access all the events occurring in the same frame .. to distinguish overlapping events triggered by the same rule

/**
 * frameIndex - frame in which the event happened
 * 
 * objectBbox - identifies the object (its annotation) which triggered the event
 *      (e.g. cell or car; to highlight bbox of the object in the main canvas)
 * 
 * firstEventOfObject - this is the first event related to the object which triggered the event when applying this rule
 *      (e.g. this is the first frame in which the cell's bbox has intersected with a point (just this rule))
 * 
 * entity - user defined graphic entity which has triggered the event
 *      (e.g. point or rectangle; to highlight the entity in the main canvas;
 *      useful for cases when one rule is simultaneously applied to multiple graphic entities)
 * 
 * ruleId - identifies the rule that has been applied, triggering the events
 *      (to distinguish between outputs of applying multiple rules)
 */
public class EventData(int frameIndex, BoundingBox objectBbox, bool isFirstEventOfObject, Entity entity, int ruleId)
{
    public int FrameIndex { get; } = frameIndex;
    private BoundingBox ObjectBbox { get;} = objectBbox;
    public bool IsFirstEventOfObject { get; } = isFirstEventOfObject;
    private Entity Entity { get; set;} = entity;
    public int RuleId { get; } = ruleId;

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($" Frame: {FrameIndex + 1}\n");
        sb.Append($" Object track ID: {ObjectBbox.ClassName.TrackId}\n");
        
        // TODO: info about entity (maybe add entity IDs to identify them easily?)
        var entityType = Entity.GetType().Name;
        if (entityType.EndsWith("Entity")) entityType = entityType.Substring(0, entityType.Length - 6);
        sb.Append($" Entity: {entityType}\n");
        
        // TODO: info about rule (its name, e.g. "Point intersection", "Rectangle intersection", "Line passing"...)
        sb.Append($" Rule: {RuleId}");
        return sb.ToString();
    }
}