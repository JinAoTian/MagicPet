<p align="center">
  <img src="./icon/icon.png" width="200" alt="MagicPet Logo">
</p>

# MagicPet - 魔法桌宠

[![License](https://img.shields.io/badge/License-GPL%203.0-blue.svg)](https://opensource.org/licenses/GPL-3.0)
[![Engine](https://img.shields.io/badge/Engine-Godot-orange.svg)](https://godotengine.org/)

**MagicPet** 是一款基于 Godot 开发的轻量化桌宠工具箱。其核心定位是一个具备桌宠外壳的 **CLI 与自动化脚本启动器**。

通过内置的 GDScript 支持，用户可以实现极高自由度的模组化定制，将复杂的自动化任务集成于灵动的桌面角色之中。

> **素材致谢**：美术资源引用自开源项目 [VPet](https://github.com/LorisYounger/VPet)。

---

## 🛠 功能与操作

### 交互指令

* **缩放**：滚动鼠标滑轮。
* **移动**：左键点击并拖拽人物。
* **菜单**：右键点击呼出指令面板。
* **交互**：支持直接拖放文件至人物，或点击后通过 `Ctrl + V` 粘贴内容。

### 输入支持

* **文件路径**：支持鼠标拖入或剪切板读取（兼容目录及快捷方式）。
* **文本信息**：支持剪切板纯文本粘贴。
* **网络链接**：自动识别剪切板中的 URL 地址。

---

## ⚙️ 配置与扩展

您可以通过右键菜单进入“配置”界面进行个性化设置：

* **外部工具**：可执行文件存放在 `/bin` 目录下。
* **脚本逻辑**：模组逻辑位于 `/mods` 目录。
  * *提示：在目录名前添加下划线 `_` 即可快速禁用该模组。*
* **模组开发**：结构设计直观，建议参考现有目录实现自定义功能，详细教程待后续更新。

---

## 📦 集成开源工具清单

项目预集成了以下优秀开源工具，以强化自动化处理能力：

| 工具名称            | 说明             | 链接                                                   |
|:--------------- |:-------------- |:---------------------------------------------------- |
| **CopyQ**       | 强大的剪切板管理器      | [GitHub](https://github.com/hluk/CopyQ)              |
| **Everything**  | 毫秒级全盘文件搜索      | [Voidtools](https://www.voidtools.com/)              |
| **Flameshot**   | 功能丰富的跨平台截图工具   | [GitHub](https://github.com/flameshot-org/flameshot) |
| **ImageMagick** | 命令行图像处理工具集     | [GitHub](https://github.com/ImageMagick/ImageMagick) |
| **Optimizer**   | Windows 系统优化工具 | [GitHub](https://github.com/hellzerg/optimizer)      |
| **yt-dlp**      | 视频下载命令行工具      | [GitHub](https://github.com/yt-dlp/yt-dlp)           |

---

## 📝 注意

* **项目现状**：项目为学习godot时顺手做的,包含较多面条代码与 AI 生成代码，后续会重构，暂不建议深入阅读。
* **模组门槛**：外部执行逻辑全由 AI 辅助生成，侧面验证了本项目模组开发具备极低的准入门槛。
* **说明文档由ai细化，能看就行=-=**

---

## 📜 开源协议

本项目遵循 **GPL-3.0** 开源协议。