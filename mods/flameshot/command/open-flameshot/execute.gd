extends RefCounted

func execute(info: Dictionary) -> void:
	var tool_path = info.get("tool", "")
	var out_dir = info.get("out", "") # 截图保存的目标文件夹
	
	# 1. 检查工具路径
	if tool_path == "":
		push_error("错误: 未提供 Flameshot 路径")
		return

	# --- 修改部分：生成 MM-DD-HH-MM-SS 格式的文件名 ---
	var time = Time.get_datetime_dict_from_system() # 注意这里改用 datetime 获取日期
	var file_name = "%02d-%02d-%02d-%02d-%02d.png" % [
		time.month, 
		time.day, 
		time.hour, 
		time.minute, 
		time.second
	]
	var full_path = out_dir.path_join(file_name) 
	# ----------------------------------------------

	# 2. 准备参数
	# 使用 full_path 确保 Flameshot 直接保存到指定文件
	var args: PackedStringArray = ["gui", "-p", full_path]
	
	if info.has("args"):
		args.append_array(info.get("args"))

	# 3. 打印调试信息
	print("执行命令: ", tool_path, " ", " ".join(args))

	# 4. 启动程序
	var output = []
	var exit_code = OS.execute(tool_path, args, output, true)

	# --- 修改部分：根据执行结果更新 info ---
	if exit_code == 0:
		print("截图任务已完成并保存至: ", full_path)
		# 将结果路径以列表形式存入 info["result"]
		info["result"] = [full_path]
	else:
		push_error("Flameshot 执行失败，退出代码: %d" % exit_code)
		if output.size() > 0:
			print("错误详情: ", output[0])
	# ---------------------------------------
