using Godot;

namespace desktop.script.UX;

public partial class PopupMenuShortUpHandle : PopupMenu
{
    public override void _ShortcutInput(InputEvent @event)
    {
        // 1. 基础检查：如果是回声（按住不放）或者不是按下事件，直接返回
        if (@event.IsEcho() || !@event.IsPressed()) 
            return;
        // 2. 遍历 PopupMenu 中所有的 Item
        for (var i = 0; i < ItemCount; i++)
        {
            // 跳过分隔符或被禁用的项
            if (IsItemSeparator(i) || IsItemDisabled(i))
                continue;
    
            // 获取该项关联的 Shortcut
            var sc = GetItemShortcut(i);
            // 3. 核心：检查 Shortcut 是否存在且与当前输入匹配
            if (sc != null && sc.MatchesEvent(@event))
            {
                // 获取 Item 的 ID
                var id = GetItemId(i);
                
                // 4. 模拟点击效果：发射 id_pressed 信号
                // 这样你在外部连接的信号处理函数就会被触发
                EmitSignal(PopupMenu.SignalName.IdPressed, id);
    
                // 标记输入已处理，防止事件继续传递给底层 UI 或场景
                GetViewport().SetInputAsHandled();
                
                // 找到匹配项后即可退出循环
                return;
            }
        }
    
        // 如果没有匹配到，调用基类逻辑
        base._ShortcutInput(@event);
    }
}