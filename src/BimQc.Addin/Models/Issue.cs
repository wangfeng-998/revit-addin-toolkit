using System;

namespace BimQc.Models
{
    /// <summary>一条检查问题。字段与 CSV 报告列一一对应，便于直接导出。</summary>
    public class Issue
    {
        public string Category { get; set; }
        public int ElementId { get; set; }
        public string Family { get; set; }
        public string TypeName { get; set; }
        public string RuleId { get; set; }
        public string IssueType { get; set; }
        public string Detail { get; set; }

        public Issue()
        {
            Category = string.Empty;
            Family = string.Empty;
            TypeName = string.Empty;
            RuleId = string.Empty;
            IssueType = string.Empty;
            Detail = string.Empty;
        }

        public string[] ToRow()
        {
            return new[]
            {
                Category,
                ElementId.ToString(),
                Family,
                TypeName,
                RuleId,
                IssueType,
                Detail
            };
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1} {2} ({3})", RuleId, Category, ElementId, IssueType);
        }
    }
}
