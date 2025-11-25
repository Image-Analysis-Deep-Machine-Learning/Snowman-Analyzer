using System;
using System.Collections.Generic;

namespace Snowman.Events;

public class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = [];
}
