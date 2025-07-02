using System;

namespace Snowman.Utilities;

public static class UIEventBus
{
    public static event Action<string>? InfoRequested;

    public static void RaiseInfo(string message)
    {
        InfoRequested?.Invoke(message);
    }
}