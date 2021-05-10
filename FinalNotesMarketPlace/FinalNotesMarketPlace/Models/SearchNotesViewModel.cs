using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalNotesMarketPlace.Models
{
    public class SearchNotesViewModel
    {
        public SellerNotes Note { get; set; }
        public int AverageRating { get; set; }
        public int TotalRating { get; set; }
        public int TotalSpam { get; set; }
    }
}