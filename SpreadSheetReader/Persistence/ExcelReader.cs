using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SpreadSheetReader.Persistence
{
    public static class ExcelReader
    {

        public static List<string[]> getExcelDump(string path)
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

        public static void WriteToFile(IEnumerable<Promotion> promotions)
        {
            string date = DateTime.Now.ToString("yy-MM-dd") + "-" + DateTime.Now.Hour + "_" + DateTime.Now.Minute;
            var filePath = $@"C:\Users\andy.bainton\Documents\PromotionOutput-{date}.csv";
            var result = new List<string[]>();
            result.Add(new[] { "ID", "StartDate", "EndDate", "SkuCode", "Promoted Price", "Store", "Days Ran in Store", "Volume By Store" });
            foreach (var promotion in promotions)
            {
                result.AddRange(promotion.DaysRaninEachStore.Select(store => new[] { promotion.Id.ToString("N"), promotion.StartDate.ToString("d"), promotion.EndDate.ToString("d"), promotion.Sku, store.Value.InitialPrice.ToString("N"), store.Key, store.Value.DaysRanInStore.ToString(), store.Value.TotalVolume.ToString() }));
            }
            StringBuilder sb = new StringBuilder();
            foreach (string[] t in result)
                sb.AppendLine(string.Join(",", t));

            File.WriteAllText(filePath, sb.ToString());
        }

        public static Dictionary<string, int> GetStoreCount(List<string[]> dump)
        {
            var result = new Dictionary<string, int>();
            foreach(var row in dump)
            {
                var store = row[1];
                if(result.TryGetValue(store, out int numberOfStores))
                {
                    result[store]++;
                } else 
                {
                    result.Add(store, 1);
                }
            }
            return result;
        }

        public static IEnumerable<ExcelRow> ParsePromotionRows(List<string[]> dump)
        {
            foreach (var row in dump)
            {
                var date = Convert.ToDateTime(row[0]);
                var actualPrice = Convert.ToDouble(row[3]);
                var basePrice = Convert.ToDouble(row[4]);
                var volume = Convert.ToDouble(row[5]);
                yield return new ExcelRow
                {
                    Date = date,
                    Store = row[1],
                    SKU = row[2],
                    ActualPrice = actualPrice,
                    BasePrice = basePrice,
                    Volume = volume
                };
            }
        }
    }
}
