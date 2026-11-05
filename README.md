> English | [中文](README.zh-CN.md)

# revit-addin-toolkit

A Revit model QC add-in written in C#: open a model → one-click scan → export a CSV report that can be handed to modeling colleagues for fixing.

It targets the four most common low-level mistakes in BIM modeling and detailing—**duplicate marks, elements missing a level, missing required parameters, and unplaced or unnamed rooms**. Checking a model by eye takes half a day; one script run takes seconds.

> Personal learning project, MIT licensed. Contains no real project drawings, company information, or internal paths.

## Requirements

| Item | Version (measured on this machine, 2026-11-05) |
| --- | --- |
| Revit | **2020 (20.0.0.377) — the version this project actually uses** |
| .NET | .NET Framework 4.8.1 (Release 533509), compilation target `net48` (Revit 2020 requires 4.7.2+) |
| Build | .NET SDK 10.0.203 / 10.0.402 (`dotnet build`); Visual Studio is not required |
| IDE (optional) | Visual Studio Community 2026 (18.11.0) on this machine; VS 2022 / Rider can also open the `.csproj` |
| Revit API reference | The project references the DLLs under `D:\Program Files\Autodesk\Revit 2020\` by default (RevitAPI.dll / RevitAPIUI.dll, `Private=false` so they are not packed); on another machine, change `RevitApiDir` in the csproj or override it with `-p:RevitApiDir=…` |

## Build

```powershell
# repository root
dotnet build src/BimQc.Addin/BimQc.Addin.csproj -c Release
```

Output: `bin/Release/BimQc.Addin.dll`.

If Revit is installed elsewhere, override the property (the default is already this machine's Revit 2020 path):

```powershell
dotnet build src/BimQc.Addin/BimQc.Addin.csproj -c Release `
  -p:RevitApiDir="D:\Program Files\Autodesk\Revit 2020\"
```

## Installation

1. Copy `BimQc.Addin.dll` and `BimQc.addin` from the repository root into:
   `%AppData%\Autodesk\Revit\Addins\2020\` (this project uses Revit 2020)
2. The DLL must be compiled against the Revit 2020 API (this project does so by default); a DLL built against another major version's API will fail to load in 2020.
3. Restart Revit and find **BIM 模型质检** (BIM Model QC) under "Add-Ins → External Tools".

> The `.addin` and the `.dll` must be in the same directory; Revit only accepts this fixed path, so they cannot be placed arbitrarily.

## Usage

Click the command → a directory picker appears → choose an output directory → wait a few seconds → a dialog shows the summary, and `模型质检_yyyyMMdd_HHmm.csv` is written to the same directory (the file name prefix is Chinese for "model QC").

The report columns are: `类别, 构件ID, 族, 类型, 规则, 问题类型, 说明` (Category, Element ID, Family, Type, Rule, Issue Type, Detail). An element ID can be located directly in Revit with "Select by ID".

## Check rules

| ID | Rule | Criterion | Status |
| --- | --- | --- | --- |
| R1 | Duplicate marks | Multiple elements in the same category share one "Mark" value; an empty mark does not count | ✅ |
| R2 | Element missing a level | The element has no resolvable level (rooms, grids, and annotations have no level by nature and are excluded) | ✅ |
| R3 | Missing required parameters | Distinguishes "parameter not defined" from "parameter exists but is empty"; checks `标记` (Mark) and `系统类型` (System Type) by default | ✅ |
| R4 | Unplaced/unnamed rooms | Area = 0 is treated as unplaced; a normal area with an empty name is treated as unnamed | ✅ |

The rules are strictly read-only: the whole command runs with `TransactionMode.ReadOnly`, and the add-in does not change a single parameter in the model.

## Project layout

```
revit-addin-toolkit/
├─ BimQc.addin              # Revit add-in manifest
├─ src/BimQc.Addin/
│  ├─ Command.cs            # Entry point: collect → run rules → export → summarize
│  ├─ Models/Issue.cs       # A single issue record
│  ├─ Rules/                # One class per rule, implementing ICheckRule
│  ├─ RevitWrap/            # Thin wrapper over the Revit API (parameter text, level names, etc.)
│  └─ Reporting/            # CSV report output
└─ docs/design-notes.md     # Layering, extension points, known limitations
```

## Roadmap

- [x] v0.1 Command entry + R1–R4 + CSV report
- [ ] v0.2 WPF panel: results table, double-click to locate an element, filter by rule
- [ ] v0.3 Configurable rules (JSON describes parameter names and thresholds), no longer hard-coded
- [ ] v0.4 HTML report + summary statistics by level/discipline
- [ ] v1.0 Run the full workflow on a real anonymized model, add GIFs and screenshots

## Known limitations

- Only the current document is scanned; linked models and local-copy differences in worksharing are not handled.
- R3's parameter names are currently written in `Command.cs`, so changing a parameter name requires a rebuild (v0.3 will externalize them into configuration).
- Room statistics rely on the `Area` parameter; non-area rooms (e.g. when the area scheme is off) will all be reported as "unplaced".

## License

[MIT](LICENSE)
