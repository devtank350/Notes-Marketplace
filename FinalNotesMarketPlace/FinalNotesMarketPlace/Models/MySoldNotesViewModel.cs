﻿using FinalNotesMarketPlace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalNotesMarketPlace.Models
{
    public class MySoldNotesViewModel
    {
        public int NoteID { get; set; }
        public int DownloadID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Buyer { get; set; }
        public string SellType { get; set; }
        public decimal? Price { get; set; }
        public DateTime? DownloadedDate { get; set; }
        public bool NoteDownloaded { get; set; }
    }
}