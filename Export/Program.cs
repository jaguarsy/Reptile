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

namespace Export
{
    class Program
    {
        static void Main(string[] args)
        {
            testEntities dbContext = new testEntities();

            //1.城市快速路
            log("正在导出上海交通出行网城市快速路数据");

            HSSFWorkbook book = new HSSFWorkbook();

            var today = DateTime.Now.ToShortDateString();
            var result = dbContext.SHRoadIndex.Where(p => p.Date.Equals(today) && p.Type == 0).Distinct()
                .GroupBy(p => p.Name).ToList();

            foreach (var item in result)
            {
                ISheet sheet = book.CreateSheet(item.Key.Replace("*", ""));
                var list = item.ToList();
                for (var i = 0; i < list.Count; i++)
                {
                    getsheet(sheet, list);
                }
            }
            FileStream xfile = new FileStream(DateTime.Now.ToShortDateString() + "-城市快速路.xls", FileMode.Create, System.IO.FileAccess.Write);
            book.Write(xfile);
            xfile.Close();

            log("导出完毕");

            //2.地面道路
            log("正在导出上海交通出行网地面道路数据");
            book = new HSSFWorkbook();

            result = dbContext.SHRoadIndex.Where(p => p.Date.Equals(today) && p.Type == 1).Distinct()
                .GroupBy(p => p.Name).ToList();

            foreach (var item in result)
            {
                ISheet sheet = book.CreateSheet(item.Key.Replace("*", ""));
                var list = item.ToList();
                for (var i = 0; i < list.Count; i++)
                {
                    getsheet(sheet, list);
                }
            }
            xfile = new FileStream(DateTime.Now.ToShortDateString() + "-地面道路.xls", FileMode.Create, System.IO.FileAccess.Write);
            book.Write(xfile);
            xfile.Close();

            log("导出完毕");

            //3.停车位
            log("正在导出上海交通出行网停车位数据");
            book = new HSSFWorkbook();

            var packing = dbContext.Packing.Distinct().ToList();

            var packingsheet = book.CreateSheet("停车位");
            getsheet(packingsheet, packing);

            xfile = new FileStream(DateTime.Now.ToShortDateString() + "-停车位.xls", FileMode.Create, System.IO.FileAccess.Write);
            book.Write(xfile);
            xfile.Close();

            log("导出完毕");

            //4.道路实况
            log("正在导出上海交通出行网道路实况");
            book = new HSSFWorkbook();

            var news = dbContext.News.Distinct().ToList();
            ISheet newssheet = book.CreateSheet("道路实况");

            getsheet(newssheet, news);

            xfile = new FileStream(DateTime.Now.ToShortDateString() + "-道路实况.xls", FileMode.Create, System.IO.FileAccess.Write);
            book.Write(xfile);
            xfile.Close();

            log("导出完毕");
        }

        private static void log(string message)
        {
            Console.WriteLine(message);
        }

        private static void getsheet(ISheet sheet, List<SHRoadIndex> list)
        {
            // 第一行
            NPOI.SS.UserModel.IRow row = sheet.CreateRow(0);
            PropertyInfo[] props = typeof(SHRoadIndex).GetProperties(BindingFlags.Public | BindingFlags.Instance);

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
    }
}
