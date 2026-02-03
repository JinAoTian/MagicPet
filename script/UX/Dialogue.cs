using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using desktop.script.Asset;
using desktop.script.Loader;
using desktop.script.logic;
using desktop.script.Util;
using DialogueManagerRuntime;
using Godot;

namespace desktop.script.UX;

public partial class Dialogue : Node
{
    [Export] public PopupMenu 选项菜单;
    [Export] public RichTextLabel 标题;
    [Export] public IconResource IconResource;
    private const float 打字速度 = 20.0f;
    private static Dialogue _单例;
    private static readonly List<可见脚本信息> 脚本列表 = new();
    private static E选项类型 _选项类型 = E选项类型.无;
    private static string _脚本选项标题;
    private static string _当前文本;
    public override void _Ready()
    {
        // 清理并添加选项
        选项菜单.Clear();
        选项菜单.AddThemeConstantOverride("icon_max_width", 24);
        选项菜单.AddThemeFontSizeOverride("font_size", 24);
        选项菜单.IdPressed += OnMenuItemPressed;
        选项菜单.PopupHide += 关闭标题;
        标题.Visible = false;
        _单例 = this;
    }
    // ReSharper disable once MemberCanBePrivate.Global 外部调用
    public static void 延迟显示标题(string 文本) => _单例.CallDeferred("单例显示标题",文本);
    public static void 显示标题(string 文本) => _单例.单例显示标题(文本);
    private static int _当前标题序列号;
    public static async Task 显示临时标题(string 文本,int 显示时间=3000)
    {
        if (string.IsNullOrEmpty(文本))return;
        // 每次调用，递增序列号
        var 当前序列 = ++_当前标题序列号;
        try
        {
            _单例.单例显示标题(文本);

            await Task.Delay(显示时间);
            if (_当前标题序列号 == 当前序列)
            {
                关闭标题();
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"临时标题异常: {e.Message}");
        }
    }
    private void 单例显示标题(string 原始文本)
    {
        if(string.IsNullOrEmpty(原始文本))return;
        var 文本 = Tr(原始文本);
        _当前文本 = 原始文本;
        var t标题 = 标题;
        t标题.Visible = true;

        // 1. 设置带 BBCode 的文本
        t标题.Text = $"[wave][bgcolor=#00000066]{文本}[/bgcolor][/wave]";
        // 2. 初始可见字符设为 0
        t标题.VisibleCharacters = 0;

        // 3. 计算文本总长度并创建动画
        // GetTotalCharacterCount 会自动忽略 BBCode 标签，只计算实际显示的字符
        var 总字符数 = t标题.GetTotalCharacterCount();
        var 持续时间 = 总字符数 / 打字速度;
        // 创建 Tween 动画
        var 动画 = _单例.CreateTween();
        动画.TweenProperty(
            t标题, 
            "visible_characters", 
            总字符数, 
            持续时间
        ).SetTrans(Tween.TransitionType.Linear);
    }
    public static void 关闭标题()
    {
        _单例.标题.Visible = false;
        _当前文本 = null;
    }
    public static void 关闭指定标题(string text)
    {
        if (_当前文本 == text)
        {
            关闭标题();
        }
    }
    private void 底部居中显示()
    {
        选项菜单.AddIconItem(IconResource.取消图标,Tr("cancel"),114514);//固定取消,防止退出bug
        var viewportSize = GetViewport().GetVisibleRect().Size;
        var menuSize = 选项菜单.GetContentsMinimumSize();
        var x = (viewportSize.X - menuSize.X) / 2;
        var y = viewportSize.Y * 2 / 3;
        选项菜单.Size = new Vector2I(20, 8);
        选项菜单.MaxSize = new Vector2I(选项菜单.MaxSize.X, (int)(viewportSize.Y/3));
        选项菜单.Position = new Vector2I((int)x, (int)y);
        选项菜单.Popup();
    }
    public static void 文件处理完成(string tip)
    {
        _选项类型 = E选项类型.处理完成;
        var 选项菜单 = _单例.选项菜单;
        显示标题(tip);
        选项菜单.Clear();
        选项菜单.AddIconItem(_单例.IconResource.目录图标,_单例.Tr("opendir"),0);
        ShortCutUtil.BindShortCut(选项菜单,0,1);
        var cnt = 1;
        foreach (var 脚本信息 in CommandLoader.展示指令列表)
        {
            var name = _单例.Tr(脚本信息.name);
            _单例.选项菜单.AddIconItem(脚本信息.IconImg,name,cnt);
            ShortCutUtil.BindShortCut( _单例.选项菜单,cnt,cnt+1);
            cnt++;
        }
        _单例.底部居中显示();
    }
    public static void 显示脚本选项<T>(List<T> 新脚本列表,string 询问,bool 配置显示=false) where T : 可见脚本信息
    {
        if(新脚本列表.Count==0)return;
        _选项类型 = E选项类型.脚本;
        脚本列表.Clear();
        脚本列表.AddRange(新脚本列表);
        _脚本选项标题 = 询问;
        显示脚本选项(配置显示);
    }
    private static void 显示脚本选项(bool 配置显示=false)
    {
        if(脚本列表.Count==0)return;
        if(!string.IsNullOrEmpty(_脚本选项标题))显示标题(_脚本选项标题);
        _单例.选项菜单.Clear();
        var cnt = 0;
        foreach (var 脚本信息 in 脚本列表)
        {
            var name = _单例.Tr(脚本信息.name);
            _单例.选项菜单.AddIconItem(脚本信息.IconImg,name,cnt);
            ShortCutUtil.BindShortCut( _单例.选项菜单,cnt,cnt+1);
            cnt++;
        }
        if (配置显示)
        {
            foreach (var 脚本信息 in Main.配置脚本列表)
            {
                var name = _单例.Tr(脚本信息.name)+_单例.Tr("config");
                _单例.选项菜单.AddIconItem(脚本信息.IconImg,name,cnt);
                ShortCutUtil.BindShortCut( _单例.选项菜单,cnt,cnt+1);
                cnt++;
            }
        }
        _单例.底部居中显示();
    }
    private static void OnMenuItemPressed(long id)
    {
        _单例.选项菜单.Hide();
        switch (id)
        {
            case 114514:
                _选项类型 = E选项类型.无;
                脚本结束();
                return;
            case 6174:
                _选项类型 = E选项类型.脚本;
                _单例.CallDeferred(nameof(显示脚本选项));
                return;
            default:
                switch (_选项类型)
                {
                    case E选项类型.无:
                        return;
                    case E选项类型.脚本:
                        if (脚本列表!=null)
                        {
                            if (id < 脚本列表.Count)
                            {
                                Main.选择脚本(脚本列表[(int)id]);
                            }
                            else
                            {
                                Main.打开配置((int)id-脚本列表.Count);
                            }
                        }
                        break;
                    case E选项类型.处理完成:
                        文件处理完成回调((int)id);
                        break;
                    case E选项类型.对话:
                        if (_当前回复列表 != null && _当前回复列表.Count > (int)id)
                        {
                            // 获取点击选项对应的下一个 ID
                            var nextId = _当前回复列表[(int)id].NextId;
                            foreach (var tag in _当前回复列表[(int)id].Tags)
                            {
                                var list = tag.Split("=", 2);
                                IO.单例.set(list[0],list[1]);
                            }
                            进行对话(nextId);
                        }
                        else if (_当前回复列表 == null || _当前回复列表.Count == 0)
                        {
                            对话结束();
                        }
                        break;
                }
                break;
        }
    }

