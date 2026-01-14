extends RefCounted

func execute(info: Dictionary) -> void:
	var input_paths = info.get("in", [])
	var output_paths = info.get("inR", []) 
	var output_dir = info.get("out", "")
	var magick_path = info.get("tool", "")
	var size = info.get("size", "")
	var size_str = info.get(size, size)
	var success_results = []

	for i in range(input_paths.size()):
		var input_path = input_paths[i]
		
		if input_path == "" or i >= output_paths.size():
			continue
			
		var relative_path = output_paths[i]
		var output_path = output_dir.path_join(relative_path)
		
		# --- 新增：确保输出目录存在 ---
		var base_dir = output_path.get_base_dir()
		if not DirAccess.dir_exists_absolute(base_dir):
			var err = DirAccess.make_dir_recursive_absolute(base_dir)
			if err != OK:
				print("错误：无法创建目录 ", base_dir, " 错误码：", err)
				continue # 如果目录创建失败，跳过此文件
		# ----------------------------
		
		var args = [input_path, "-resize", size_str, output_path]
		var output = []
		
		print("正在处理: ", input_path.get_file(), " -> ", relative_path)
		
		var exit_code = OS.execute(magick_path, args, output, true)
		
		if exit_code == 0:
			success_results.append(output_path)
			print("成功！已输出到: ", output_path)
		else:
			print("失败 (", relative_path, ")，退出码: ", exit_code)
			if output.size() > 0:
				print("错误详情: ", output[0])
	info["result"] = success_results
	print("所有任务处理完毕。")