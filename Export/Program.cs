using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
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
            HSSFWorkbook book = new HSSFWorkbook();
            ISheet sheet = book.CreateSheet("sheet1");

            testEntities dbContext = new testEntities();
            var result = dbContext.News.Distinct()
                .GroupBy(p=>p.RoadName).ToList();
            foreach (var item in result)
            {
                for (var i = 0; i < item.Count(); i++)
                {

                    
                }
            }
        }

        private static void getsheet(ISheet sheet, List<IGrouping<string,News>> result)
        {
            // 第一行
            NPOI.SS.UserModel.IRow row = sheet.CreateRow(0);
            PropertyInfo[] props = typeof(News).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < props.Count(); i++)
            {
                row.CreateCell(i).SetCellValue(props[i].Name);
            }

            for (int i = 0; i < result.Count; i++)
            {
                NPOI.SS.UserModel.IRow row2 = sheet.CreateRow(i + 1);
                for (int j = 0; j < props.Count(); j++)
                {
                    var value = props[j].GetValue(result[i], null);
                    if (value != null)
                    {
                        row2.CreateCell(j).SetCellValue(value.ToString());
                    }
                }
            }
        }
    }
}
