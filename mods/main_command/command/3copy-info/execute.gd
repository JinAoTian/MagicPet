extends RefCounted

func execute(info: Dictionary) -> void:
    var copy_info = info.get("info", "")
    
    if copy_info != "":
        # 关键修改：通过 call_deferred 确保在主线程执行剪切板写入
        # 这样即使 execute 是在 Task/Thread 中运行，也不会阻塞任务或导致主线程卡顿
        DisplayServer.call_deferred("clipboard_set", copy_info)
        
        # 如果需要输出打印，也建议 deferred 打印
        print_deferred("已成功加入剪切板队列: " + str(copy_info))

func print_deferred(msg: String) -> void:
    print(msg)