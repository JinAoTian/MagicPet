extends RefCounted

func execute(info: Dictionary) -> void:
	var urls = info.get("in", [])           # 完整网址
	var output_dir = info.get("out", "")    # 输出根目录
	var yt_dlp = info.get("tool", "")       # yt-dlp.exe 的路径
	var results = []
	
	# 确保输出目录存在
	if not DirAccess.dir_exists_absolute(output_dir):
		DirAccess.make_dir_recursive_absolute(output_dir)
	
	for url in urls:
		# 准备参数
		# --playlist-items 1: 确保如果是列表只下第一个
		# --print after_move:filepath: 下载完成后打印最终的绝对路径
		# -P: 指定输出目录
		var args = [
			url,
			"--playlist-items", "1",
			"-P", ProjectSettings.globalize_path(output_dir),
			"--print", "after_move:filepath",
			"--no-warnings"
		]
	
		var output = []
		# 执行同步下载
		var exit_code = OS.execute(yt_dlp, args, output)
	
		if exit_code == 0 and output.size() > 0:
			# yt-dlp 打印的路径通常带有换行符，需要 strip
			var absolute_path = output[0].strip_edges()
			if not absolute_path.is_empty():
				results.append(absolute_path)
		else:
			printerr("下载失败或 URL 无效: ", url)
	
	# 将结果存回 info 或根据你的框架逻辑处理
	info["results"] = results
	print("下载任务完成，成功获取路径数量: ", results.size())