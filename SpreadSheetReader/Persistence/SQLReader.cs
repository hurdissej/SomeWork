using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SpreadSheetReader.Persistence
{
    public static class SQLReader
    {
        public static IEnumerable<ExcelRow> GetSqlRows()
        {
            var 
        }
    }

    public class EposContext : DbContext
    {
        public IEnumerable<ExcelRow> GetRows()
        {
           return Database.ExecuteSqlCommand(
                "select[Date], [Customer], [Store_code], [Dummy_SKU_Code], [ISP], [Base_ISP], [Total_Volume] from dbo.modelinput_days");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=acumendevdb.database.windows.net;Initial Catalog=CondorChn_Consulting_DB;Integrated Security=False;User Id=ConsultingUser;Password=fiuFSuNIXXWwN2e717bbO4KuO0IKQ4I6jHm3");
        }

    }

    public class EposData
    {
        
    }
}
