using System;
using System.Collections.Generic;
using desktop.script.logic;
using Godot;

namespace desktop.script.UX;

public partial class CharAnim : AnimatedSprite2D
{
    // 不需要定义额外的 _animatedSprite 变量，因为当前类本身就是 AnimatedSprite2D
    private static CharAnim _单例;
    private static string _退出动画名;
    private static 人物数据 显示人物 => Main.显示人物;
    private const int Idle循环下限 = 8;
    private const int Idle循环上限 = 16;
    private static int _fidget触发次数;
    private static int _idle循环次数;
    private static readonly List<string> 内置动画组 = ["idle", "celerate","drag","dragup","dragdown","fidget"];
    public override void _Ready()
    {
        // 直接给自己的 AnimationFinished 信号绑定方法
        AnimationFinished += OnAnimationFinished;
        _单例 = this;
        _fidget触发次数 = Random.Shared.Next(Idle循环下限, Idle循环上限 + 1);
    }
    private void OnAnimationFinished()
    {
        var 人物数据 = 显示人物;
        var 动画数据 = 人物数据.动画信息映射[Animation];
        var 动画类型 = 动画数据.Type;
        switch (动画类型)
        {
            case "enter":
                SpriteFrames.RemoveAnimation(动画数据.name);//入场动画无用了
                _idle循环次数 = 0;
                进入状态("idle");
                break;
            case "drag":
            case "dragup":
                进入状态("drag");
                break;
            case "celerate":
            case "dragdown":
            case "fidget":
                _idle循环次数 = 0;
                进入状态("idle");
                break;
            case "idle":
                _idle循环次数++;
                if (_idle循环次数>=_fidget触发次数)
                {
                    _fidget触发次数 = Random.Shared.Next(Idle循环下限, Idle循环上限 + 1);
                    进入状态("fidget");
                }
                else
                {
                    进入状态("idle");
                }
                break;
            case "exit":
                Main.游戏结束();
                GetTree().Quit();
                break;
        }
    }
    public static void 播放退出动画()
    {
        _单例.Play(_退出动画名);
    }
    public static void 开始庆祝() => 进入状态("celerate");
    public static void 开始拖拽()=>进入状态("dragup");
    public static void 结束拖拽()=>进入状态("dragdown");
    private static void 进入状态(string id)
    {
        if (显示人物.动画池字典.TryGetValue(id,out var list) && list.Count>0)
        {

            _单例.Play(list.列表随机项().name);
        }
        else
        {
            if (id!="idle")
            {
                // ReSharper disable once TailRecursiveCall
                进入状态("idle");
            }
        }   
    }
    public static void 载入人物动画()
    {
        var 人物 = 显示人物;
        var 状态机 = _单例.SpriteFrames;
        var 进入动画 = 人物.动画池字典["enter"].列表随机项();
        var 退出动画 = 人物.动画池字典["exit"].列表随机项();
        加载动画(状态机,进入动画);
        _单例.Play(进入动画.name);//先显示,再加载后面动画
        foreach (var 动画组 in 内置动画组)
        {
            加载动画组(人物,动画组);
        }
        加载动画(状态机,退出动画);
        _退出动画名 = 退出动画.name;
    }
    private static void 加载动画组(人物数据 人物,string id)
    {
        if (人物.动画池字典.TryGetValue(id,out var list))
        {
            foreach (var 动画 in list)
            {
                加载动画(_单例.SpriteFrames,动画);
            }
        }
    }
    private static void 加载动画(SpriteFrames 状态机, 动画信息 动画信息) => 加载动画(状态机,动画信息.name,动画信息.Path,动画信息.rate);
    private static void 加载动画(SpriteFrames 状态机, string 动画名, string 目录, int 帧率)
    {
        // 1. 检查目录是否存在 (使用绝对路径)
        if (!DirAccess.DirExistsAbsolute(目录))
        {
            GD.PrintErr($"[错误] 外部目录不存在: {目录}");
            return;
        }
        // 2. 初始化动画轨道
        if (状态机.HasAnimation(动画名))
        {
            状态机.RemoveAnimation(动画名);
        }
        状态机.AddAnimation(动画名);
        状态机.SetAnimationSpeed(动画名, 帧率);
        状态机.SetAnimationLoop(动画名, false);

        // 3. 获取所有 PNG 文件
        using var dir = DirAccess.Open(目录);
        dir.ListDirBegin();
    
        var filePaths = new List<string>();
        var fileName = dir.GetNext();

        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.ToLower().EndsWith(".png"))
            {
                // 注意：这里需要存储完整的绝对路径
                filePaths.Add(目录.PathJoin(fileName));
            }
            fileName = dir.GetNext();
        }

        // 4. 自然排序（防止 frame10 排在 frame2 前面）
        filePaths.Sort();

        // 5. 循环加载外部文件并转为 Texture
        foreach (var path in filePaths)
        {
            // 从磁盘读取字节数据
            var buffer = FileAccess.GetFileAsBytes(path);
            if (buffer == null || buffer.Length == 0) continue;

            // 创建 Image 并加载数据
            var img = new Image();
            var err = img.LoadPngFromBuffer(buffer);
        
            if (err == Error.Ok)
            {
                // 将 Image 转为 Godot 渲染可用的 ImageTexture
                var texture = ImageTexture.CreateFromImage(img);
                状态机.AddFrame(动画名, texture);
            }
            else
            {
                GD.PrintErr($"[解析失败] 无法加载图片: {path}, 错误代码: {err}");
            }
        }
    }
}

public static class 工具库
{    
// 定义一个静态随机数实例，以确保随机性并避免重复实例化
    private static readonly Random DefaultRand = new Random();
    /// <summary>
    /// 从列表中随机获取一项
    /// </summary>
    public static T 列表随机项<T>(this List<T> list, Random rand = null)
    {
        // 如果未传入参数，则使用默认的静态随机数实例
        rand ??= DefaultRand;
        if (list == null || list.Count == 0)
        {
            return default(T);
        }

        return list[rand.Next(list.Count)];
    }
}