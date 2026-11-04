using System.Collections.Generic;
using Autodesk.Revit.DB;
using BimQc.Models;
using BimQc.RevitWrap;

namespace BimQc.Rules
{
    /// <summary>R2：构件拿不到所在标高（房间、轴网、注释类本来就无标高，跳过）。</summary>
    public class MissingLevelRule : ICheckRule
    {
        private static readonly HashSet<string> SkipCategories = new HashSet<string>
        {
            "房间", "面积", "轴网", "标高", "参照平面",
            "文字", "尺寸标注", "详图", "线", "填充区域"
        };

        public string Id { get { return "R2"; } }

        public string Name { get { return "构件缺标高"; } }

        public IEnumerable<Issue> Check(Document doc, IList<Element> elements)
        {
            var result = new List<Issue>();
            foreach (var el in elements)
            {
                var cat = ElementInfo.CategoryName(el);
                if (SkipCategories.Contains(cat)) continue;
                if (!string.IsNullOrEmpty(ElementInfo.LevelName(el))) continue;

                result.Add(new Issue
                {
                    Category = cat,
                    ElementId = el.Id.IntegerValue,
                    Family = ElementInfo.FamilyName(el),
                    TypeName = ElementInfo.TypeName(el),
                    RuleId = Id,
                    IssueType = "缺标高",
                    Detail = "构件未关联标高，无法按层统计"
                });
            }
            return result;
        }
    }
}
