using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            var sw = new Stopwatch();
            var rows = ExcelReader.ParsePromotionRows(ExcelReader.getExcelDump(@"C:\Users\elliot.hurdiss\Documents\CustomerData.csv")).ToList();
            sw.Start();
            var storeLevelPromotions = GetStoreLevelPromotions(rows);
            var timeForOne = sw.ElapsedMilliseconds;
            sw.Restart();
            var groupedPromotions = StoreCountPromotionProvider.GetStoreCountPromotions(rows);
            var timeForTwo = sw.ElapsedMilliseconds;
            sw.Stop();
            ExcelReader.WriteToFile(groupedPromotions);
        }

        private static IEnumerable<Promotion> GetStoreLevelPromotions(List<ExcelRow> rows)
        {
            var orderedRows = rows.OrderBy(x => x.Store).ThenBy(x => x.SKU).ThenBy(x => x.Date);
            var result = new List<Promotion>();
            Promotion currentPromotion = null;
            foreach (var row in orderedRows)
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
                   // currentPromotion.IncrementStoreCountAndVolume(row.Store, row.Volume, row.ActualPrice);
                    continue;
                }
                // RowDate is day after Previous Promotion End Date && Same SKU -
                if (row.Date == currentPromotion.EndDate.AddDays(1) && row.SKU == currentPromotion.Sku && IsWithinAcceptableRange(currentPromotion.PromotedPrice, row.ActualPrice))
                {
                    currentPromotion.EndDate = row.Date;
                   // currentPromotion.IncrementStoreCountAndVolume(row.Store, row.Volume, row.ActualPrice);
                    continue;
                }
                // This is now a back to back promo so we need to add our last promo to the results list and start a new one
                result.Add(currentPromotion);
                currentPromotion = new Promotion(row.Date, row.Date, row.ActualPrice, row.SKU, row.Store, row.Volume);

            }
            if (currentPromotion != null)
                result.Add(currentPromotion);
            
            return result;
        }

        private static bool IsWithinAcceptableRange(double currentPromotionPrice, double todaysPrice)
        {
            var lowestPrice = currentPromotionPrice * 0.95;
            var highestPrice = currentPromotionPrice * 1.05;
            return todaysPrice > lowestPrice && todaysPrice < highestPrice;
        } 

        private static bool IsPromotedDay(double basePrice, double actualPrice) => actualPrice < basePrice * 0.95;
    }
}
