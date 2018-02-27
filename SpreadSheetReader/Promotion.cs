using System;
using System.Collections.Generic;
using System.Text;

namespace SpreadSheetReader
{
    public class Promotion
    {
        public Promotion(DateTime startdate, DateTime endDate, double price, string sku, int storeCount, double volume, string customer)
        {
            Id = Guid.NewGuid();
            StartDate = startdate;
            EndDate = endDate;
            Sku = sku;
            PromotedPrice = price;
            Volume = volume;
            NumberOfStores = storeCount;
            Customer = customer;
        }

        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Sku { get; set; }
        public string Customer { get; set; }
        public double PromotedPrice { get; set; }
        public double Volume { get; set; }
        public int NumberOfStores { get; set; }
        public int NumberOfStoresInCustomerGroup { get; set; }
        public double StandardDeviation { get; set; }
    }
}
