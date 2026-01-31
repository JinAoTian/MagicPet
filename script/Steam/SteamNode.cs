using System;
using Godot;
using Steamworks;

// 确保引用了命名空间

namespace desktop.script.Steam;

public partial class SteamNode : Node
{
    // 替换为你的游戏 AppId，开发测试可用 480 (Spacewar)
    private const uint AppId = 4372100;

    public override void _Ready()
    {
        try
        {
            // 1. 初始化 Steam 客户端
            // 注意：如果 Steam 没打开，这里会抛出异常
            SteamClient.Init(AppId);

            if (SteamClient.IsValid)
            {
                GD.Print($"Steam 已连接: {SteamClient.Name} (ID: {SteamClient.SteamId})");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr("Steam 初始化失败: " + e.Message);
            // 可以在这里实现：如果 Steam 没启动则关闭游戏或进入离线模式
        }
    }

    public override void _Process(double delta)
    {
        // 2. 驱动生命周期的核心：每帧运行回调
        // 这样 Steam 的事件（如成就解锁、好友邀请）才会分发到你的 C# 代码中
        if (SteamClient.IsValid)
        {
            SteamClient.RunCallbacks();
        }
    }

    public override void _ExitTree()
    {
        // 3. 游戏退出时正确关闭 Steam 连接
        SteamClient.Shutdown();
        GD.Print("Steam 已安全关闭");
    }
}