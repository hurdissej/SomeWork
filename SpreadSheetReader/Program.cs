using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpreadSheetReader
{
   public class Promotion
    {
        public Promotion(DateTime startdate, DateTime endDate, string sku, string store)
        {
            StartDate = startdate;
            EndDate = endDate;
            Sku = sku;
            Stores = new List<string>{store};
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Sku { get; set; }
        public List<string> Stores { get; set; }
    }

    public class ExcelRow
    {
        public DateTime Date { get; set; }
        public string Store { get; set; }
        public string SKU { get; set; }
        public double ActualPrice { get; set; }
        public double BasePrice { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var dump = getExcelDump(@"C:\Users\elliot.hurdiss\Documents\BaseData.csv");
            var rows = ParseRows(dump).ToList();
            var ordered =  rows.OrderBy(x => x.SKU).ThenBy(x => x.Date).ThenBy(x => x.Store);
            var promotions = GetPromotions(ordered);
        }

        private static IEnumerable<Promotion> GetPromotions(IEnumerable<ExcelRow> OrderedRows)
        {
            var result = new List<Promotion>();
            Promotion currentPromotion = null;
            foreach (var row in OrderedRows)
            {
                //Check if promoted Day
                if (!IsPromotedDay(row.BasePrice, row.ActualPrice))
                {
                    if (currentPromotion != null)
                    {
                        result.Add(currentPromotion);
                        currentPromotion = null;
                    }
                    continue;
                }

                //If it is a new promotion
                if (currentPromotion == null)
                {
                    currentPromotion = new Promotion(row.Date, row.Date, row.SKU, row.Store);
                    continue;
                }

                // Same Date, Same SKU - Then add the store in 
                if (currentPromotion.EndDate == row.Date && row.SKU == currentPromotion.Sku)
                {
                    if(!currentPromotion.Stores.Contains(row.Store))
                        currentPromotion.Stores.Add(row.Store);
                    continue;
                }

                // RowDate is day after Previous Promotion End Date && Same SKU -
                if (row.Date == currentPromotion.EndDate.AddDays(1) && row.SKU == currentPromotion.Sku)
                {
                    currentPromotion.EndDate = row.Date;
                    if (!currentPromotion.Stores.Contains(row.Store))
                        currentPromotion.Stores.Add(row.Store);
                }
            }
            if (currentPromotion != null)
            {
                result.Add(currentPromotion);
            }
            return result;
        }

        private static bool IsPromotedDay(double basePrice, double actualPrice) => actualPrice < (basePrice * 0.95);


        private static List<string[]> getExcelDump(string path)
        {
            using (var reader = new StreamReader(path))
            {
                List<string[]> excelDump = new List<string[]>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    excelDump.Add(values);
                }
                return excelDump;
            }
        }
        
        private static IEnumerable<ExcelRow> ParseRows(List<string[]> dump)
        {
            foreach (var row in dump)
            {
                var Date = Convert.ToDateTime(row[0]);
                var ActualPrice = Convert.ToDouble(row[3]);
                var BasePrice = Convert.ToDouble(row[4]);
                yield return new ExcelRow
                {
                    Date = Date,
                    Store = row[1],
                    SKU = row[2],
                    ActualPrice = ActualPrice,
                    BasePrice = BasePrice
                };
            }
        }
    }
}
