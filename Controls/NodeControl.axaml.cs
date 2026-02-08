using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Snowman.Core.Scripting.Nodes;
using Snowman.Core.Services;
using Snowman.DataContexts;

namespace Snowman.Controls;

public partial class NodeControl : UserControlWrapper<NodeControlDataContext>
{
    private static int _topZIndex; // does not matter that it's static, there's no way someone's clicking over 2 billion times
    
    private readonly INodeService _nodeService;
    private Point _dragStartPoint;
    private bool _isDragging;
    
    public NodeControl(Node node, IServiceProvider serviceProvider)
    {
        _nodeService = serviceProvider.GetService<INodeService>();
        DataContext = new NodeControlDataContext(node);
        InitializeComponent();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || _nodeService.IsNewConnectionActive() || (e.Source is Visual visual && IsInteractiveElement(visual))) return;
        
        ZIndex = ++_topZIndex; // move the clicked node to the front
        _isDragging = true;
        _dragStartPoint = e.GetPosition(Parent as Control);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isDragging || Parent is not Canvas canvas || DataContext is not { } dataContext) return;

        var pos = e.GetPosition(canvas);
        var delta = pos - _dragStartPoint;
        
        dataContext.X += delta.X;
        dataContext.Y += delta.Y;

        _dragStartPoint = pos;
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!_isDragging) return; // prevents mouse capture issues

        _isDragging = false;
        e.Pointer.Capture(null);
        e.Handled = true;
    }
    
    private bool IsInteractiveElement(Visual? visual)
    {
        while (visual != null && visual != this)
        {
            if (visual is ComboBox or Button or TextBox or NumericUpDown or Popup) // TODO: add all controls that are clickable and interactable because this retarded framework DOES NOT FUCKING CAPTURE THE POINTER WHEN CLICKING ON SUCH CONTORL
                return true;
        
            visual = visual.Parent as Visual;
        }
        
        return false;
    }
}
