namespace Snowman.Events;

/// <summary>
/// Specific version of EventHandler that requires the sender to be strongly typed allowing callbacks without type-cast.
/// </summary>
public delegate void EventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs e);
/// <summary>
/// Event used just as a signal that something happened
/// </summary>
public delegate void SignalEventHandler();