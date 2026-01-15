using Godot;
using System;

namespace desktop.script.Util;

public partial class ShortCutUtil : Node
{
    /// <summary>
    /// 绑定 0-9 的数字键（包含主键盘和小键盘）
    /// </summary>
    public static void BindShortCut(PopupMenu menu, int id, int num)
    {
        if (num < 0 || num > 9) return;

        var shortcut = new Shortcut();

        // 1. 处理主键盘数字 (Key0 - Key9)
        var mainKey = new InputEventKey();
        mainKey.Keycode = (Key)((int)Key.Key0 + num);
        shortcut.Events.Add(mainKey);

        // 2. 处理小键盘数字 (Kp0 - Kp9)
        var kpKey = new InputEventKey();
        kpKey.Keycode = (Key)((int)Key.Kp0 + num);
        shortcut.Events.Add(kpKey);

        // 设置到对应的菜单项
        SetMenuShortcut(menu, id, shortcut);
    }
    /// <summary>
    /// 绑定非数字的普通按键（例如 "A", "Enter", "Escape"）
    /// </summary>
    public static void BindShortCut(PopupMenu menu, int id, string keyStr)
    {
        // 将字符串转换为 Godot 的 Key 枚举
        // 注意：这里的字符串需要符合 Godot 的命名，例如 "Enter", "Space"
        if (Enum.TryParse(keyStr, true, out Key keyEnum))
        {
            var shortcut = new Shortcut();
            var eventKey = new InputEventKey();
            eventKey.Keycode = keyEnum;
            shortcut.Events.Add(eventKey);

            SetMenuShortcut(menu, id, shortcut);
        }
        else
        {
            GD.PrintErr($"无法识别的按键名称: {keyStr}");
        }
    }

    private static void SetMenuShortcut(PopupMenu menu, int id, Shortcut shortcut)
    {
        var index = menu.GetItemIndex(id);
        if (index != -1)
        {
            menu.SetItemShortcut(index, shortcut);
            // 默认情况下，快捷键只有在 Popup 显示时有效
            // 如果需要全局生效，可以设置 menu.ShortcutContext
        }
    }
}