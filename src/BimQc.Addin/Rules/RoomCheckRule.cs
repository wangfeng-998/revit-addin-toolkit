using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using BimQc.Models;
using BimQc.RevitWrap;

namespace BimQc.Rules
{
    /// <summary>R4：房间未放置（面积=0）或未命名。房间单独采集，不依赖传入的构件集合。</summary>
    public class RoomCheckRule : ICheckRule
    {
        public string Id { get { return "R4"; } }

        public string Name { get { return "房间未放置/未命名"; } }

        public IEnumerable<Issue> Check(Document doc, IList<Element> elements)
        {
            var result = new List<Issue>();
            var col = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();

            foreach (var el in col)
            {
                var room = el as Room;
                if (room == null) continue;

                var number = string.Empty;
                var name = string.Empty;
                var area = 0.0;
                try
                {
                    number = room.Number ?? string.Empty;
                    name = room.get_Parameter(BuiltInParameter.ROOM_NAME) == null
                        ? string.Empty
                        : ElementInfo.Text(doc, room.get_Parameter(BuiltInParameter.ROOM_NAME));
                    area = room.Area;
                }
                catch (System.Exception)
                {
                    continue;
                }

                if (area <= 0.0001)
                {
                    result.Add(new Issue
                    {
                        Category = "房间",
                        ElementId = el.Id.IntegerValue,
                        Family = string.Empty,
                        TypeName = number,
                        RuleId = Id,
                        IssueType = "房间未放置",
                        Detail = string.Format("编号 {0}，面积为 0，未放置到封闭区域", number)
                    });
                }
                else if (name.Trim().Length == 0)
                {
                    result.Add(new Issue
                    {
                        Category = "房间",
                        ElementId = el.Id.IntegerValue,
                        Family = string.Empty,
                        TypeName = number,
                        RuleId = Id,
                        IssueType = "房间未命名",
                        Detail = string.Format("编号 {0}，面积正常但名称为空", number)
                    });
                }
            }
            return result;
        }
    }
}
