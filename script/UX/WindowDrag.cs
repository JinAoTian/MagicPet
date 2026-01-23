using desktop.script.logic;
using Godot;

namespace desktop.script.UX;

public partial class WindowDrag : Node
{
    private bool _dragging;
    private bool _isPreparing;
    private Vector2I _dragOffset;
    private Vector2I _pressOrigin;
    
    private const float DragThreshold = 5.0f; 

    public static bool IsInValidZone()
    {
        // 直接获取屏幕全局鼠标位置
        Vector2I mousePos = DisplayServer.MouseGetPosition();
        int screenIndex = DisplayServer.WindowGetCurrentScreen();
        int screenHeight = DisplayServer.ScreenGetSize(screenIndex).Y;
        
        // 判定逻辑
        return mousePos.Y >= (screenHeight / 3);
    }

    public override void _Process(double delta)
    {
        // 1. 检测鼠标左键状态
        bool isLeftPressed = Input.IsMouseButtonPressed(MouseButton.Left);

        if (isLeftPressed)
        {
            Vector2I currentMousePos = DisplayServer.MouseGetPosition();

            if (!_dragging && !_isPreparing)
            {
                // 初次按下：检查是否在有效区
                if (IsInValidZone())
                {
                    _isPreparing = true;
                    _pressOrigin = currentMousePos;
                }
            }
            else if (_isPreparing && !_dragging)
            {
                // 准备中：检查移动距离是否达标
                if (currentMousePos.DistanceTo(_pressOrigin) > DragThreshold)
                {
                    _dragging = true;
                    _isPreparing = false;
                    // 正式锁定 Offset
                    _dragOffset = currentMousePos - DisplayServer.WindowGetPosition();
                    CharAnim.开始拖拽();
                    Dialogue.关闭标题();
                    IO.单例.stopAudio();
                }
            }
            
            // 3. 执行拖拽：一旦进入拖拽状态，无视区域，直到松手
            if (_dragging)
            {
                DisplayServer.WindowSetPosition(currentMousePos - _dragOffset);
            }
        }
        else
        {
            // 4. 松开鼠标：重置所有状态
            if (_dragging)
            {
                CharAnim.结束拖拽();
            }
            _dragging = false;
            _isPreparing = false;
        }
    }
}