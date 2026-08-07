# MacBridgeWin

<p align="center">
  <a href="https://github.com/madscirick/MacBridgeWin/releases/download/v0.1.0/MacBridgeWin-v0.1.0-win-x64.zip">
    <img alt="下载 MacBridgeWin v0.1.0 Windows 版" src="https://img.shields.io/badge/普通用户下载-MacBridgeWin%20v0.1.0-1677FF?style=for-the-badge&logo=windows11&logoColor=white">
  </a>
</p>

<p align="center"><strong>Windows 10/11 · 版本 v0.1.0 · 发布日期 2026-08-07</strong></p>

## Windows 用户下载

**普通用户请下载已经构建好的运行版本，不要下载仓库源码包。**

### [下载 MacBridgeWin v0.1.0 Windows 版（ZIP）](https://github.com/madscirick/MacBridgeWin/releases/download/v0.1.0/MacBridgeWin-v0.1.0-win-x64.zip)

1. 下载上面的 ZIP，并完整解压到一个文件夹。
2. 在解压后的文件夹中运行 `MacBridgeWin.App.exe`。
3. 程序启动后位于 Windows 右下角通知区域，请从托盘菜单打开设置。

> [!WARNING]
> 除非你准备自己编译程序，否则不要使用 GitHub 绿色 **Code → Download ZIP** 按钮，也不要下载自动生成的 **Source code (zip)**。这些文件是源码，不是可以直接运行的软件。

[查看 v0.1.0 Release 页面](https://github.com/madscirick/MacBridgeWin/releases/tag/v0.1.0) · [SHA-256 校验文件](https://github.com/madscirick/MacBridgeWin/releases/download/v0.1.0/MacBridgeWin-v0.1.0-win-x64.zip.sha256)

**发布日期：** 2026-08-07

MacBridgeWin 是一款面向经常在 macOS 与 Windows 之间切换的用户的轻量 Windows 10/11 托盘工具。它让部分操作更接近 macOS，同时尽量保留 Windows 原生行为。

> **当前状态：** 早期版本。功能已经可以使用，但目前没有安装程序和代码签名。

## 主要功能

- 将指定的 `Alt` 快捷键映射为常用 `Ctrl` 快捷键，同时保留 `Ctrl`、`Alt+Tab` 和 `Alt+F4`。
- 支持鼠标右键手势和 `左,上` 等多段手势。
- 未识别出有效手势时恢复普通右键点击。
- 托盘运行，提供设置窗口、应用专属配置、开机启动和配置导入/导出。

## 从源码运行

需要 Windows 10/11 和 `global.json` 指定的 .NET 8 SDK：

```powershell
dotnet build MacBridgeWin.sln -c Release
dotnet run --project src\MacBridgeWin.App\MacBridgeWin.App.csproj -c Release
```

程序启动后位于通知区域。请从托盘菜单打开设置；键盘映射和鼠标手势默认关闭。

## 使用发布版本

1. 从 GitHub Releases 下载 `MacBridgeWin-<版本>-win-x64.zip`。
2. 可使用同名 `.sha256` 文件核对下载文件。
3. 完整解压后运行 `MacBridgeWin.App.exe`。

当前可执行文件尚未进行代码签名，因此 Windows SmartScreen 可能显示提醒。请先核对下载来源、源码和校验值，再决定是否运行。不要为了安装本软件而关闭 Windows 安全软件。

卸载时，先在设置中关闭开机启动，再从托盘退出程序并删除解压目录。本地配置和日志位于 `%LocalAppData%\MacBridgeWin`，需要时可手动删除。

## 隐私与安全

- 所有快捷键和鼠标手势处理都在本机进行。
- 没有账号、遥测、分析、广告或网络服务，不上传输入数据。
- 不保存用户输入的文字。诊断日志会记录程序生命周期、已配置的快捷键/手势名称和异常信息，但不会记录前台进程名称或配置的应用路径。
- 配置和日志保存在 `%LocalAppData%\MacBridgeWin`。导出的配置可能包含用户主动选择的应用名称或路径，请在分享前检查。

完整说明见 [PRIVACY.md](PRIVACY.md)，安全问题请按 [SECURITY.md](SECURITY.md) 私下报告。

## 参与项目

提交问题前请阅读 [CONTRIBUTING.md](CONTRIBUTING.md)。项目采用 [MIT License](LICENSE)，版本记录见 [CHANGELOG.md](CHANGELOG.md)，路线图见 [PLAN.md](PLAN.md)。
