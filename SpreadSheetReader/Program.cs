using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpreadSheetReader
{
    public class PromotionDay
    {
        public DateTime Day { get; set; }
        public string Sku { get; set; }
        public string Store { get; set; }
    }

    public class Promotion
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Sku { get; set; }
        public List<string> Stores { get; set; }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var dump = getExcelDump(@"C:\Users\elliot.hurdiss\Documents\BaseData.csv");
            var skus = GetSkus(dump);
            var promoDates = getPromoDate(skus, dump);
            // to do make into List<PromotionDays>
            // compile into promotion
        }

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

        private static IEnumerable<string> GetSkus(List<string[]> excelDump)
        {
           return excelDump.Select(x => x[2]).Distinct();
        } 

        private static Dictionary<string, List<string>> getPromoDate(IEnumerable<string> skus, List<string[]> dump)
        {
            var promoDates = new Dictionary<string, List<string>>();
            foreach (var sku in skus)
            {
                foreach (string[] t in dump)
                {
                    if (t[2] != sku)
                        continue;

                    if (Double.Parse(t[3]) < Double.Parse(t[4]) * 0.9)
                    {
                        if (promoDates.TryGetValue(t[2], out List<string> skuPrice))
                        {
                            promoDates[t[2]].Add($"Date {t[0]} is promoted for sku {t[2]} in store {t[1]}");
                        }
                        else
                        {
                            promoDates.Add(t[2], new List<string> { $"Date {t[0]} is promoted for sku {t[2]} in store {t[1]}" });
                        }
                    }
                }
            }
            return promoDates;
        }
    }
}
