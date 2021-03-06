﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SpreadSheetReader
{
    public class Promotion
    {
        public Promotion(DateTime startdate, DateTime endDate, double price, string sku, int storeCount, double volume, string customer, int storeCountForCustomer, List<string> province)
        {
            Id = Guid.NewGuid();
            StartDate = startdate;
            EndDate = endDate;
            Sku = sku;
            PromotedPrice = price;
            Volume = volume;
            MinimumNumberOfStores = storeCount;
            MaximumNumberOfStores = storeCount;
            NumberOfStoresInCustomerGroup = storeCountForCustomer;
            PercentageOfStores = ((double)storeCount / (double)storeCountForCustomer);
            Customer = customer;
            Province = province;
        }

        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Sku { get; set; }
        public string Customer { get; set; }
        public List<string> Province { get; set; }
        public double PromotedPrice { get; set; }
        public double Volume { get; set; }
        public int MinimumNumberOfStores { get; set; }
        public int MaximumNumberOfStores { get; set; }
        public int NumberOfStoresInCustomerGroup { get; set; }
        public double PercentageOfStores { get; set; }
        public double StandardDeviation { get; set; }
    }
}
