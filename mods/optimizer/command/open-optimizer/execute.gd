extends RefCounted

func execute(info: Dictionary) -> void:
    var tool_path = info.get("tool", "")
    
    # 1. 检查路径是否为空
    if tool_path == "":
        push_error("错误: 未提供工具路径 (tool path is empty)")
        return

    # 2. 准备参数 (如果有的话，通常放在 info 的 "args" 键中)
    var args: PackedStringArray = info.get("args", [])

    # 3. 启动外部程序
    # OS.execute 是同步执行（会阻塞 Godot 直到程序关闭）
    # 如果你想异步启动，请使用 OS.create_process
    
    var output = []
    var exit_code = OS.execute(tool_path, args, output, true)

    if exit_code == 0:
        print("优化器启动成功!")
        print("输出信息: ", output)
    else:
        push_error("优化器启动失败，退出代码: %d" % exit_code)