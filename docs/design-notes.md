> English | [中文](design-notes.zh-CN.md)

# Design Notes

## Why this layering

```
Command.cs          orchestration: collect → run rules → pick directory → export → summary dialog
  ├─ Rules/         one class per rule, responsible only for "decide and emit an Issue"
  ├─ RevitWrap/     thin wrapper over the Revit API, isolating null checks and unit/text conversion
  └─ Reporting/     writes Issues to a file, fully decoupled from Revit
```

There is only one goal: **adding one rule should not require touching anything else**. A rule must be readable, testable, and removable on its own.

## Rule contract

```csharp
public interface ICheckRule
{
    string Id { get; }
    string Name { get; }
    IEnumerable<Issue> Check(Document doc, IList<Element> elements);
}
```

- `elements` comes from `CollectModelElements`: it walks the categories of `CategoryType.Model` in `doc.Settings.Categories` and takes all non-type instances.
- Rules are **read-only**: they open no Transaction and write no parameters. The whole command is declared as `TransactionMode.ReadOnly`, so a write throws immediately—an intentional safety net.
- Rules return `List<Issue>` instead of `yield return`: iterators that cannot carry try/catch are easy to get wrong, and each rule must catch exceptions from individual elements.

## Three steps to add a new rule

1. Create `XxxRule.cs` under `Rules/` and implement `ICheckRule`.
2. Read values with `ElementInfo` (`Text` / `Value` / `LevelName` / `CategoryName` …); it already handles `null`, formatted text, and unit conversion.
3. Add one `new` line to the `rules` list in `Command.cs`.

## Issue model

```csharp
class Issue {
    string Category; int ElementId; string Family; string TypeName;
    string RuleId; string IssueType; string Detail;
}
```

`RuleId` + `IssueType` are human-facing categories, and `Detail` spells out "why this counts as a problem and which IDs are involved". The report is aimed at colleagues who will fix the model accordingly, not at programs that read it.

## Error handling strategy

- **A single rule throws**: `Command` catches it and skips that rule; the rest still run. Better to lose one rule from the report than to fail the whole command.
- **A single element read throws**: the rule skips that element.
- **A category collection throws**: that category is skipped.

The Revit API throws `InvalidOperationException` / `Autodesk.Revit.Exceptions.ArgumentException` when an element becomes invalid or a parameter is deleted, and all of these fall within the fallbacks above.

## Known limitations and next steps

| Limitation | Plan |
| --- | --- |
| Parameter names hard-coded in Command | Externalize as JSON configuration in v0.3 |
| CSV only, no locate interaction | v0.2 WPF panel + double-click to select an element |
| Linked models not handled | Requires `FilteredElementCollector(doc, linkId)` and coordinate transforms; a separate phase |
| No unit tests | The rule layer is decoupled from Revit down to just `Element`, so a fake implementation can be built for `ElementInfo` |
