> [English](README.md) | 中文

# revit-addin-toolkit

一个用 C# 写的 Revit 模型质检插件：打开模型 → 一键扫描 → 导出可交给建模同事整改的 CSV 报告。

面向的是 BIM 建模/深化阶段最常见的四类低级错误——**重复标记、构件缺标高、必填参数缺失、房间未放置或未命名**。这些问题靠人眼翻模型要花半天，脚本跑一遍只要几秒。

> 个人学习项目，MIT 许可。不含任何真实项目图纸、公司信息或内部路径。

## 环境要求

| 项 | 版本（2026-11-05 本机实测） |
| --- | --- |
| Revit | **2020（20.0.0.377）——本项目实际使用版本** |
| .NET | .NET Framework 4.8.1（Release 533509），编译目标 `net48`（Revit 2020 要求 4.7.2+） |
| 构建 | .NET SDK 10.0.203 / 10.0.402（`dotnet build`）；无需安装 Visual Studio |
| IDE（可选） | 本机为 Visual Studio Community 2026（18.11.0）；VS 2022 / Rider 同样能打开 `.csproj` |
| Revit API 引用 | 工程默认引用 `D:\Program Files\Autodesk\Revit 2020\` 的 DLL（RevitAPI.dll / RevitAPIUI.dll，`Private=false` 不打进包）；换机器时改 csproj 的 `RevitApiDir` 或用 `-p:RevitApiDir=…` 覆盖 |

## 构建

```powershell
# 仓库根目录
dotnet build src/BimQc.Addin/BimQc.Addin.csproj -c Release
```

产物：`bin/Release/BimQc.Addin.dll`。

Revit 装在其他路径时，用属性覆盖（默认值已是本机 Revit 2020 的路径）：

```powershell
dotnet build src/BimQc.Addin/BimQc.Addin.csproj -c Release `
  -p:RevitApiDir="D:\Program Files\Autodesk\Revit 2020\"
```

## 安装

1. 把 `BimQc.Addin.dll` 和仓库根目录的 `BimQc.addin` 复制到：
   `%AppData%\Autodesk\Revit\Addins\2020\`（本项目使用 Revit 2020）
2. DLL 必须是针对 Revit 2020 API 编译的（本工程默认即如此）；用其他大版本的 API 编出来的 DLL 放进 2020 会加载失败。
3. 重启 Revit，在「附加模块 → 外部工具」里找到 **BIM 模型质检**。

> `.addin` 与 `.dll` 必须在同一目录；Revit 只认这个固定路径，不能随便放。

## 使用

点击命令 → 弹出目录选择框 → 选一个输出目录 → 等几秒 → 弹窗给出汇总，同目录下生成 `模型质检_yyyyMMdd_HHmm.csv`。

报告列为：`类别, 构件ID, 族, 类型, 规则, 问题类型, 说明`。构件 ID 可以直接在 Revit 里「按 ID 选择」定位。

## 检查规则

| ID | 规则 | 判定 | 状态 |
| --- | --- | --- | --- |
| R1 | 重复标记 | 同一类别下多个构件共用同一个「标记」值；空标记不算 | ✅ |
| R2 | 构件缺标高 | 构件无法取得所属标高（房间/轴网/注释类本来就无标高，已排除） | ✅ |
| R3 | 必填参数缺失 | 区分「参数未定义」与「参数存在但为空」，默认检查 `标记`、`系统类型` | ✅ |
| R4 | 房间未放置/未命名 | 面积 = 0 视为未放置；面积正常但名称为空视为未命名 | ✅ |

规则是纯只读的：整个命令以 `TransactionMode.ReadOnly` 运行，插件不会改动模型任何一个参数。

## 目录结构

```
revit-addin-toolkit/
├─ BimQc.addin              # Revit 加载清单
├─ src/BimQc.Addin/
│  ├─ Command.cs            # 入口：采集 → 跑规则 → 导出 → 汇总
│  ├─ Models/Issue.cs       # 一条问题记录
│  ├─ Rules/                # 每条规则一个类，实现 ICheckRule
│  ├─ RevitWrap/            # 对 Revit API 的薄封装（取参数文本、标高名等）
│  └─ Reporting/            # CSV 报告输出
└─ docs/design-notes.zh-CN.md  # 分层、扩展方式、已知限制
```

## 路线图

- [x] v0.1 命令入口 + R1–R4 + CSV 报告
- [ ] v0.2 WPF 面板：结果表格、双击定位构件、按规则筛选
- [ ] v0.3 规则可配置（JSON 描述参数名与阈值），不再硬编码
- [ ] v0.4 HTML 报告 + 按楼层/专业的汇总统计
- [ ] v1.0 用真实脱敏模型跑通全流程，补 GIF 与截图

## 已知限制

- 只扫描当前文档，不处理链接模型与工作共享的本地副本差异。
- R3 的参数名目前写在 `Command.cs` 里，改参数名要重新编译（v0.3 会外置成配置）。
- 房间统计依赖 `Area` 参数，非面积型房间（如面积方案关闭）会全部报「未放置」。

## 许可

[MIT](LICENSE)
