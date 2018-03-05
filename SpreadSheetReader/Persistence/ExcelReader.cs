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

        public static void WriteToFile(IEnumerable<Promotion> promotions, string directory)
        {
            string date = DateTime.Now.ToString("yy-MM-dd") + "-" + DateTime.Now.Hour + "_" + DateTime.Now.Minute;
            var filePath = $"{directory}/PromotionOutput-{date}.csv";
            var result = new List<string[]>();
            result.Add(new[] { "PromotionID", "Customer", "Province", "StartDate", "EndDate", "SkuCode", "Average Promoted Price", "Standard Deviation", "Number Of Stores Ran" , "PercentageOfStoresRan","Volume"});
            foreach (var promotion in promotions)
            {
                result.Add( new[]
                {
                    promotion.Id.ToString("N"),
                    promotion.Customer, 
                    String.Join(",", promotion.Province),
                    promotion.StartDate.ToString("d"),
                    promotion.EndDate.ToString("d"),
                    promotion.Sku,
                    promotion.PromotedPrice.ToString("N"),
                    promotion.StandardDeviation.ToString("N"),
                    promotion.NumberOfStores.ToString("N"),
                    promotion.PercentageOfStores.ToString("G"),
                    promotion.Volume.ToString()
                }); 
            }
            StringBuilder sb = new StringBuilder();
            foreach (string[] t in result)
                sb.AppendLine(string.Join(",", t));

            File.WriteAllText(filePath, sb.ToString());
        }

        public static IEnumerable<ExcelRow> ParsePromotionRows(List<string[]> dump)
        {
            foreach (var row in dump)
            {
                var date = Convert.ToDateTime(row[0]);
                var actualPrice = Convert.ToDouble(row[5]);
                var basePrice = Convert.ToDouble(row[6]);
                var volume = Convert.ToDouble(row[7]);
                yield return new ExcelRow
                {
                    Date = date,
                    Customer = row[1],
                    Province = row[2],
                    Store = row[3],
                    SKU = row[4],
                    ActualPrice = actualPrice,
                    BasePrice = basePrice,
                    Volume = volume,
                };
            }
        }
    }
}
