using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using FinalNotesMarketPlace;
using FinalNotesMarketPlace.Models;
using FinalNotesMarketPlace.SendEmail;


namespace FinalNotesMarketPlace.Controllers
{
    public class SearchNotesController : Controller
    {

        Notes_MarketPlaceEntities context = new Notes_MarketPlaceEntities();
        // GET: SearchNotes
        [HttpGet]
        [AllowAnonymous]
        [Route("Search")]
        public ActionResult Search(string search, string type, string category, string university, string course, string country, string ratings, int page = 1)
        {
            // if  is logged iusern and logged in user is not member then redirect to admin dashboard
            if (User.Identity.IsAuthenticated)
            {
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();
                if (user.RoleID != context.UserRoles.Where(x => x.Name.ToLower() == "member").Select(x => x.ID).FirstOrDefault())
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
            }

            // Viewbag for active class in navigation
            ViewBag.SearchNotes = "active";

            // viewbag for search results
            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Type = type;
            ViewBag.University = university;
            ViewBag.Course = course;
            ViewBag.Country = country;
            ViewBag.Rating = ratings;

            // viewbag for dropdown lists
            ViewBag.CategoryList = context.NoteCategories.ToList();
            ViewBag.TypeList = context.NoteTypes.ToList();
            ViewBag.CountryList = context.Countries.ToList();
            ViewBag.UniversityList = context.SellerNotes.Where(x => x.IsActive == true && x.UniversityName != null && x.Status == 9).Select(x => x.UniversityName).Distinct();
            ViewBag.CourseList = context.SellerNotes.Where(x => x.IsActive == true && x.Course != null && x.Status == 9).Select(x => x.Course).Distinct();
            ViewBag.RatingList = new List<SelectListItem> { new SelectListItem { Text = "1+", Value = "1" }, new SelectListItem { Text = "2+", Value = "2" }, new SelectListItem { Text = "3+", Value = "3" }, new SelectListItem { Text = "4+", Value = "4" }, new SelectListItem { Text = "5", Value = "5" } };

            // get published notes
            var noteslist = context.SellerNotes.Where(x => x.Status == 9);

            // if search is not empty
            if (!String.IsNullOrEmpty(search))
            {
                noteslist = noteslist.Where(x => x.Title.ToLower().Contains(search.ToLower()) ||
                                                 x.NoteCategories.Name.ToLower().Contains(search.ToLower())
                                            );
            }
            // if note type is not empty
            if (!String.IsNullOrEmpty(type))
            {
                noteslist = noteslist.Where(x => x.NoteType.ToString().ToLower().Contains(type.ToLower()));
            }
            // if note category is nnot empty
            if (!String.IsNullOrEmpty(category))
            {
                noteslist = noteslist.Where(x => x.Category.ToString().ToLower().Contains(category.ToLower()));
            }
            // if university is not empty
            if (!String.IsNullOrEmpty(university))
            {
                noteslist = noteslist.Where(x => x.UniversityName.ToLower().Contains(university.ToLower()));
            }
            // if course is not empty
            if (!String.IsNullOrEmpty(course))
            {
                noteslist = noteslist.Where(x => x.Course.ToLower().Contains(course.ToLower()));
            }
            // if country is not empty
            if (!String.IsNullOrEmpty(country))
            {
                noteslist = noteslist.Where(x => x.Country.ToString().ToLower().Contains(country.ToLower()));
            }

            // create list object of search notes view model
            List<SearchNotesViewModel> searchnoteslist = new List<SearchNotesViewModel>();

            // if rating is empty
            if (String.IsNullOrEmpty(ratings))
            {
                foreach (var item in noteslist)
                {
                    // get reviews
                    var review = context.SellerNotesReviews.Where(x => x.NoteID == item.ID && x.IsActive == true).Select(x => x.Ratings);
                    // count reviews
                    var totalreview = review.Count();
                    // get average reviews
                    var avgreview = totalreview > 0 ? Math.Ceiling(review.Average()) : 0;
                    // get spam report count
                    var spamcount = context.SellerNotesReportedIssues.Where(x => x.NoteID == item.ID).Count();

                    // create searchnotesviewmodel object
                    SearchNotesViewModel note = new SearchNotesViewModel()
                    {
                        Note = item,
                        AverageRating = Convert.ToInt32(avgreview),
                        TotalRating = totalreview,
                        TotalSpam = spamcount
                    };
                    // add object into list
                    searchnoteslist.Add(note);
                }
            }
            // if rating is not empty
            else
            {
                foreach (var item in noteslist)
                {
                    // get reviews
                    var review = context.SellerNotesReviews.Where(x => x.NoteID == item.ID).Select(x => x.Ratings);
                    // count reviews
                    var totalreview = review.Count();
                    // get average reviews
                    var avgreview = totalreview > 0 ? Math.Ceiling(review.Average()) : 0;
                    // get spam report count
                    var spamcount = context.SellerNotesReportedIssues.Where(x => x.NoteID == item.ID).Count();
                    // check if average review is greater than or equal to rating
                    if (avgreview >= Convert.ToInt32(ratings))
                    {
                        // create searchnotesviewmodel object
                        SearchNotesViewModel note = new SearchNotesViewModel()
                        {
                            Note = item,
                            AverageRating = Convert.ToInt32(avgreview),
                            TotalRating = totalreview,
                            TotalSpam = spamcount
                        };
                        // add object into list
                        searchnoteslist.Add(note);
                    }

                }
            }

            // page number
            ViewBag.PageNumber = page;
            // count total pages
            ViewBag.TotalPages = Math.Ceiling(searchnoteslist.Count() / 9.0);
            // show record according to pagination
            IEnumerable<SearchNotesViewModel> result = searchnoteslist.AsEnumerable().Skip((page - 1) * 9).Take(9);
            // total result count
            ViewBag.ResultCount = searchnoteslist.Count();

            return View(result);
        }

