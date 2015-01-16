using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Models.ExcelEntities;

namespace Export
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] amapName = { "全部道路", "快速高速道路", "普通道路" };
            string[] shNews = { "城市快速路", "干线公路", "地面道路" };
            testEntities dbContext = new testEntities();
            FileStream xfile;

            //1.城市快速路
            log("正在导出上海交通出行网城市快速路数据");

            HSSFWorkbook book = new HSSFWorkbook();

            var today = DateTime.Now.AddDays(-1).ToShortDateString();
            var result = dbContext.SHRoadIndex
                .Where(p => p.Date.Equals(today) && p.Type == 0)
                .Select(p => new SHRoadEntity()
                {
                    Name = p.Name,
                    CurrentIndex = p.CurrentIndex,
                    Date = p.Date,
                    DValue = p.DValue,
                    ReferenceIndex = p.ReferenceIndex,
                    State = p.State,
                    Time = p.Time
                })
                .Distinct()
                .OrderBy(p => p.Time)
                .GroupBy(p => p.Name);

            foreach (var item in result)
            {
                ISheet sheet = book.CreateSheet(item.Key.Replace("*", ""));
                var list = item.ToList();
                for (var i = 0; i < list.Count; i++)
                {
                    getsheet(sheet, list);
                }
            }
            xfile = new FileStream(DateTime.Now.ToString("yyyyMMdd") + "-上海交通出行网-城市快速路.xls", FileMode.Create, System.IO.FileAccess.Write);
            book.Write(xfile);
            xfile.Close();

            log("导出完毕");

            //2.地面道路
            log("正在导出上海交通出行网地面道路数据");
            book = new HSSFWorkbook();

            result = dbContext.SHRoadIndex
                .Where(p => p.Date.Equals(today) && p.Type == 1)
                .Select(p => new SHRoadEntity()
                {
                    Name = p.Name,
                    CurrentIndex = p.CurrentIndex,
                    Date = p.Date,
                    DValue = p.DValue,
                    ReferenceIndex = p.ReferenceIndex,
                    State = p.State,
                    Time = p.Time
                })
                .Distinct()
                .OrderBy(p => p.Time)
                .GroupBy(p => p.Name);

            foreach (var item in result)
            {
                ISheet sheet = book.CreateSheet(item.Key.Replace("*", ""));
                var list = item.OrderBy(p => p.Time).ToList();
                for (var i = 0; i < list.Count; i++)
                {
                    getsheet(sheet, list);
                }
            }
            xfile = new FileStream(DateTime.Now.ToString("yyyyMMdd") + "-上海交通出行网-地面道路.xls", FileMode.Create, System.IO.FileAccess.Write);
            book.Write(xfile);
            xfile.Close();

            log("导出完毕");

            //3.停车位
            log("正在导出上海交通出行网停车位数据");
            book = new HSSFWorkbook();

            var packing = dbContext.Packing
                .Where(p => p.date.Equals(today))
                .OrderBy(p => p.Time)
                .GroupBy(p => p.Name);

            foreach (var item in packing)
            {
                var packingsheet = book.CreateSheet(item.Key);
                getsheet(packingsheet, item.ToList());
            }

            xfile = new FileStream(DateTime.Now.ToString("yyyyMMdd") + "-上海交通出行网-停车位.xls", FileMode.Create, System.IO.FileAccess.Write);
            book.Write(xfile);
            xfile.Close();

            log("导出完毕");

            //4.道路实况
            log("正在导出上海交通出行网道路实况");
            book = new HSSFWorkbook();

            var news = dbContext.News
                .Where(p => p.date.Equals(today))
                .OrderBy(p => p.Time)
                .GroupBy(p => p.RoadType);

            foreach (var item in news)
            {
                var newssheet = book.CreateSheet(shNews[item.Key.Value - 1]);

                getsheet(newssheet, item.ToList());
            }

            xfile = new FileStream(DateTime.Now.ToString("yyyyMMdd") + "-上海交通出行网-道路实况.xls", FileMode.Create, System.IO.FileAccess.Write);
            book.Write(xfile);
            xfile.Close();

            log("导出完毕");

            //4.高德地图
            log("正在导出高德地图数据");
            for (var i = 1; i <= 3; i++)
            {
                book = new HSSFWorkbook();

                var amap = dbContext.AMap
                    .Where(p => p.Date.Equals(today) && p.Type == i)
                    .GroupBy(p => p.Name);
                foreach (var item in amap)
                {
                    ISheet ampsheet = book.CreateSheet(item.Key);

                    getsheet(ampsheet, item.ToList());
                }

                xfile = new FileStream(DateTime.Now.ToString("yyyyMMdd") + "-高德-" + amapName[i - 1] + ".xls", FileMode.Create, System.IO.FileAccess.Write);
                book.Write(xfile);
                xfile.Close();
            }

            log("导出完毕");
        }

        private static void log(string message)
        {
            Console.WriteLine(message);
        }

        private static void getsheet(ISheet sheet, List<SHRoadEntity> list)
        {
            // 第一行
            NPOI.SS.UserModel.IRow row = sheet.CreateRow(0);
            PropertyInfo[] props = typeof(SHRoadEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < props.Count(); i++)
            {
                row.CreateCell(i).SetCellValue(props[i].Name);
            }

            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow row2 = sheet.CreateRow(i + 1);
                for (int j = 0; j < props.Count(); j++)
                {
                    var value = props[j].GetValue(list[i], null);
                    if (value != null)
                    {
                        row2.CreateCell(j).SetCellValue(value.ToString());
                    }
                }
            }
        }

        private static void getsheet(ISheet sheet, List<Packing> list)
        {
            // 第一行
            NPOI.SS.UserModel.IRow row = sheet.CreateRow(0);
            PropertyInfo[] props = typeof(Packing).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < props.Count(); i++)
            {
                row.CreateCell(i).SetCellValue(props[i].Name);
            }

            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow row2 = sheet.CreateRow(i + 1);
                for (int j = 0; j < props.Count(); j++)
                {
                    var value = props[j].GetValue(list[i], null);
                    if (value != null)
                    {
                        row2.CreateCell(j).SetCellValue(value.ToString());
                    }
                }
            }
        }

        private static void getsheet(ISheet sheet, List<News> list)
        {
            // 第一行
            NPOI.SS.UserModel.IRow row = sheet.CreateRow(0);
            PropertyInfo[] props = typeof(News).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < props.Count(); i++)
            {
                row.CreateCell(i).SetCellValue(props[i].Name);
            }

            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow row2 = sheet.CreateRow(i + 1);
                for (int j = 0; j < props.Count(); j++)
                {
                    var value = props[j].GetValue(list[i], null);
                    if (value != null)
                    {
                        row2.CreateCell(j).SetCellValue(value.ToString());
                    }
                }
            }
        }

        private static void getsheet(ISheet sheet, List<AMap> list)
        {
            // 第一行
            NPOI.SS.UserModel.IRow row = sheet.CreateRow(0);
            PropertyInfo[] props = typeof(AMap).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < props.Count(); i++)
            {
                row.CreateCell(i).SetCellValue(props[i].Name);
            }

            for (int i = 0; i < list.Count; i++)
            {
                NPOI.SS.UserModel.IRow row2 = sheet.CreateRow(i + 1);
                for (int j = 0; j < props.Count(); j++)
                {
                    var value = props[j].GetValue(list[i], null);
                    if (value != null)
                    {
                        row2.CreateCell(j).SetCellValue(value.ToString());
                    }
                }
            }
        }
    }
}
