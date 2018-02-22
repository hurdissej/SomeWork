using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpreadSheetReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var dump = getExcelDump(@"C:\Users\elliot.hurdiss\Documents\FullData.csv");
            var skus = GetSkus(dump);
            var prices = GetMaxPrice(dump, skus);
            var promoDates = new List<string>();
            foreach (var sku in skus)
            {
                var promoPrice = prices[sku] * 0.9;
                foreach (string[] t in dump)
                {
                    if(t[2] != sku)
                        continue;

                    if (Double.Parse(t[3]) < promoPrice)
                    {
                        promoDates.Add($"Date {t[0]} is promoted for sku {t[2]} in store {t[1]}");
                    }
                }
            }

            promoDates.ForEach(Console.WriteLine);
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

        private static Dictionary<string, double> GetMaxPrice(List<string[]> excelDump, IEnumerable<string> skus)
        {
            var result = new Dictionary<string, double>();
            foreach (var eposRow in excelDump)
            {
                if (result.TryGetValue(eposRow[2], out double skuPrice))
                {
                    result[eposRow[2]] = skuPrice > Double.Parse(eposRow[3]) ? skuPrice : Double.Parse(eposRow[3]);
                } else {
                    result.Add(eposRow[2], Double.Parse(eposRow[3]));   
                }
            }
            return result;
        }
    }
}
