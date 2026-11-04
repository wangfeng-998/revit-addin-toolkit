using System.Collections.Generic;
using Autodesk.Revit.DB;
using BimQc.Models;
using BimQc.RevitWrap;

namespace BimQc.Rules
{
    /// <summary>R3：必填参数缺失（区分"参数未定义"与"参数存在但为空"）。</summary>
    public class MissingParamRule : ICheckRule
    {
        private readonly string[] _paramNames;

        public MissingParamRule(IEnumerable<string> paramNames)
        {
            _paramNames = new List<string>(paramNames).ToArray();
        }

        public string Id { get { return "R3"; } }

        public string Name { get { return "必填参数缺失"; } }

        public IEnumerable<Issue> Check(Document doc, IList<Element> elements)
        {
            var result = new List<Issue>();
            foreach (var el in elements)
            {
                foreach (var name in _paramNames)
                {
                    Parameter p = null;
                    try
                    {
                        p = el.LookupParameter(name);
                        if (p == null)
                        {
                            var tid = el.GetTypeId();
                            if (tid != null && tid != ElementId.InvalidElementId)
                            {
                                var t = doc.GetElement(tid);
                                if (t != null) p = t.LookupParameter(name);
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        p = null;
                    }

                    if (p == null)
                    {
                        result.Add(new Issue
                        {
                            Category = ElementInfo.CategoryName(el),
                            ElementId = el.Id.IntegerValue,
                            Family = ElementInfo.FamilyName(el),
                            TypeName = ElementInfo.TypeName(el),
                            RuleId = Id,
                            IssueType = "参数未定义",
                            Detail = name
                        });
                    }
                    else if (ElementInfo.Text(doc, p).Trim().Length == 0)
                    {
                        result.Add(new Issue
                        {
                            Category = ElementInfo.CategoryName(el),
                            ElementId = el.Id.IntegerValue,
                            Family = ElementInfo.FamilyName(el),
                            TypeName = ElementInfo.TypeName(el),
                            RuleId = Id,
                            IssueType = "未填写",
                            Detail = name
                        });
                    }
                }
            }
            return result;
        }
    }
}
