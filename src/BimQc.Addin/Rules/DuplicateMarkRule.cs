using System.Collections.Generic;
using Autodesk.Revit.DB;
using BimQc.Models;
using BimQc.RevitWrap;

namespace BimQc.Rules
{
    /// <summary>R1：同一类别下"标记(Mark)"重复。空标记不算问题。</summary>
    public class DuplicateMarkRule : ICheckRule
    {
        public string Id { get { return "R1"; } }

        public string Name { get { return "重复标记"; } }

        public IEnumerable<Issue> Check(Document doc, IList<Element> elements)
        {
            var result = new List<Issue>();
            var bucket = new Dictionary<string, List<Element>>();

            foreach (var el in elements)
            {
                var mark = ElementInfo.Value(el, BuiltInParameter.ALL_MODEL_MARK).Trim();
                if (mark.Length == 0) continue;

                var key = ElementInfo.CategoryName(el) + "|" + mark;
                List<Element> list;
                if (!bucket.TryGetValue(key, out list))
                {
                    list = new List<Element>();
                    bucket[key] = list;
                }
                list.Add(el);
            }

            foreach (var pair in bucket)
            {
                var list = pair.Value;
                if (list.Count < 2) continue;

                var ids = new List<string>();
                for (var i = 0; i < list.Count && i < 8; i++)
                {
                    ids.Add(list[i].Id.IntegerValue.ToString());
                }

                foreach (var el in list)
                {
                    result.Add(new Issue
                    {
                        Category = ElementInfo.CategoryName(el),
                        ElementId = el.Id.IntegerValue,
                        Family = ElementInfo.FamilyName(el),
                        TypeName = ElementInfo.TypeName(el),
                        RuleId = Id,
                        IssueType = "重复标记",
                        Detail = string.Format("同类别下 {0} 个构件共用此标记（ID: {1}）",
                            list.Count, string.Join("/", ids.ToArray()))
                    });
                }
            }
            return result;
        }
    }
}
