> [English](design-notes.md) | 中文

# 设计说明

## 为什么这样分层

```
Command.cs          编排：采集 → 跑规则 → 选目录 → 导出 → 汇总弹窗
  ├─ Rules/         每条规则一个类，只负责"判定并产出 Issue"
  ├─ RevitWrap/     对 Revit API 的薄封装，隔离 null 检查与单位/文本转换
  └─ Reporting/     把 Issue 写成文件，与 Revit 完全解耦
```

目的只有一个：**加一条规则不用动别的地方**。规则要能被单独阅读、单独测试、单独删掉。

## 规则契约

```csharp
public interface ICheckRule
{
    string Id { get; }
    string Name { get; }
    IEnumerable<Issue> Check(Document doc, IList<Element> elements);
}
```

- `elements` 来自 `CollectModelElements`：遍历 `doc.Settings.Categories` 中 `CategoryType.Model` 的类别，取全部非类型实例。
- 规则**只读**：不开启 Transaction，不写参数。整个命令声明为 `TransactionMode.ReadOnly`，写操作会直接抛异常——这是有意的保险。
- 规则返回 `List<Issue>` 而不是 `yield return`：迭代器里不能带 try/catch 的写法容易踩坑，且规则内部要 catch 掉单个构件的异常。

## 加一条新规则的三步

1. 在 `Rules/` 新建 `XxxRule.cs`，实现 `ICheckRule`。
2. 用 `ElementInfo` 取值（`Text` / `Value` / `LevelName` / `CategoryName` …），它们已经把 `null`、格式化文本、单位换算处理过。
3. 在 `Command.cs` 的 `rules` 列表里 `new` 一行。

## Issue 模型

```csharp
class Issue {
    string Category; int ElementId; string Family; string TypeName;
    string RuleId; string IssueType; string Detail;
}
```

`RuleId` + `IssueType` 是给人看的分类，`Detail` 写清"为什么算问题、涉及哪些 ID"。报告面向的是要照着改模型的同事，不是给程序读的。

## 错误处理策略

- **单条规则抛异常**：`Command` 里 catch 后跳过该规则，其余规则照跑。宁可报告少一条，也不要整个命令失败。
- **单构件取值异常**：规则内部跳过该构件。
- **采集类别异常**：跳过该类别。

Revit API 在元素失效、参数被删除时会抛 `InvalidOperationException` / `Autodesk.Revit.Exceptions.ArgumentException`，这些都在上述兜底范围内。

## 已知限制与后续

| 限制 | 计划 |
| --- | --- |
| 参数名硬编码在 Command 里 | v0.3 外置为 JSON 配置 |
| 只有 CSV，无定位交互 | v0.2 WPF 面板 + 双击选中构件 |
| 不处理链接模型 | 需要 `FilteredElementCollector(doc, linkId)` 与坐标转换，单独一期 |
| 无单元测试 | 规则层已与 Revit 解耦到只需 `Element`，可为 `ElementInfo` 做假实现 |
