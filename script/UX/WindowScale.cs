using Godot;

namespace desktop.script.UX;

public partial class WindowScale : Node
{
    // 定义窗口宽度的上下限（像素）
    [Export] private int _maxWidth = 512;
    [Export] private int _minWidth = 256;
    // 每次滚轮滚动改变的宽度增量
    [Export] private int _speed = 16;

    private float _aspectRatio;

    public override void _Ready()
    {
        // 在启动时获取初始窗口的宽高比：高 / 宽
        var size = GetWindow().Size;
        _aspectRatio = (float)size.Y / size.X;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && WindowDrag.IsInValidZone())
        {
            var window = GetWindow();
            var currentSize = window.Size;
            var targetWidth = currentSize.X;

            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
            {
                // 计算新的宽度，并限制在最大值内
                targetWidth = Mathf.Min(currentSize.X + _speed, _maxWidth);
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
            {
                // 计算新的宽度，并限制在最小值内
                targetWidth = Mathf.Max(currentSize.X - _speed, _minWidth);
            }
            else
            {
                return; // 不是滚轮事件则退出
            }

            // 根据新的宽度和原始比例计算高度
            var targetHeight = Mathf.RoundToInt(targetWidth * _aspectRatio);
            
            window.Size = new Vector2I(targetWidth, targetHeight);
        }
    }
}