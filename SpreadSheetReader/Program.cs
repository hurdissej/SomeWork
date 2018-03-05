using System.Linq;
using SpreadSheetReader.Persistence;

namespace SpreadSheetReader
{
    public class Program
    {
        static void Main(string[] args)
        {
            var directory = @"C:\Users\elliot.hurdiss\Documents";
            var rows = ExcelReader.ParsePromotionRows(ExcelReader.getExcelDump($@"{directory}\FullDataTest.csv")).ToList();
            var promotions = StoreCountPromotionProvider.GetStoreCountPromotions(rows, directory);
            ExcelReader.WriteToFile(promotions, directory);
        }
        
    }
}
