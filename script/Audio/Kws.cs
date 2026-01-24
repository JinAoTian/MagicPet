using System;
using System.Collections.Generic;
using System.IO;
// 用于内存操作
using desktop.script.logic;
using Godot;
using SherpaOnnx; // 确保引用了 SherpaOnnx 命名空间

namespace desktop.script.Audio;

public partial class Kws : Node
{
    private static KeywordSpotter _spotter;
    private static OnlineStream _stream;
    private static AudioEffectCapture _effectCapture;
    public static readonly List<string> 关键词列表 = new();
    public static readonly Dictionary<string, 关键词信息> 关键词映射 = new();
    public static void TurnOn()
    {        
        var modelPaths = Main.工具路径字典;
        // 1. 安全检查路径
        if (!modelPaths.TryGetValue("KWS-decoder", out var decoderPath)) return;
        if (!modelPaths.TryGetValue("KWS-encoder", out var encoderPath)) return;
        if (!modelPaths.TryGetValue("KWS-joiner", out var joinerPath)) return;
        if (!modelPaths.TryGetValue("KWS-tokens", out var tokensPath)) return;
        if (!modelPaths.TryGetValue("KWS-keywords", out var keywordsPath)) return;
        try 
        {
            // 2. 配置 KeywordSpotter
            var config = new KeywordSpotterConfig();
            
            // 配置 Transducer 模型路径
            config.ModelConfig.Transducer.Encoder = encoderPath;
            config.ModelConfig.Transducer.Decoder = decoderPath;
            config.ModelConfig.Transducer.Joiner = joinerPath;
            config.ModelConfig.Tokens = tokensPath;
            config.ModelConfig.NumThreads = 1; // 建议先设为1调试
            config.ModelConfig.Debug = 0;      // 开启调试日志，这能让你在控制台看到崩溃前的最后输出

            File.WriteAllLines(keywordsPath,关键词列表);
            config.KeywordsFile = keywordsPath; 
            config.KeywordsThreshold = 0.05f;
            // 初始化 Spotter
            _spotter = new KeywordSpotter(config);
            _stream = _spotter.CreateStream();
            
            GD.Print("Sherpa-Onnx KWS 模型加载成功");
            SetupAudio();
        }
        catch (Exception e)
        {
            GD.PrintErr($"KWS 初始化失败: {e.Message}");
        }
    }

    private static void SetupAudio()
    {
        // 获取名为 "Record" 的总线索引
        var busIndex = AudioServer.GetBusIndex("Record");
        if (busIndex < 0)
        {
            GD.PrintErr("未找到名为 'Record' 的音频总线，请在 Audio 面板中创建。");
            return;
        }

        // 获取 Capture 效果器 (假设它是该总线上的第一个效果，索引为 0)
        _effectCapture = AudioServer.GetBusEffect(busIndex, 0) as AudioEffectCapture;
        if (_effectCapture == null)
        {
            GD.PrintErr("请在 'Record' 总线上添加 AudioEffectCapture 效果。");
        }
    }

    public override void _Process(double delta)
    {
        if (_spotter == null || _stream == null || _effectCapture == null) return;

        ProcessAudio();
    }

    private void ProcessAudio()
    {
        // 1. 获取麦克风缓冲区的帧数
        var framesAvailable = _effectCapture.GetFramesAvailable();
        if (framesAvailable <= 0) return;

        // 2. 读取音频数据 (Vector2[] 包含左右的声道)
        var buffer = _effectCapture.GetBuffer(framesAvailable);
        
        // 3. 将 Stereo (双声道) 转为 Mono (单声道) 并转为 float 数组
        // SherpaOnnx 需要 float[] 格式的单声道数据
        float[] samples = new float[buffer.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            // 取平均值混合声道，或者只取左声道 buffer[i].X
            samples[i] = (buffer[i].X + buffer[i].Y) / 2.0f;
        }

        // 4. 将音频数据喂给 Sherpa-Onnx
        // 注意：Godot 默认采样率通常是 44100 或 48000。
        // 如果你的模型是 16000，你需要在这里做重采样，或者在 Sherpa 配置中允许重采样。
        // 这里假设 samples 已经是合适的采样率，或者模型支持该采样率。
        _stream.AcceptWaveform((int)AudioServer.GetMixRate(), samples);

        // 5. 解码检测
        while (_spotter.IsReady(_stream))
        {
            _spotter.Decode(_stream);
            var result = _spotter.GetResult(_stream);
            
            if (!string.IsNullOrEmpty(result.Keyword))
            {
                
                GetKey(result.Keyword);
                
                _stream.Dispose(); // 释放旧流
                _stream = _spotter.CreateStream();
            }
        }
    }

    private static void GetKey(string keyword)
    {
        // 可以在这里通过事件总线发送信号，或者直接执行逻辑
        GD.Print($"激活 - {keyword}");
        if (关键词映射.TryGetValue(keyword,out var 关键词信息))
        {
            if (关键词信息.info!=null)
            {
                foreach (var (k,v) in 关键词信息.info)
                {
                    IO.单例.set(k,v);
                }
            }
            Main.选择脚本(关键词信息.对应脚本);
        }
        //_ = Dialogue.显示临时标题("我在");
        // 示例：调用 Main 中的逻辑
        // Main.Instance.OnVoiceCommandReceived();
    }
    
    // 释放资源
    public override void _ExitTree()
    {
        _stream?.Dispose();
        _spotter?.Dispose();
    }
}