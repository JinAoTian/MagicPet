extends RefCounted

func execute(info: Dictionary) -> void:
	var input_paths = info.get("in", [])
	var output_paths = info.get("inR", [])     # 新增：从 info 获取相对路径数组
	var output_dir = info.get("out", "")      # 输出目录
	var magick_path = info.get("tool", "")     # magick.exe 的路径
	var out_ext = info.get("outExt", "")       # 目标后缀 (例如 ".webp" 或 ".jpg")

	# 1. 基础检查
	if input_paths.size() == 0:
		printerr("错误: 输入路径列表为空")
		return
	
	if out_ext == "":
		printerr("错误: 目标格式为空")
		return

	# 2. 遍历所有输入路径
	var success_count = 0
	var fail_count = 0
	var result_paths = []
	for i in range(input_paths.size()):
		var input_path = input_paths[i]
		
		# 基础校验：输入路径不能为空，且 output_paths 必须有对应项
		if input_path == "" or i >= output_paths.size():
			continue

		# --- 修改后的代码 ---
		var relative_path = output_paths[i]
		# 使用 get_basename() 去掉旧后缀名 (例如 "a/b.png" 变成 "a/b")
		var path_without_ext = relative_path.get_basename()
		# 拼接新后缀名
		var output_path = output_dir.path_join(path_without_ext + out_ext)

		# 4. 自动创建输出子目录
		var base_dir = output_path.get_base_dir()
		if not DirAccess.dir_exists_absolute(base_dir):
			DirAccess.make_dir_recursive_absolute(base_dir)

		# 5. 准备 ImageMagick 参数
		var args = [
			input_path,
			output_path
		]

		# 6. 执行转换
		print("正在转换 [%d/%d]: " % [i + 1, input_paths.size()], input_path.get_file(), " -> ", relative_path + out_ext)
		
		var output = []
		var exit_code = OS.execute(magick_path, args, output, true)

		if exit_code == 0:
			success_count += 1
			result_paths.append(output_path)
		else:
			fail_count += 1
			printerr("转换失败: ", input_path, " 错误代码: ", exit_code)
			if output.size() > 0:
				print("详细错误: ", output[0])

	# 7. 打印最终结果总结
	info["result"] = result_paths
	print("-----------------------------------")
	print("格式转换完成！成功: %d, 失败: %d" % [success_count, fail_count])
