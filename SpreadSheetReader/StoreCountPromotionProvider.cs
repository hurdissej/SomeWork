using System;
using System.Collections.Generic;
using System.Linq;

namespace SpreadSheetReader
{
    public static class StoreCountPromotionProvider
    {
        public static IEnumerable<Promotion> GetStoreCountPromotions(List<ExcelRow> promotionRows, string directory)
        {
            var results = new List<Promotion>();
            var storeCount = promotionRows.Select(x => new {x.Customer, x.Store})
                .Distinct()
                .GroupBy(x => x.Customer)
                .ToDictionary(y=> y.Key, y => y.Count());
            
            var groupedPromotion = promotionRows
                .Where(x => IsPromotedDay(x.BasePrice, x.ActualPrice))
                .GroupBy(x => new { x.Date, x.ActualPrice, x.SKU, x.Customer, x.Province })
                .Select(
                y => new ExcelRow
                {
                    Province =  y.Key.Province,
                    Customer = y.Key.Customer,
                    ActualPrice = y.Key.ActualPrice,
                    BasePrice = y.Average(x => x.BasePrice),
                    Date = y.Key.Date,
                    SKU = y.Key.SKU,
                    NumberOfStores = y.Count(),
                    Volume = y.Sum(x => x.Volume)
                }).ToList();
            
            var uniqueSkuPrices = promotionRows
                .Where(x => IsPromotedDay(x.BasePrice, x.ActualPrice))
                .GroupBy(x => new { x.ActualPrice, x.SKU })
                .Select(y => new KeyValuePair<string, double>(y.Key.SKU, y.Key.ActualPrice));

            foreach (var uniquePrice in uniqueSkuPrices)
            {
                var daysAtPrice = groupedPromotion
                    .Where(x => x.ActualPrice == uniquePrice.Value)
                    .Where(x => x.SKU == uniquePrice.Key)
                    .OrderBy(x => x.Date);
                Promotion currentPromotion = null;
                foreach (var day in daysAtPrice)
                {
                    if (currentPromotion == null)
                    {
                        currentPromotion = new Promotion(day.Date, day.Date, day.ActualPrice, day.SKU, day.NumberOfStores, day.Volume, day.Customer, storeCount[day.Customer], new List<string>{day.Province});
                        continue;
                    }
                    if (day.Date >= currentPromotion.EndDate && day.Date <= currentPromotion.EndDate.AddDays(5))
                    {
                        currentPromotion.EndDate = day.Date;
                        currentPromotion.Volume += day.Volume;
                        continue;
                    }
                    results.Add(currentPromotion);
                    currentPromotion = new Promotion(day.Date, day.Date, day.ActualPrice, day.SKU, day.NumberOfStores, day.Volume, day.Customer, storeCount[day.Customer], new List<string> { day.Province });
                }
                results.Add(currentPromotion);
            }

            var grouped = results
                .GroupBy(x => new {x.Customer, x.Sku, x.StartDate, x.EndDate})
                .Select(y => new Promotion(y.Key.StartDate, y.Key.EndDate, y.Average(x => x.PromotedPrice), y.Key.Sku,
                    y.Sum(x => x.NumberOfStores), y.Sum(x => x.Volume), y.Key.Customer, storeCount[y.Key.Customer], y.SelectMany(s => s.Province).Distinct().ToList())
                {
                    NumberOfStoresInCustomerGroup = storeCount[y.Key.Customer],
                    StandardDeviation =Math.Sqrt(y.Sum(d => Math.Pow(d.PromotedPrice - y.Average(x => x.PromotedPrice), 2)) / y.Count())
                })
                .OrderBy(x => x.Customer)
                .ThenBy(x => x.Sku)
                .ThenBy(x => x.StartDate);


            var joinedPromotions = new List<Promotion>();

            Promotion currentPromo = null;

            foreach (var promotion in grouped)
            {
                if (currentPromo == null)
                {
                    currentPromo = promotion;
                    continue;
                }
                if (IsWithinAcceptableRange(currentPromo, promotion))
                {
                    currentPromo.EndDate = promotion.EndDate;
                    currentPromo.Volume += promotion.Volume;
                    var newProvinces = promotion.Province.Where(p => promotion.Province.All(p2 => p2 != p)); 
                    if(newProvinces.Any())
                        currentPromo.Province.AddRange(newProvinces);
                    continue;
                }
                joinedPromotions.Add(currentPromo);
                currentPromo = promotion;
            }
            return joinedPromotions;
        }

        private static bool IsWithinAcceptableRange(Promotion currentPromo, Promotion promotion)
        {
            if (currentPromo.EndDate.AddDays(1) == promotion.StartDate && currentPromo.Customer == promotion.Customer && currentPromo.Sku == promotion.Sku)
                return promotion.PromotedPrice < currentPromo.PromotedPrice * 1.10 && promotion.PromotedPrice > currentPromo.PromotedPrice * 0.9;
            return false;
        }
        private static bool IsPromotedDay(double basePrice, double actualPrice) => actualPrice < basePrice * 0.95;
    }
}