        [AllowAnonymous]
        [Route("Search/Notes/{id}")]
        public ActionResult Notes(int id)
        {
            // get logged in user if user is logged in 
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // if  is logged iusern and logged in user is not member then redirect to admin dashboard
            if (User.Identity.IsAuthenticated)
            {
                if (user.RoleID != context.UserRoles.Where(x => x.Name.ToLower() == "member").Select(x => x.ID).FirstOrDefault())
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
            }

            // get note by id
            var NoteDetail = context.SellerNotes.Where(x => x.ID == id && x.IsActive == true).FirstOrDefault();
            // if note is not found
            if (NoteDetail == null)
            {
                return HttpNotFound();
            }
            // get seller
            var seller = context.Users.Where(x => x.ID == NoteDetail.SellerID).FirstOrDefault();
            // get reviews and user's full name and user's image
            IEnumerable<ReviewsViewModel> reviews = from review in context.SellerNotesReviews
                                                    join users in context.Users on review.ReviewedByID equals users.ID
                                                    join userprofile in context.UserProfile on review.ReviewedByID equals userprofile.UserID
                                                    where review.NoteID == id && review.IsActive == true
                                                    orderby review.Ratings descending
                                                    select new ReviewsViewModel { TblSellerNotesReview = review, TblUser = users, TblUserProfile = userprofile };
            // count reviews
            var reviewcounts = reviews.Count();
            // count average review
            decimal avgreview = 0;
            if (reviewcounts > 0)
            {
                avgreview = Math.Ceiling((from x in reviews select x.TblSellerNotesReview.Ratings).Average());
            }
            // count total spam report
            var spams = context.SellerNotesReportedIssues.Where(x => x.NoteID == id).Count();
            // create notesdetailviewmodel object
            NotesDetailViewModel notesdetail = new NotesDetailViewModel();
            //if user is authenticated
            if (user != null)
            {
                notesdetail.UserID = user.ID;
            }
            notesdetail.SellerNote = NoteDetail;
            notesdetail.Seller = seller.FirstName + " " + seller.LastName;
            notesdetail.Buyer = user.FirstName;
            notesdetail.NotesReview = reviews;
            notesdetail.AverageRating = Convert.ToInt32(avgreview);
            notesdetail.TotalReview = reviewcounts;
            notesdetail.TotalSpamReport = spams;
            // check if user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                // check if this note is already requested by logged in user or not
                // if it's already requested then we need to hide download button until seller allows download
                var request = context.Downloads.Where(x => x.NoteID == id && x.Downloader == user.ID && x.IsSellerHasAllowedDownload == false && x.AttachmentPath == null).FirstOrDefault();
                // if logged in user is allow download this note
                var allowdownloadnotes = context.Downloads.Where(x => x.NoteID == id && x.Downloader == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).FirstOrDefault();

                // assign values according to if user is already requested note or allowdownload
                if (request == null && allowdownloadnotes == null)
                {
                    notesdetail.NoteRequested = false;
                }
                else
                {
                    notesdetail.NoteRequested = true;
                }

                if (allowdownloadnotes != null && request == null)
                {
                    notesdetail.AllowDownload = true;
                }
                else
                {
                    notesdetail.AllowDownload = false;
                }
            }

