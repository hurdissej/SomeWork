using System.Linq;
using SpreadSheetReader.Persistence;

namespace SpreadSheetReader
{
    public class Program
    {
        static void Main(string[] args)
        {
            var rows = ExcelReader.ParsePromotionRows(ExcelReader.getExcelDump(@"C:\Users\elliot.hurdiss\Documents\OrderedDataSet.csv")).ToList();
            var promotions = StoreCountPromotionProvider.GetStoreCountPromotions(rows);
            ExcelReader.WriteToFile(promotions);
        }
        
    }
}
