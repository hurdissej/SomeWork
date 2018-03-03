using System;
using System.Collections.Generic;
using System.Text;

namespace SpreadSheetReader
{
    public class ExcelRow
    {
        public DateTime Date { get; set; }
        public string Customer { get; set; }
        public string Province { get; set; }
        public string Store { get; set; }
        public string SKU { get; set; }
        public double ActualPrice { get; set; }
        public double BasePrice { get; set; }
        public double Volume { get; set; }
        public int NumberOfStores { get; set; }
    }
}
