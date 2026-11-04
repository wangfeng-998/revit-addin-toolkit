using System.Collections.Generic;
using Autodesk.Revit.DB;
using BimQc.Models;

namespace BimQc.Rules
{
    /// <summary>
    /// 一条检查规则的统一接口。新增规则 = 新建一个实现类 + 在 Command 里注册，
    /// 不改动已有规则（开闭原则），这也是面试时值得讲的一点。
    /// </summary>
    public interface ICheckRule
    {
        /// <summary>规则编号，如 R1。</summary>
        string Id { get; }

        /// <summary>规则名称，显示在报告与 UI 列表里。</summary>
        string Name { get; }

        /// <summary>对给定构件集合执行检查，返回问题列表。</summary>
        IEnumerable<Issue> Check(Document doc, IList<Element> elements);
    }
}
