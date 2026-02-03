using System.Diagnostics.CodeAnalysis;
using desktop.script.Steam;
using desktop.script.UX;
using Godot;
using Godot.Collections;

namespace desktop.script.logic;

// ReSharper disable once InconsistentNaming
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class IO : Node
{
    public Dictionary Info = new ();//单例方便dialogue获取
    public Dictionary Global  = new();
    public static IO 单例;
    public static readonly ConfigFile 配置 = new();
    public const string 配置路径 = "user://settings.cfg";
    public override void _Ready()
    {
        单例 = this;
        var err = 配置.Load(配置路径);
        if (err == Error.Ok)
        {
            var lang = 配置.GetValue("Player", "lang", "").AsString();
            if (!string.IsNullOrEmpty(lang))
            {
                TranslationServer.SetLocale(lang);
            }
        }
    }
    public void set(string key, Variant value)
    {
        if(Info==null)return;
        Info[key] = value;
    }
    public Variant get(string key)
    {
        if (Info != null && Info.TryGetValue(key, out var value))
        {
            return value;
        }
        return default;
    }
    public bool get(string key, out Variant value)
    {
        if (Info != null && Info.TryGetValue(key, out value))
        {
            return true;
        }
    
        value = default;
        return false;
    }
    public void setG(string key, Variant value)
    {
        if(Global==null)return;
        Global[key] = value;
    }
    public Variant getG(string key)
    {
        if (Global != null && Global.TryGetValue(key, out var value))
        {
            return value;
        }
        return default;
    }
    public bool getG(string key, out Variant value)
    {
        if (Global != null && Global.TryGetValue(key, out value))
        {
            return true;
        }
        value = default;
        return false;
    }
    private AudioStreamPlayer _currentMasterPlayer;
    private string audioText;
    private int audiotxtCnt;
    private const float fadeTime = 1f;
    public void setAudioText(string text)
    {
        audioText = text;
        Main.结束标题 = true;
        Dialogue.延迟显示标题(text);
        audiotxtCnt++;
    }
    public void ChangeLang(string code)
    {
        // 注意：这里传递的是变量 code，而不是字符串 "code"
        CallDeferred(nameof(ChangeLangDef), code);
    }
    private void ChangeLangDef(string code)
    {
        TranslationServer.SetLocale(code);
        Context.显示指令列表();
        配置.SetValue("Player", "lang",code);
        配置.Save(配置路径);
    }
    public void stopAudioDef()
    {
        CallDeferred(nameof(stopAudio));
    }
    public void stopAudio()
    {
        // 关键点：将全局引用转为局部引用，防止执行过程中 _currentMasterPlayer 被换成新的
        var playerToStop = _currentMasterPlayer;
        _currentMasterPlayer = null; // 立即清空，表示当前没有“活跃”的主音乐了
        var cnt = audiotxtCnt;
        if (IsInstanceValid(playerToStop) && playerToStop.IsInsideTree())
        {
            var tween = GetTree().CreateTween();
            tween.TweenProperty(playerToStop, "volume_db", -80.0f, fadeTime)
                .SetTrans(Tween.TransitionType.Linear);

            tween.Finished += () => 
            {
                if (IsInstanceValid(playerToStop))
                {
                    playerToStop.Stop();
                    playerToStop.QueueFree();
                    // 触发结束回调
                    OnAudioFinished(cnt);
                }
            };
        }
    }
    public bool playAudio(string path, float volumeLinear = 1.0f)
    {
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"音频文件不存在: {path}");
            return false;
        }

        var stream = LoadAudioFromPath(path);
        if (stream == null) return false;

        // 立即执行停止逻辑，确保旧的播放器进入淡出流程
        // 注意：这里不在 Deferred 里调用，是为了确保变量立刻被重置
        stopAudio();

        Callable.From(() => {
            var player = new AudioStreamPlayer();
            _currentMasterPlayer = player; // 现在的 player 是全新的
            AddChild(player);

            player.Stream = stream;
            player.Bus = "Master";
            player.VolumeDb = Mathf.LinearToDb(volumeLinear);
            player.Finished += () => 
            {
                // 只有当这个 player 依然是“当前播放器”时才触发
                // 防止淡出销毁和自然结束销毁冲突
                if (IsInstanceValid(player))
                {
                    if (_currentMasterPlayer == player) _currentMasterPlayer = null;
                    OnAudioFinished(audiotxtCnt);
                    player.QueueFree();
                }
            };

            player.Play();
        }).CallDeferred();

        return true;
    }
    private static AudioStream LoadAudioFromPath(string path)
    {
        byte[] bytes = FileAccess.GetFileAsBytes(path);
        if (bytes == null || bytes.Length == 0) return null;

        string ext = path.GetExtension().ToLower();

        switch (ext)
        {
            case "mp3":
                return new AudioStreamMP3 { Data = bytes };
            case "ogg":
                return AudioStreamOggVorbis.LoadFromBuffer(bytes);
            case "wav":
                // WAV 外部加载通常需要处理 Header，这里保持简单赋值
                return new AudioStreamWav { Data = bytes };
            default:
                GD.PrintErr($"不支持的音频格式: {ext}");
                return null;
        }
    }
    private void OnAudioFinished(int cnt)
    {
        if (cnt == audiotxtCnt && !string.IsNullOrEmpty(audioText))
        {
            Dialogue.关闭指定标题(audioText);
            audioText = null;
        }
        //TODO:后续添加回调
    }

    #region steam

    // ReSharper disable once MemberCanBeMadeStatic.Global
    public void PublishItem(string path)
    {
        var _ = WorkShop.PublishItem(path).GetAwaiter().GetResult();
    }

    #endregion
}