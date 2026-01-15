using System.Collections.Generic;
using desktop.script.Asset;
using desktop.script.Loader;
using desktop.script.logic;
using Godot;

namespace desktop.script.UX;

public partial class Context : Node
{
    [Export] public PopupMenu 右键菜单;
    [Export] public IconResource IconResource;
    private static List<脚本信息> 直接指令列表 => CommandLoader.直接指令列表;
    private static Dictionary<string, 脚本组信息> 直接指令组映射 => CommandLoader.直接指令组映射;
    private static Dictionary<string, List<脚本信息>> 直接指令组脚本映射 => CommandLoader.直接指令组脚本映射;
    private static readonly List<string> 脚本组列表 = [];
    private static Context _单例;
    public override void _Ready()
    {
        _单例 = this;
        // 清理并添加选项
        右键菜单.AddThemeConstantOverride("icon_max_width", 24);
        右键菜单.AddThemeFontSizeOverride("font_size", 24);
        右键菜单.IdPressed += OnMenuItemPressed;
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true } mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
               ShowMenuAtMouse();
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                右键菜单.Hide();
            }
        }
    }
    private void ShowMenuAtMouse()
    {
        if (右键菜单.Visible) { 右键菜单.Hide(); }
        // 1. 获取当前鼠标的全局坐标
        var mousePos = GetViewport().GetMousePosition();
        
        // 2. 获取窗口可视区域大小和菜单实际大小
        var windowSize = GetViewport().GetVisibleRect().Size;
        Vector2 menuSize = 右键菜单.Size;
        // 最终确定的显示位置
        var finalPos = mousePos;

        // 3. 边界检测逻辑
        // 如果 鼠标位置 + 菜单宽度 > 窗口宽度，说明右边出界了，将菜单往左移
        if (mousePos.X + menuSize.X > windowSize.X)
        {
            finalPos.X -= menuSize.X;
        }

        // 如果 鼠标位置 + 菜单高度 > 窗口高度，说明下方出界了，将菜单往上移
        if (mousePos.Y + menuSize.Y > windowSize.Y)
        {
            finalPos.Y -= menuSize.Y;
        }
        // 4. 设置位置并显示 (确保坐标不为负数)
        右键菜单.MaxSize = new Vector2I(右键菜单.MaxSize.X, (int)windowSize.Y/3);
        右键菜单.Position = (Vector2I)finalPos.Clamp(Vector2.Zero, windowSize);
        右键菜单.Show();
    }
    
    public static void 显示指令列表()
    {
        var 右键菜单 = _单例.右键菜单;
        右键菜单.Clear();
        var cnt = 1;
        foreach (var 脚本信息 in 直接指令列表)
        {
           右键菜单.AddIconItem(脚本信息.IconImg,_单例.Tr(脚本信息.name),cnt);
           cnt++;
        }
        foreach (var key in 直接指令组脚本映射.Keys)
        {
            if (直接指令组映射.TryGetValue(key,out var 脚本组信息))
            {
                脚本组列表.Add(key);
                右键菜单.AddIconItem(脚本组信息.IconImg,_单例.Tr(脚本组信息.name),cnt);
            }
            cnt++;
        }
        右键菜单.AddIconItem(_单例.IconResource.关机图标,_单例.Tr("close"), 0);
        右键菜单.AddIconItem(_单例.IconResource.取消图标,_单例.Tr("cancel"),114514);
    }

    private static void OnMenuItemPressed(long id)
    {
        if(id==114514)return;
        switch (id)
        {
            case 0:
                CharAnim.播放退出动画();
                return;
        }
        var index = (int)id - 1;
        if (index < 直接指令列表.Count)
        {
            Main.选择脚本(直接指令列表[index]);
            return;
        }
        index -= 直接指令列表.Count;
        if (index<脚本组列表.Count)
        {
            var key = 脚本组列表[index];
            Dialogue.显示脚本选项(直接指令组脚本映射[key],_单例.Tr(直接指令组映射[key].ask));
        }
    }
    public override void _Notification(int what)
    {
        // 鼠标离开窗口关闭菜单
        if (what == NotificationWMMouseExit)
        {
            右键菜单.Hide();
        }
    }
}