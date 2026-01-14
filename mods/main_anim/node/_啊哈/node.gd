extends Node

# 当节点进入场景树并准备就绪时触发
func _ready():
	print("--- 外部脚本测试成功 ---")
	print("节点名称: ", name)
	print("运行时间: ", Time.get_time_string_from_system())
	
	# 可以在这里改变节点属性证明脚本在运行
	if get_parent() == get_tree().root:
		print("提示：我目前被挂载在 Root 根节点下。")