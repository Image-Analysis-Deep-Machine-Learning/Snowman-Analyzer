using Snowman.Controls;

namespace Snowman.Core.Scripting.UserInterface;

public abstract class VariableControl<T> : UserControlWrapper<T> where T : class, new();