    #region 对话逻辑
    private static Resource _当前对话资源;
    private static Godot.Collections.Array<DialogueResponse> _当前回复列表;
    private static readonly Dictionary<string, Texture2D> IconMap = new();

    public static void 开始对话(Resource 对话资源)
    {
        _当前对话资源 = 对话资源;
        IconMap.Clear();
        进行对话("start");
    }

    private static void 对话结束()
    {
        
        关闭标题(); // 对话结束
        _选项类型 = E选项类型.无;
        Main.对话结束();
    }
    private static async void 进行对话(string 节点)
    {
        try
        {
            // 获取对话行数据
            var line = await DialogueManager.GetNextDialogueLine(_当前对话资源, 节点);
            if (line != null)
            {
                _单例.处理对话行(line);
            }
            else
            {
                对话结束();
            }
        }
        catch (Exception e)
        {
            GD.Print($"进行对话异常:{e.Message}");
        }
    }
    private void 处理对话行(DialogueLine line)
    {
        _选项类型 = E选项类型.对话;
        var 脚本 = Main.当前脚本;
        显示标题(line.Text);
        选项菜单.Clear();
        _当前回复列表 = line.Responses;
        var cnt = 1;
        if (_当前回复列表.Count > 0)
        {
            for (var i = 0; i < _当前回复列表.Count; i++)
            {
                var 选项 = _当前回复列表[i];
                if (选项.IsAllowed)
                {
                    var iconpath = 选项.GetTagValue("icon");
                    if (string.IsNullOrEmpty(iconpath)) iconpath = "icon.png";
                    if (!IconMap.ContainsKey(iconpath))
                    {
                        if (ImageUtil.Loadimage(Path.Combine(脚本.Path,iconpath),out var icon))
                        {
                            IconMap[iconpath] = icon;
                        }
                        else
                        {
                            IconMap[iconpath] = null;
                        }
                    }
                    选项菜单.AddIconItem(IconMap[iconpath],Tr(选项.Text), i);
                    ShortCutUtil.BindShortCut( _单例.选项菜单,i,cnt++);
                }
            }
        }
        else
        {
            选项菜单.AddItem("继续...", 0);
        }

        if (脚本列表.Count>0)选项菜单.AddIconItem(IconResource.返回图标,Tr("return"),6174);
        ShortCutUtil.BindShortCut( _单例.选项菜单,6174,0);
        底部居中显示();
    }
    public static void 脚本结束()
    {
        脚本列表.Clear();
    }
    #endregion
    
    #region 复制剪切

    private static void 文件处理完成回调(int id)
    {
        if (id == 0)
        {
            GD.Print((string)IO.单例.get("out"));
            OS.ShellOpen((string)IO.单例.get("out"));
            return; 
        }
        IO.单例.set("in",IO.单例.get("result"));
        if (id <= CommandLoader.展示指令列表.Count)
        {
            Main.选择脚本( CommandLoader.展示指令列表[id-1]);
        }
    }
    #endregion
}

public enum E选项类型
{
    无,
    脚本,
    处理完成,
    对话
}