using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace BimQc.RevitWrap
{
    /// <summary>
    /// Revit API 的薄封装：把"取参数值 / 取标高 / 取族类型名"这些重复代码收在一处。
    /// UI 与规则层不直接碰 Element 的弯弯绕绕，换 Revit 版本时只改这一层。
    /// </summary>
    public static class ElementInfo
    {
        public static string Text(Document doc, Parameter p)
        {
            if (p == null) return string.Empty;
            try
            {
                switch (p.StorageType)
                {
                    case StorageType.String:
                        return p.AsString() ?? string.Empty;
                    case StorageType.Integer:
                        return p.AsValueString() ?? p.AsInteger().ToString();
                    case StorageType.Double:
                        return p.AsValueString() ?? p.AsDouble().ToString("0.###");
                    case StorageType.ElementId:
                        {
                            var id = p.AsElementId();
                            if (id == null || id == ElementId.InvalidElementId) return string.Empty;
                            var e = doc.GetElement(id);
                            return e == null ? string.Empty : e.Name;
                        }
                    default:
                        return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>先取实例参数，取不到回落到类型参数。</summary>
        public static Parameter GetParameter(Element el, BuiltInParameter bip)
        {
            if (el == null) return null;
            try
            {
                var p = el.get_Parameter(bip);
                if (p != null) return p;
                var tid = el.GetTypeId();
                if (tid != null && tid != ElementId.InvalidElementId)
                {
                    var t = el.Document.GetElement(tid);
                    if (t != null) return t.get_Parameter(bip);
                }
            }
            catch (Exception)
            {
                // 少数构件不支持该参数，按"没有"处理
            }
            return null;
        }

        public static string Value(Element el, BuiltInParameter bip)
        {
            return Text(el.Document, GetParameter(el, bip));
        }

        public static string LevelName(Element el)
        {
            try
            {
                var lid = el.LevelId;
                if (lid != null && lid != ElementId.InvalidElementId)
                {
                    var lv = el.Document.GetElement(lid);
                    if (lv != null) return lv.Name;
                }
            }
            catch (Exception)
            {
                // 部分构件没有 LevelId
            }

            var candidates = new[]
            {
                BuiltInParameter.FAMILY_LEVEL_PARAM,
                BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM,
                BuiltInParameter.SCHEDULE_LEVEL_PARAM
            };
            foreach (var bip in candidates)
            {
                var text = Value(el, bip);
                if (!string.IsNullOrEmpty(text)) return text;
            }
            return string.Empty;
        }

        public static string FamilyName(Element el)
        {
            return Value(el, BuiltInParameter.ELEM_FAMILY_PARAM);
        }

        public static string TypeName(Element el)
        {
            try
            {
                var t = el.Document.GetElement(el.GetTypeId());
                return t == null ? string.Empty : t.Name;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string CategoryName(Element el)
        {
            try
            {
                return el.Category == null ? string.Empty : el.Category.Name;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
