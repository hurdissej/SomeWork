using System.Linq;
using SpreadSheetReader.Persistence;

namespace SpreadSheetReader
{
    public class Program
    {
        static void Main(string[] args)
        {
            var directory = "/Users/hurdissej/Promotion";
            var rows = ExcelReader.ParsePromotionRows(ExcelReader.getExcelDump($"{directory}/BBQProvince.csv")).ToList();
            var promotions = StoreCountPromotionProvider.GetStoreCountPromotions(rows, directory);
            ExcelReader.WriteToFile(promotions, directory);
        }
        
    }
}
