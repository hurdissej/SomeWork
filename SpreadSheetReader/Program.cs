using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SpreadSheetReader
{
   public class Promotion
    {
        public Promotion(DateTime startdate, DateTime endDate, double price, string sku, string store)
        {
            StartDate = startdate;
            EndDate = endDate;
            Sku = sku;
            InitialPrice = price;
            DaysRaninEachStore = new Dictionary<string, int>() {{store,1}};
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Sku { get; set; }
        public double InitialPrice { get; set; }
        public Dictionary<string, int> DaysRaninEachStore { get; set; }

        public void PromotionHappened(string store)
        {
            IncrementStoreDate(store);
        }
        private void IncrementStoreDate(string store)
        {
            if (DaysRaninEachStore.TryGetValue(store, out var storeCount))
            {
                DaysRaninEachStore[store] = storeCount + 1;
            }
            else
            {
                DaysRaninEachStore.Add(store, 1);
            }

        }
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
            WriteToFile(promotions);
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
                    currentPromotion = new Promotion(row.Date, row.Date, row.ActualPrice, row.SKU, row.Store);
                    continue;
                }
                // Same Date, Same SKU - Then add the store in 
                if (currentPromotion.EndDate == row.Date && row.SKU == currentPromotion.Sku)
                {
                    currentPromotion.PromotionHappened(row.Store);
                    continue;
                }
                // RowDate is day after Previous Promotion End Date && Same SKU -
                if (row.Date == currentPromotion.EndDate.AddDays(1) && row.SKU == currentPromotion.Sku)
                {
                    currentPromotion.EndDate = row.Date;
                    currentPromotion.PromotionHappened(row.Store);
                }
            }
            if (currentPromotion != null)
                result.Add(currentPromotion);
            
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

        private static void WriteToFile(IEnumerable<Promotion> promotions)
        {
            string date = DateTime.Now.ToString("yy-MM-dd") + "-" + DateTime.Now.Hour + "_" + DateTime.Now.Minute; 
            var filePath = $@"C:\Users\elliot.hurdiss\Documents\PromotionOutput-{date}.csv";
            var result = new List<string[]>();
            result.Add(new []{"StartDate", "EndDate", "SkuCode", "Promoted Price", "Store", "Days Ran in Store"});
            foreach (var promotion in promotions)
            {
                result.AddRange(promotion.DaysRaninEachStore.Select(store => new[] {promotion.StartDate.ToString("d"), promotion.EndDate.ToString("d"), promotion.Sku, promotion.InitialPrice.ToString("N"), store.Key, store.Value.ToString()}));
            }
            StringBuilder sb = new StringBuilder();
            foreach (string[] t in result)
                sb.AppendLine(string.Join(",", t));

            File.WriteAllText(filePath, sb.ToString());
        }
        
        private static IEnumerable<ExcelRow> ParseRows(List<string[]> dump)
        {
            foreach (var row in dump)
            {
                var date = Convert.ToDateTime(row[0]);
                var actualPrice = Convert.ToDouble(row[3]);
                var basePrice = Convert.ToDouble(row[4]);
                yield return new ExcelRow
                {
                    Date = date,
                    Store = row[1],
                    SKU = row[2],
                    ActualPrice = actualPrice,
                    BasePrice = basePrice
                };
            }
        }
    }
}
