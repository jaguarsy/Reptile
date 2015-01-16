using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.ExcelEntities
{
    public class SHRoadEntity
    {
        public string State { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> CurrentIndex { get; set; }
        public Nullable<decimal> ReferenceIndex { get; set; }
        public Nullable<decimal> DValue { get; set; }
        public string Time { get; set; }
        public Nullable<int> Type { get; set; }
        public string Date { get; set; }
    }
}
