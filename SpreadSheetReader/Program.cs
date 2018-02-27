using System.Linq;
using SpreadSheetReader.Persistence;

namespace SpreadSheetReader
{
    public class Program
    {
        static void Main(string[] args)
        {
            var directory = @"INSERT PATH HERE";
            var rows = ExcelReader.ParsePromotionRows(ExcelReader.getExcelDump($@"{directory}\INSERTFILENAME.csv")).ToList();
            var promotions = StoreCountPromotionProvider.GetStoreCountPromotions(rows, directory);
            ExcelReader.WriteToFile(promotions, directory);
        }
        
    }
}
