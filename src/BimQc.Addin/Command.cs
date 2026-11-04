using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimQc.Models;
using BimQc.Reporting;
using BimQc.Rules;
using BimQc.RevitWrap;

namespace BimQc
{
    /// <summary>
    /// 插件入口命令：采集构件 → 跑全部规则 → 选目录 → 导出 CSV → 汇总弹窗。
    /// 只读模式，不修改模型（写操作要单独开 Transaction）。
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class QcCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (commandData == null || commandData.Application == null) return Result.Failed;

            var uidoc = commandData.Application.ActiveUIDocument;
            if (uidoc == null)
            {
                message = "没有打开的文档，请先打开一个模型。";
                return Result.Failed;
            }

            var doc = uidoc.Document;
            List<Element> modelElements;
            try
            {
                modelElements = CollectModelElements(doc);
            }
            catch (Exception ex)
            {
                message = "采集构件失败：" + ex.Message;
                return Result.Failed;
            }

            var rules = new List<ICheckRule>
            {
                new DuplicateMarkRule(),
                new MissingLevelRule(),
                new MissingParamRule(new[] { "标记", "系统类型" }),
                new RoomCheckRule()
            };

            var issues = new List<Issue>();
            var perRule = new Dictionary<string, int>();
            foreach (var rule in rules)
            {
                List<Issue> found;
                try
                {
                    found = new List<Issue>(rule.Check(doc, modelElements));
                }
                catch (Exception)
                {
                    // 单条规则失败不影响其它规则，报告里能看到少了一条
                    continue;
                }
                perRule[rule.Id + " " + rule.Name] = found.Count;
                issues.AddRange(found);
            }

            var folder = PickFolder();
            if (string.IsNullOrEmpty(folder)) return Result.Cancelled;

            string path;
            try
            {
                path = CsvReporter.Write(folder, issues);
            }
            catch (Exception ex)
            {
                message = "写报告失败：" + ex.Message;
                return Result.Failed;
            }

            var lines = new List<string>
            {
                string.Format("检查构件：{0} 个", modelElements.Count),
                string.Format("问题合计：{0} 条", issues.Count),
                string.Empty
            };
            foreach (var pair in perRule)
            {
                lines.Add(string.Format("  {0}：{1}", pair.Key, pair.Value));
            }
            lines.Add(string.Empty);
            lines.Add("报告：" + path);

            TaskDialog.Show("BIM 模型质检", string.Join(Environment.NewLine, lines.ToArray()));
            return Result.Succeeded;
        }

        private static string PickFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择质检报告输出目录";
                dialog.ShowNewFolderButton = true;
                return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
            }
        }

        /// <summary>取所有"模型类别"下的实例构件。</summary>
        private static List<Element> CollectModelElements(Document doc)
        {
            var list = new List<Element>();
            foreach (Category cat in doc.Settings.Categories)
            {
                if (cat == null || cat.CategoryType != CategoryType.Model) continue;
                try
                {
                    var col = new FilteredElementCollector(doc)
                        .OfCategoryId(cat.Id)
                        .WhereElementIsNotElementType();
                    list.AddRange(col.ToElements());
                }
                catch (Exception)
                {
                    // 个别类别不可枚举，跳过
                }
            }
            return list;
        }
    }
}
