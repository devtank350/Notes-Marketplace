using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalNotesMarketPlace.Models
{
    public class ReviewsViewModel
    {
        public Users TblUser { get; set; }
        public UserProfile TblUserProfile { get; set; }
        public SellerNotesReviews TblSellerNotesReview { get; set; }
    }
}