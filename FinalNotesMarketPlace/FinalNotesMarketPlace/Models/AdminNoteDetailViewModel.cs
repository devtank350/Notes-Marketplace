using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalNotesMarketPlace.Models
{
    public class AdminNoteDetailViewModel
    {
        public int? UserID { get; set; }
        public SellerNotes SellerNote { get; set; }
        public IEnumerable<UserReviewsViewModel> NotesReview { get; set; }
        public int? AverageRating { get; set; }
        public int? TotalReview { get; set; }
        public int? TotalSpamReport { get; set; }
    }

    public class UserReviewsViewModel
    {
        public Users TblUser { get; set; }
        public UserProfile TblUserProfile { get; set; }
        public SellerNotesReviews TblSellerNotesReview { get; set; }
    }

}