            if (TempData["Requested"] != null)
            {
                ViewBag.Requested = "Requested";
            }

            return View(notesdetail);
        }

        [Authorize(Roles = "Member")]
        [Route("Search/Notes/{noteid}/Download")]
        public ActionResult DownloadNotes(int noteid)
        {
            // get note 
            var note = context.SellerNotes.Find(noteid);
            // if note is not found
            if (note == null)
            {
                return HttpNotFound();
            }
            // get first object of seller note attachement for attachement
            var noteattachement = context.SellerNotesAttachments.Where(x => x.NoteID == note.ID).FirstOrDefault();
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // variable for attachement path
            string path;

            // note's seller if and logged in user's id is same it means user want to download his own book 
            // then we need to provide downloaded note without entry in download tables
            if (note.SellerID == user.ID)
            {
                // get attavhement path
                path = Server.MapPath(noteattachement.FilePath);

                DirectoryInfo dir = new DirectoryInfo(path);
                // create zip of attachement
                using (var memoryStream = new MemoryStream())
                {
                    using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var item in dir.GetFiles())
                        {
                            // file path is attachement path + file name
                            string filepath = path + item.ToString();
                            ziparchive.CreateEntryFromFile(filepath, item.ToString());
                        }
                    }
                    // return zip
                    return File(memoryStream.ToArray(), "application/zip", note.Title + ".zip");
                }
            }

            // if note is free then we need to add entry in download table with allow download is true
            // downloaded date time is current date time for first time download
            // if user download again then we have to return zip of attachement without changes in data
            if (note.IsPaid == false)
            {
                // if user has already downloaded note then get download object
                var downloadfreenote = context.Downloads.Where(x => x.NoteID == noteid && x.Downloader == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).FirstOrDefault();
                // if user is not downloaded 
                if (downloadfreenote == null)
                {
                    // create download object
                    Downloads download = new Downloads
                    {
                        NoteID = note.ID,
                        Seller = note.SellerID,
                        Downloader = user.ID,
                        IsSellerHasAllowedDownload = true,
                        AttachmentPath = noteattachement.FilePath,
                        IsAttachmentDownloaded = true,
                        AttachmentDownloadedDate = DateTime.Now,
                        IsPaid = note.IsPaid,
                        PurchasedPrice = note.SellingPrice,
                        NoteTitle = note.Title,
                        NoteCategory = note.NoteCategories.Name,
                        CreatedDate = DateTime.Now,
                        CreatedBy = user.ID,
                    };

                    // save download object in database
                    context.Downloads.Add(download);
                    context.SaveChanges();

                    path = Server.MapPath(download.AttachmentPath);
                }
                // if user is already downloaded note then get attachement path
                else
                {
                    path = Server.MapPath(downloadfreenote.AttachmentPath);
                }

                DirectoryInfo dir = new DirectoryInfo(path);
                // create zip of attachement
                using (var memoryStream = new MemoryStream())
                {
                    using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var item in dir.GetFiles())
                        {
                            // file path is attachement path + file name
                            string filepath = path + item.ToString();
                            ziparchive.CreateEntryFromFile(filepath, item.ToString());
                        }
                    }
                    // return zip
                    return File(memoryStream.ToArray(), "application/zip", note.Title + ".zip");
                }
            }
            // if note is paid 
            else
            {
                // get download object
                var downloadpaidnote = context.Downloads.Where(x => x.NoteID == noteid && x.Downloader == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).FirstOrDefault();

                // if user is not already downloaded
                if (downloadpaidnote != null)
                {
                    // if user is download note first time then we need to update following record in download table 
                    if (downloadpaidnote.IsAttachmentDownloaded == false)
                    {
                        downloadpaidnote.IsAttachmentDownloaded = true;
                        downloadpaidnote.ModifiedDate = DateTime.Now;
                        downloadpaidnote.NodifiedBy = user.ID;

                        // update ans save data in database
                        context.Entry(downloadpaidnote).State = EntityState.Modified;
                        context.SaveChanges();
                    }

                    // get attachement path
                    path = Server.MapPath(downloadpaidnote.AttachmentPath);

                    DirectoryInfo dir = new DirectoryInfo(path);

                    // create zip of attachement
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var ziparchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {
                            foreach (var item in dir.GetFiles())
                            {
                                // file path is attachement path + file name
                                string filepath = path + item.ToString();
                                ziparchive.CreateEntryFromFile(filepath, item.ToString());
                            }
                        }
                        // return zip
                        return File(memoryStream.ToArray(), "application/zip", note.Title + ".zip");
                    }
                }
                return RedirectToAction("Notes", "SearchNotes", new { id = noteid });
            }
        }


        [Authorize(Roles = "Member")]
        [Route("Search/Notes/{noteid}/Request")]
        public ActionResult RequestPaidNotes(int noteid)
        {
            // get note
            var note = context.SellerNotes.Find(noteid);
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // create download object
            Downloads download = new Downloads
            {
                NoteID = note.ID,
                Seller = note.SellerID,
                Downloader = user.ID,
                IsSellerHasAllowedDownload = false,
                IsAttachmentDownloaded = false,
                IsPaid = note.IsPaid,
                PurchasedPrice = note.SellingPrice,
                NoteTitle = note.Title,
                NoteCategory = note.NoteCategories.Name,
                CreatedDate = DateTime.Now,
                CreatedBy = user.ID,
            };

            // add and save data in database
            context.Downloads.Add(download);
            context.SaveChanges();

            // sned mail
            RequestPaidNotesTemplate(download, user);

            TempData["Requested"] = "Requested";

            return RedirectToAction("Notes", new { id = note.ID });
        }

        // for request to download we need to send mail to seller 
        public void RequestPaidNotesTemplate(Downloads download, Users user)
        {
            // get all text from requestpaidnotes from emailtemplate directory
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "RequestPaidNotes" + ".cshtml");
            //get seller
            var seller = context.Users.Where(x => x.ID == download.Seller).FirstOrDefault();
            // replace seller name and buyer name from template
            body = body.Replace("ViewBag.SellerName", seller.FirstName);
            body = body.Replace("ViewBag.BuyerName", user.FirstName);
            body = body.ToString();

            // get support email
            var fromemail = context.SystemConfiguration.Where(x => x.Key == "supportemail").FirstOrDefault();

            // set from, to, subject, body
            string from, to, subject;
            from = fromemail.Value.Trim();
            to = seller.EmailID.Trim();
            subject = user.FirstName + " wants to purchase your notes";
            StringBuilder sb = new StringBuilder();
            sb.Append(body);
            body = sb.ToString();

            // create mailmessage object
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(from, "NotesMarketplace");
            mail.To.Add(new MailAddress(to));
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            // send mail (NotesMarketplace/SendMail)
            SendingEmail.SendEmail(mail);
        }

    }
}