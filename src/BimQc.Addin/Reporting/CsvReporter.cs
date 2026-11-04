using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BimQc.Models;

namespace BimQc.Reporting
{
    /// <summary>CSV 报告导出（UTF-8 BOM，Excel 双击不乱码）。后续扩展 HTML/JSON 时另开类。</summary>
    public static class CsvReporter
    {
        public static readonly string[] Header =
        {
            "类别", "构件ID", "族", "类型", "规则", "问题类型", "说明"
        };

        public static string Write(string folder, IEnumerable<Issue> issues)
        {
            var path = Path.Combine(folder,
                "模型质检_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv");

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", Escape(Header)));
            var count = 0;
            foreach (var issue in issues)
            {
                sb.AppendLine(string.Join(",", Escape(issue.ToRow())));
                count++;
            }

            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
            return path;
        }

        private static string[] Escape(string[] cells)
        {
            var outCells = new string[cells.Length];
            for (var i = 0; i < cells.Length; i++)
            {
                var s = cells[i] ?? string.Empty;
                if (s.IndexOf(',') >= 0 || s.IndexOf('"') >= 0 || s.IndexOf('\n') >= 0 || s.IndexOf('\r') >= 0)
                {
                    s = "\"" + s.Replace("\"", "\"\"") + "\"";
                }
                outCells[i] = s;
            }
            return outCells;
        }
    }
}
