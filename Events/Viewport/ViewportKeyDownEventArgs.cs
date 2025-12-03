using Avalonia.Input;

namespace Snowman.Events.Viewport;
public readonly record struct ViewportKeyDownEventArgs(KeyEventArgs WrappedArgs);