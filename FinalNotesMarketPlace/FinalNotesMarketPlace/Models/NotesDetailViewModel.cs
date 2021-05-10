﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalNotesMarketPlace.Models
{
    public class NotesDetailViewModel
    {

        public int? UserID { get; set; }
        public SellerNotes SellerNote { get; set; }
        public string Seller { get; set; }
        public string Buyer { get; set; }
        public IEnumerable<ReviewsViewModel> NotesReview { get; set; }
        public int? AverageRating { get; set; }
        public int? TotalReview { get; set; }
        public int? TotalSpamReport { get; set; }
        public bool NoteRequested { get; set; }
        public bool AllowDownload { get; set; }


    }
}