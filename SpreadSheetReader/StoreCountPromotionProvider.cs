using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SpreadSheetReader.Persistence;

namespace SpreadSheetReader
{
    public class DistinctPriceRow
    {
        public double ActualPrice { get; set; }
        public double BasePrice { get; set; }
        public string SKU { get; set; }
    }
    public static class StoreCountPromotionProvider
    {
        public static IEnumerable<Promotion> GetStoreCountPromotions(List<ExcelRow> promotionRows)
        {
            // Result list
            var results = new List<Promotion>();
            var storeCount = ExcelReader.GetStoreCount(ExcelReader.getExcelDump(@"C:\Users\elliot.hurdiss\Documents\TestHierarchy.csv"));
            // Group promotionRows
            var groupedPromotion = promotionRows.GroupBy(x => new {x.Date, x.ActualPrice, x.BasePrice, x.SKU, x.Customer}).Select(
                y => new ExcelRow
                {
                    Customer = y.Key.Customer,
                    ActualPrice = y.Key.ActualPrice,
                    BasePrice = y.Key.BasePrice,
                    Date = y.Key.Date,
                    SKU = y.Key.SKU,
                    Store = y.Count().ToString(),
                    Volume = y.Sum(x => x.Volume)
                }).ToList();
            // Get List SKU / Prices
            var uniqueSkuPrices = promotionRows
                .Where(x => IsPromotedDay(x.BasePrice, x.ActualPrice))
                .GroupBy(x => new {x.ActualPrice, x.SKU})
                .Select(y => new KeyValuePair<string, double>(y.Key.SKU, y.Key.ActualPrice));
            // Group SKU prices if possible (do later)

            // Loop Through Prices
            foreach (var uniquePrice in uniqueSkuPrices)
            {
                // Get Days at that price
                var daysAtPrice = groupedPromotion
                    .Where(x => x.ActualPrice == uniquePrice.Value)
                    .Where(x => x.SKU == uniquePrice.Key)
                    .OrderBy(x => x.Date);
                // Set current promotion
                Promotion currentPromotion = null;
                //Promo Starts when store count goes over 60% - new promotions
                foreach (var day in daysAtPrice)
                {
                    var storeCountOnDay = Convert.ToInt32(day.Store);
                    var totalStoreCount = storeCount[day.Customer];
                    if (storeCountOnDay < totalStoreCount * 0.1)
                    {
                        if (currentPromotion != null)
                        {
                            results.Add(currentPromotion);
                            currentPromotion = null;
                        }
                        continue;
                    }
                    if (currentPromotion == null )
                    {
                        if(storeCountOnDay > totalStoreCount * 0.6)
                            currentPromotion = new Promotion(day.Date, day.Date, day.ActualPrice, day.SKU, day.Store, day.Volume);
                        continue;
                    }
                    if (currentPromotion.EndDate.AddDays(1) == day.Date)
                    {
                        currentPromotion.EndDate = day.Date;
                        currentPromotion.Volume += day.Volume;
                    }

                }
            }
            return results;
        }


        private static bool IsPromotedDay(double basePrice, double actualPrice) => actualPrice < basePrice * 0.95;

    }
}
