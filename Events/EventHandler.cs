namespace Snowman.Events;

/// <summary>
/// Specific version of EventHandler that requires the sender to be strongly typed allowing callbacks without type-cast.
/// </summary>
public delegate void EventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);