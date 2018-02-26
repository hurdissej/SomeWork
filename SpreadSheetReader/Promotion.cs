using System;
using System.Collections.Generic;
using System.Text;

namespace SpreadSheetReader
{
    public class Promotion
    {

        public Promotion(DateTime startdate, DateTime endDate, double price, string sku, string store, double volume)
        {
            Id = Guid.NewGuid();
            StartDate = startdate;
            EndDate = endDate;
            Sku = sku;
            PromotedPrice = price;
            Volume = volume;
            NumberOfStores = Convert.ToInt32(store);
        }

        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Sku { get; set; }
        public double PromotedPrice { get; set; }
        public double Volume { get; set; }
        public int NumberOfStores { get; set; }
    }

    public class StoreVolume
    {
        public StoreVolume(int initialDays, double dayOneVolume, double price)
        {
            DaysRanInStore = initialDays;
            TotalVolume = dayOneVolume;
            InitialPrice = price;
        }

        public double InitialPrice { get; set; }
        public int DaysRanInStore { get; set; }
        public double TotalVolume { get; set; }

    }
}
