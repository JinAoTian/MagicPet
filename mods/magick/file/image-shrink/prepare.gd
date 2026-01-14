extends RefCounted

func prepare(info: Dictionary) -> void:
	if(!(info.get("single", false))):
		return
	var input_path = info.get("in", [""])[0]
	if input_path == "":
		push_error("InputPath 不能为空")
		return
		
	var img = Image.load_from_file(input_path)
	if img:
		var w = img.get_width()
		var h = img.get_height()
		
		info["width"] = w
		info["height"] = h
		
		# 1. 确定原始最短边
		var max_side = max(w, h)
		info["max"] = max_side
		info["T"] = false
		# 2. 定义目标缩放尺寸
		var target_steps = [32, 64, 128, 256, 512]
		
		# 3. 确定缩放比例
		var is_width_longer = w >= h
		# 计算比例：长边 / 短边
		var aspect_ratio = float(h) / float(w) if is_width_longer else float(w) / float(h)
		
		for step in target_steps:
			# --- 新增逻辑：若目标尺寸大于原始最短边，则跳过 ---
			if step > max_side:
				continue
			
			var new_w: int
			var new_h: int
			
			if is_width_longer:
				new_w = step
				new_h = round(step * aspect_ratio)
			else:
				new_h = step
				new_w = round(step * aspect_ratio)
			
			# 存入 info，例如 info["32"] = "32*64"
			info[str(step)] = str(new_w) + "x" + str(new_h)
			
	else:
		push_error("无法加载图片文件: " + input_path)