﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalNotesMarketPlace.Models
{
    public class DownloadedNotesViewModel
    {
        public int NoteID { get; set; }
        public int BuyerID { get; set; }
        public int SellerID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Seller { get; set; }
        public string Buyer { get; set; }
        public string SellType { get; set; }
        public decimal? Price { get; set; }
        public DateTime DownloadedDate { get; set; }
    }
}