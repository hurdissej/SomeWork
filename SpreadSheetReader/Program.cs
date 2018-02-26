using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SpreadSheetReader.Persistence;

namespace SpreadSheetReader
{
    public class Program
    {
        static void Main(string[] args)
        {
            var dump = ExcelReader.getExcelDump(@"C:\Users\elliot.hurdiss\Documents\VolumeTest.csv");
            var rows = ExcelReader.ParseRows(dump).ToList();
            var ordered =  rows.OrderBy(x => x.SKU).ThenBy(x => x.Date).ThenBy(x => x.Store);
            var promotions = GetPromotions(ordered);
            ExcelReader.WriteToFile(promotions);
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
                    currentPromotion = new Promotion(row.Date, row.Date, row.ActualPrice, row.SKU, row.Store, row.Volume);
                    continue;
                }
                // Same Date, Same SKU - Then add the store in 
                if (currentPromotion.EndDate == row.Date && row.SKU == currentPromotion.Sku)
                {
                    currentPromotion.IncrementStoreCountAndVolume(row.Store, row.Volume);
                    continue;
                }
                // RowDate is day after Previous Promotion End Date && Same SKU -
                if (row.Date == currentPromotion.EndDate.AddDays(1) && row.SKU == currentPromotion.Sku)
                {
                    currentPromotion.EndDate = row.Date;
                    currentPromotion.IncrementStoreCountAndVolume(row.Store, row.Volume);
                }
            }
            if (currentPromotion != null)
                result.Add(currentPromotion);
            
            return result;
        }

        private static bool IsPromotedDay(double basePrice, double actualPrice) => actualPrice < (basePrice * 0.95);
    }
}
