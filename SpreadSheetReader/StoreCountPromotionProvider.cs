using System;
using System.Collections.Generic;
using System.Linq;

namespace SpreadSheetReader
{
    public static class StoreCountPromotionProvider
    {
        public static IEnumerable<Promotion> GetStoreCountPromotions(List<ExcelRow> promotionRows, string directory)
        {
            var uniqueSkuPrices = promotionRows
                .Where(x => IsPromotedDay(x.BasePrice, x.ActualPrice))
                .GroupBy(x => new { x.ActualPrice, x.SKU })
                .Select(y => new KeyValuePair<string, double>(y.Key.SKU, y.Key.ActualPrice));

            var storeCount = GetStoreCount(promotionRows);
            var uniquePromotions = GetUniquePromotions(promotionRows, uniqueSkuPrices, storeCount);
            var grouped = uniquePromotions
                .GroupBy(x => new {x.Customer, x.Sku, x.StartDate, x.EndDate})
                .Select(y => new Promotion(y.Key.StartDate, y.Key.EndDate, y.Average(x => x.PromotedPrice), y.Key.Sku, y.Sum(x => x.MaximumNumberOfStores), y.Sum(x => x.Volume), y.Key.Customer, storeCount[y.Key.Customer], y.SelectMany(s => s.Province).Distinct().ToList())
                {
                    MinimumNumberOfStores = y.Min(x => x.MinimumNumberOfStores),
                    NumberOfStoresInCustomerGroup = storeCount[y.Key.Customer],
                    StandardDeviation =Math.Sqrt(y.Sum(d => Math.Pow(d.PromotedPrice - y.Average(x => x.PromotedPrice), 2)) / y.Count())
                })
                .OrderBy(x => x.Customer)
                .ThenBy(x => x.Sku)
                .ThenBy(x => x.StartDate);
          
            return StitchPromotionsTogether(grouped);
        }

        private static  List<Promotion> GetUniquePromotions(IEnumerable<ExcelRow> excelRows, IEnumerable<KeyValuePair<string, double>> uniquePrices, Dictionary<string, int> storeCount)
        {

            var groupedRows = GetGroupedRows(excelRows);
            var results = new List<Promotion>();
            foreach (var uniquePrice in uniquePrices)
            {
                var daysAtPrice = groupedRows
                    .Where(x => x.ActualPrice == uniquePrice.Value)
                    .Where(x => x.SKU == uniquePrice.Key)
                    .OrderBy(x => x.Customer)
                    .ThenBy(x => x.Date);
                Promotion currentPromotion = null;
                foreach (var day in daysAtPrice)
                {
                    if (currentPromotion == null)
                    {
                        currentPromotion = new Promotion(day.Date, day.Date, day.ActualPrice, day.SKU, day.NumberOfStores, day.Volume, day.Customer, storeCount[day.Customer], new List<string> { day.Province });
                        continue;
                    }
                    if (day.Date >= currentPromotion.EndDate && day.Date <= currentPromotion.EndDate.AddDays(5) && currentPromotion.Customer == day.Customer)
                    {
                        currentPromotion.MinimumNumberOfStores = day.NumberOfStores < currentPromotion.MinimumNumberOfStores ? day.NumberOfStores : currentPromotion.MinimumNumberOfStores;
                        currentPromotion.MaximumNumberOfStores = day.NumberOfStores > currentPromotion.MaximumNumberOfStores ? day.NumberOfStores : currentPromotion.MaximumNumberOfStores;
                        currentPromotion.EndDate = day.Date;
                        currentPromotion.Volume += day.Volume;
                        continue;
                    }
                    results.Add(currentPromotion);
                    currentPromotion = new Promotion(day.Date, day.Date, day.ActualPrice, day.SKU, day.NumberOfStores, day.Volume, day.Customer, storeCount[day.Customer], new List<string> { day.Province });
                }
                results.Add(currentPromotion);
            }
            return results;
        }

        private static IEnumerable<ExcelRow> GetGroupedRows(IEnumerable<ExcelRow> excelRows)
        {
            return excelRows
                .Where(x => IsPromotedDay(x.BasePrice, x.ActualPrice))
                .GroupBy(x => new { x.Date, x.ActualPrice, x.SKU, x.Customer, x.Province })
                .Select(
                    y => new ExcelRow
                    {
                        Province = y.Key.Province,
                        Customer = y.Key.Customer,
                        ActualPrice = y.Key.ActualPrice,
                        BasePrice = y.Average(x => x.BasePrice),
                        Date = y.Key.Date,
                        SKU = y.Key.SKU,
                        NumberOfStores = y.Count(),
                        Volume = y.Sum(x => x.Volume)
                    }).ToList();
        }

        private static Dictionary<string, int> GetStoreCount(IEnumerable<ExcelRow> excelRows)
        {
            return excelRows.Select(x => new { x.Customer, x.Store })
                .Distinct()
                .GroupBy(x => x.Customer)
                .ToDictionary(y => y.Key, y => y.Count());
        }

        private static List<Promotion> StitchPromotionsTogether(IEnumerable<Promotion> grouped)
        {
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
                    currentPromo.MinimumNumberOfStores = promotion.MinimumNumberOfStores < currentPromo.MinimumNumberOfStores ? promotion.MinimumNumberOfStores : currentPromo.MinimumNumberOfStores;
                    currentPromo.MaximumNumberOfStores = promotion.MaximumNumberOfStores > currentPromo.MaximumNumberOfStores ? promotion.MaximumNumberOfStores : currentPromo.MaximumNumberOfStores;
                    currentPromo.EndDate = promotion.EndDate;
                    currentPromo.Volume += promotion.Volume;
                    var newProvinces = promotion.Province.Where(p => promotion.Province.All(p2 => p2 != p));
                    if (newProvinces.Any())
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
                return promotion.PromotedPrice < currentPromo.PromotedPrice * 1.05 && promotion.PromotedPrice > currentPromo.PromotedPrice * 0.95;
            return false;
        }
        private static bool IsPromotedDay(double basePrice, double actualPrice) => actualPrice < basePrice * 0.95;
    }
}
