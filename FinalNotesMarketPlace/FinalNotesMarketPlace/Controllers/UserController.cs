﻿using FinalNotesMarketPlace.SendEmail;
using FinalNotesMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace FinalNotesMarketPlace.Controllers
{
    public class UserController : Controller
    {
        Notes_MarketPlaceEntities context = new Notes_MarketPlaceEntities();

        [HttpGet]
        [Authorize(Roles = "Member")]
        [Route("Profile")]
        public ActionResult UserProfile()
        {
            // get logged in user 
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get logged in user's profile if exists
            var userprofile = context.UserProfile.Where(x => x.UserID == user.ID).FirstOrDefault();

            // create object of userprofileviewmodel
            UserProfileViewModel userprofileviewmodel = new UserProfileViewModel();

            // if userprofile is not null then store it's value in userprofileviewmodel object for edit profile
            if (userprofile != null)
            {
                userprofileviewmodel.CountryList = context.Countries.Where(x => x.IsActive == true).ToList();
                userprofileviewmodel.GenderList = context.ReferenceData.Where(x => x.RefCategory == "Gender" && x.IsActive == true).ToList();
                userprofileviewmodel.UserID = user.ID;
                userprofileviewmodel.EmailID = user.EmailID;
                userprofileviewmodel.FirstName = user.FirstName;
                userprofileviewmodel.LastName = user.LastName;
                userprofileviewmodel.DOB = userprofile.DOB;
                userprofileviewmodel.PhoneNumberCountryCode = userprofile.PhoneNumberCountryCode;
                userprofileviewmodel.PhoneNumber = userprofile.PhoneNumber;
                userprofileviewmodel.Gender = userprofile.Gender;
                userprofileviewmodel.AddressLine1 = userprofile.AddressLine1;
                userprofileviewmodel.AddressLine2 = userprofile.AddressLine2;
                userprofileviewmodel.City = userprofile.City;
                userprofileviewmodel.State = userprofile.State;
                userprofileviewmodel.ZipCode = userprofile.ZipCode;
                userprofileviewmodel.Country = userprofile.Country;
                userprofileviewmodel.University = userprofile.University;
                userprofileviewmodel.College = userprofile.College;
                userprofileviewmodel.ProfilePictureUrl = userprofile.ProfilePicture;
            }
            //if userprofile is null then create it
            else
            {
                userprofileviewmodel.CountryList = context.Countries.Where(x => x.IsActive == true).ToList();
                userprofileviewmodel.GenderList = context.ReferenceData.Where(x => x.RefCategory.ToLower() == "gender" && x.IsActive == true).ToList();
                userprofileviewmodel.UserID = user.ID;
                userprofileviewmodel.EmailID = user.EmailID;
                userprofileviewmodel.FirstName = user.FirstName;
                userprofileviewmodel.LastName = user.LastName;
            }

            // return userprofileviewmodel
            return View(userprofileviewmodel);
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        [Route("Profile")]
        public ActionResult UserProfile(UserProfileViewModel userprofileviewmodel)
        {
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // check if ModelState is valid or not
            if (ModelState.IsValid)
            {
                // check if profile picture is not null
                if (userprofileviewmodel.ProfilePicture != null)
                {
                    // get image size and check that it is not more than 10 MB
                    var profilepicsize = userprofileviewmodel.ProfilePicture.ContentLength;
                    if (profilepicsize > 10 * 1024 * 1024)
                    {
                        // if image size is more than 10 MB show error
                        ModelState.AddModelError("ProfilePicture", "Image size limit is 10 MB");
                        userprofileviewmodel.CountryList = context.Countries.Where(x => x.IsActive == true).ToList();
                        userprofileviewmodel.GenderList = context.ReferenceData.Where(x => x.RefCategory == "Gender" && x.IsActive == true).ToList();
                        return View(userprofileviewmodel);
                    }
                }

                // get logged in user's profile if exists
                var profile = context.UserProfile.Where(x => x.UserID == user.ID).FirstOrDefault();

                // if profile is not null then edit userprofile
                if (profile != null)
                {
                    profile.DOB = userprofileviewmodel.DOB;
                    profile.Gender = userprofileviewmodel.Gender;
                    profile.PhoneNumberCountryCode = userprofileviewmodel.PhoneNumberCountryCode.Trim();
                    profile.PhoneNumber = userprofileviewmodel.PhoneNumber.Trim();
                    profile.AddressLine1 = userprofileviewmodel.AddressLine1.Trim();
                    profile.AddressLine2 = userprofileviewmodel.AddressLine2.Trim();
                    profile.City = userprofileviewmodel.City.Trim();
                    profile.State = userprofileviewmodel.State.Trim();
                    profile.ZipCode = userprofileviewmodel.ZipCode.Trim();
                    profile.Country = userprofileviewmodel.Country.Trim();
                    profile.University = userprofileviewmodel.University.Trim();
                    profile.College = userprofileviewmodel.College.Trim();
                    profile.ModifiedDate = DateTime.Now;
                    profile.ModifiedBy = user.ID;

                    // check if loggedin user's profile picture is not null and user upload new profile picture then delete old one
                    if (userprofileviewmodel.ProfilePicture != null && profile.ProfilePicture != null)
                    {
                        string path = Server.MapPath(profile.ProfilePicture);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    // if user upload profile picture then save it and store path in database
                    if (userprofileviewmodel.ProfilePicture != null)
                    {
                        string filename = System.IO.Path.GetFileName(userprofileviewmodel.ProfilePicture.FileName);
                        string fileextension = System.IO.Path.GetExtension(userprofileviewmodel.ProfilePicture.FileName);
                        string newfilename = "DP_" + DateTime.Now.ToString("ddMMyyyy_hhmmss") + fileextension;
                        string profilepicturepath = "~/Members/" + profile.UserID + "/";
                        CreateDirectoryIfMissing(profilepicturepath);
                        string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                        profile.ProfilePicture = profilepicturepath + newfilename;
                        userprofileviewmodel.ProfilePicture.SaveAs(path);
                    }

                    // update logged in user's profile and save
                    context.Entry(profile).State = EntityState.Modified;
                    context.SaveChanges();

                    // update first name and lastname and save
                    user.FirstName = userprofileviewmodel.FirstName.Trim();
                    user.LastName = userprofileviewmodel.LastName.Trim();
                    context.Entry(user).State = EntityState.Modified;
                    context.SaveChanges();

                }
                // if profile is null then create user profile
                else
                {
                    // create new userprofile object
                    UserProfile userprofile = new UserProfile();

                    userprofile.UserID = user.ID;
                    userprofile.DOB = userprofileviewmodel.DOB;
                    userprofile.Gender = userprofileviewmodel.Gender;
                    userprofile.PhoneNumberCountryCode = userprofileviewmodel.PhoneNumberCountryCode.Trim();
                    userprofile.PhoneNumber = userprofileviewmodel.PhoneNumber.Trim();
                    userprofile.AddressLine1 = userprofileviewmodel.AddressLine1.Trim();
                    userprofile.AddressLine2 = userprofileviewmodel.AddressLine2.Trim();
                    userprofile.City = userprofileviewmodel.City.Trim();
                    userprofile.State = userprofileviewmodel.State.Trim();
                    userprofile.ZipCode = userprofileviewmodel.ZipCode.Trim();
                    userprofile.Country = userprofileviewmodel.Country.Trim();
                    userprofile.University = userprofileviewmodel.University.Trim();
                    userprofile.College = userprofileviewmodel.College.Trim();
                    userprofile.CreatedDate = DateTime.Now;
                    userprofile.CreatedBy = user.ID;

                    // if profile picture is not null then save it and store path in database with filename is timestamp
                    if (userprofileviewmodel.ProfilePicture != null)
                    {
                        string filename = System.IO.Path.GetFileName(userprofileviewmodel.ProfilePicture.FileName);
                        string fileextension = System.IO.Path.GetExtension(userprofileviewmodel.ProfilePicture.FileName);
                        string newfilename = "DP_" + DateTime.Now.ToString("ddMMyyyy_hhmmss") + fileextension;
                        string profilepicturepath = "~/Members/" + userprofile.UserID + "/";
                        CreateDirectoryIfMissing(profilepicturepath);
                        string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                        userprofile.ProfilePicture = profilepicturepath + newfilename;
                        userprofileviewmodel.ProfilePicture.SaveAs(path);
                    }

                    // save logged in user's profile
                    context.UserProfile.Add(userprofile);
                    context.SaveChanges();

                    // save firstname and lastname and save
                    user.FirstName = userprofileviewmodel.FirstName.Trim();
                    user.LastName = userprofileviewmodel.LastName.Trim();
                    context.Entry(user).State = EntityState.Modified;
                    context.SaveChanges();
                }

                // if userprofile is created or edited then redirect to search page
                return RedirectToAction("Search", "SearchNotes");
            }
            // if ModelState is invalid then redirect to userProfile page
            else
            {
                userprofileviewmodel.CountryList = context.Countries.Where(x => x.IsActive == true).ToList();
                userprofileviewmodel.GenderList = context.ReferenceData.Where(x => x.RefCategory == "Gender" && x.IsActive == true).ToList();
                return View(userprofileviewmodel);
            }
        }

        // Create Directory
        private void CreateDirectoryIfMissing(string folderpath)
        {
            // check if directory is exists or not
            bool folderalreadyexists = Directory.Exists(Server.MapPath(folderpath));
            // if directory is not exists then create directory
            if (!folderalreadyexists)
                Directory.CreateDirectory(Server.MapPath(folderpath));
        }

        [HttpGet]
        [Authorize(Roles = "Member")]
        [Route("MyDownloads")]
        public ActionResult MyDownloads(string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get downloaded notes
            IEnumerable<MyDownloadsViewModel> mydownloads = from download in context.Downloads
                                                            join users in context.Users on download.Seller equals users.ID
                                                            join review in context.SellerNotesReviews on download.ID equals review.AgainstDownloadsID into r
                                                            from notereview in r.DefaultIfEmpty()
                                                            where download.Downloader == user.ID && download.IsSellerHasAllowedDownload == true && download.AttachmentPath != null
                                                            select new MyDownloadsViewModel
                                                            {
                                                                NoteID = download.NoteID,
                                                                DownloadID = download.ID,
                                                                Title = download.NoteTitle,
                                                                Category = download.NoteCategory,
                                                                Seller = users.EmailID,
                                                                SellType = download.IsPaid == true ? "Paid" : "Free",
                                                                Price = download.PurchasedPrice,
                                                                DownloadedDate = download.AttachmentDownloadedDate.Value,
                                                                NoteDownloaded = download.IsAttachmentDownloaded,
                                                                ReviewID = notereview.ID,
                                                                Rating = notereview.Ratings,
                                                                Comment = notereview.Comments
                                                            };

            // get searched result if search is not empty
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                mydownloads = mydownloads.Where(x => x.Title.ToLower().Contains(search) ||
                                                     x.Category.ToLower().Contains(search) ||
                                                     x.Seller.ToLower().Contains(search) ||
                                                     x.Price.ToString().ToLower().Contains(search) ||
                                                     x.SellType.ToLower().Contains(search)
                                               );
            }

            // sorting result
            mydownloads = SortTableMyDownloads(sort, mydownloads);

            // viewbag for count total pages
            ViewBag.TotalPages = Math.Ceiling(mydownloads.Count() / 10.0);

            // show result based on pagination
            mydownloads = mydownloads.Skip((page - 1) * 10).Take(10);

            // return results
            return View(mydownloads);
        }

        // sorting for my downloads results
        private IEnumerable<MyDownloadsViewModel> SortTableMyDownloads(string sort, IEnumerable<MyDownloadsViewModel> table)
        {
            switch (sort)
            {
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.Title);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.Title);
                        break;
                    }
                case "Category_Asc":
                    {
                        table = table.OrderBy(x => x.Category);
                        break;
                    }
                case "Category_Desc":
                    {
                        table = table.OrderByDescending(x => x.Category);
                        break;
                    }
                case "Seller_Asc":
                    {
                        table = table.OrderBy(x => x.Seller);
                        break;
                    }
                case "Seller_Desc":
                    {
                        table = table.OrderByDescending(x => x.Seller);
                        break;
                    }
                case "Type_Asc":
                    {
                        table = table.OrderBy(x => x.SellType);
                        break;
                    }
                case "Type_Desc":
                    {
                        table = table.OrderByDescending(x => x.SellType);
                        break;
                    }
                case "Price_Asc":
                    {
                        table = table.OrderBy(x => x.Price);
                        break;
                    }
                case "Price_Desc":
                    {
                        table = table.OrderByDescending(x => x.Price);
                        break;
                    }
                case "DownloadedDate_Asc":
                    {
                        table = table.OrderBy(x => x.DownloadedDate);
                        break;
                    }
                case "DownloadedDate_Desc":
                    {
                        table = table.OrderByDescending(x => x.DownloadedDate);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DownloadedDate);
                        break;
                    }
            }
            return table;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        [Route("Note/AddReview")]
        public ActionResult AddReview(SellerNotesReviews notereview)
        {
            // check if comment is null or not
            if (String.IsNullOrEmpty(notereview.Comments))
            {
                return RedirectToAction("MyDownloads");
            }

            // check if rating is between 1 to 5
            if (notereview.Ratings < 1 || notereview.Ratings > 5)
            {
                return RedirectToAction("MyDownloads");
            }

            // get Download object for check if user is downloaded note or not
            var notedownloaded = context.Downloads.Where(x => x.ID == notereview.AgainstDownloadsID && x.IsAttachmentDownloaded == true).FirstOrDefault();

            // user can provide review after downloading the note
            if (notedownloaded != null)
            {
                //get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // check if user already provided review or not
                var alreadyprovidereview = context.SellerNotesReviews.Where(x => x.AgainstDownloadsID == notereview.AgainstDownloadsID && x.IsActive == true).FirstOrDefault();

                // if user not provide review then add review
                if (alreadyprovidereview == null)
                {
                    // create a sellernotesreview object and initialize it
                    SellerNotesReviews review = new SellerNotesReviews();

                    review.NoteID = notereview.NoteID;
                    review.AgainstDownloadsID = notereview.AgainstDownloadsID;
                    review.ReviewedByID = user.ID;
                    review.Ratings = notereview.Ratings;
                    review.Comments = notereview.Comments;
                    review.CreatedDate = DateTime.Now;
                    review.CreatedBy = user.ID;
                    review.IsActive = true;

                    // save sellernotesreview into database
                    context.SellerNotesReviews.Add(review);
                    context.SaveChanges();

                    return RedirectToAction("MyDownloads");
                }
                // if user is already provided review then edit it
                else
                {
                    alreadyprovidereview.Ratings = notereview.Ratings;
                    alreadyprovidereview.Comments = notereview.Comments;
                    alreadyprovidereview.ModifiedDate = DateTime.Now;
                    alreadyprovidereview.ModifiedBy = user.ID;

                    // update review and save in database
                    context.Entry(alreadyprovidereview).State = EntityState.Modified;
                    context.SaveChanges();

                    return RedirectToAction("MyDownloads");
                }
            }
            return RedirectToAction("MyDownloads");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        [Route("Note/ReportSpam")]
        public ActionResult SpamReport(FormCollection form)
        {
            // get download id by form 
            int downloadid = Convert.ToInt32(form["downloadid"]);

            // get ReportedIssues object 
            var alreadyreportedspam = context.SellerNotesReportedIssues.Where(x => x.AgainstDownloadsID == downloadid).FirstOrDefault();

            // if user not slready reported spam 
            if (alreadyreportedspam == null)
            {
                //get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

                // store logged in user name into variable
                string membername = user.FirstName + " " + user.LastName;

                // create a spam report object
                SellerNotesReportedIssues spamnote = new SellerNotesReportedIssues();

                spamnote.NoteID = Convert.ToInt32(form["noteid"]);
                spamnote.AgainstDownloadsID = downloadid;
                spamnote.ReportedByID = user.ID;
                spamnote.Remarks = form["spamreport"];
                spamnote.CreatedDate = DateTime.Now;
                spamnote.CreatedBy = user.ID;

                // save spam report object into database
                context.SellerNotesReportedIssues.Add(spamnote);
                context.SaveChanges();

                // send mail to admin that buyer reported the notes as inappropriate
                SpamReportTemplate(spamnote, membername);
            }
            return RedirectToAction("MyDownloads");
        }

        // intializing the template that we want to send to admin for marking note as inappropriate
        private void SpamReportTemplate(SellerNotesReportedIssues spam, string membername)
        {
            string from, to, body, subject;

            // get all texts from SpamReport.cshtml from EmailTemplate
            body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "SpamReport" + ".cshtml");

            // get notes and user by using SellerNotesReportedIssue object
            var note = context.SellerNotes.Find(spam.NoteID);
            var seller = context.Users.Find(note.SellerID);

            // replace some text with note title, seller name and user's name who mark this note as inappropriate
            body = body.Replace("ViewBag.SellerName", seller.FirstName + " " + seller.LastName);
            body = body.Replace("ViewBag.NoteTitle", note.Title);
            body = body.Replace("ViewBag.ReportedBy", membername);
            body = body.ToString();

            // get support email
            var fromemail = context.SystemConfiguration.Where(x => x.Key == "supportemail").FirstOrDefault();
            var tomail = context.SystemConfiguration.Where(x => x.Key == "notifyemail").FirstOrDefault();

            // set from, to, subject, body
            from = fromemail.Value.Trim();
            to = tomail.Value.Trim();
            subject = membername + " Reported an issue for " + note.Title;
            StringBuilder sb = new StringBuilder();
            sb.Append(body);
            body = sb.ToString();

            // create object of MailMessage
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(from, "NotesMarketplace");
            mail.To.Add(new MailAddress(to));
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            // send mail (NotesMarketplace/SendMail/)
            SendingEmail.SendEmail(mail);
        }

        [HttpGet]
        [Authorize(Roles = "Member")]
        [Route("MyRejectedNotes")]
        public ActionResult MyRejectedNotes(string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get rejected id
            var rejectedid = context.ReferenceData.Where(x => x.Value.ToLower() == "rejected").Select(x => x.ID).FirstOrDefault();

            // get user's rejected notes 
            IEnumerable<SellerNotes> rejectednotes = context.SellerNotes.Where(x => x.SellerID == user.ID && x.Status == rejectedid && x.IsActive == true).ToList();

            // get searched result if search is not empty
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                rejectednotes = rejectednotes.Where(x => x.Title.ToLower().Contains(search) ||
                                                     x.NoteCategories.Name.ToLower().Contains(search) ||
                                                     x.AdminRemarks.ToLower().Contains(search)).ToList();
            }

            // sort result
            rejectednotes = SortTableMyRejectedNotes(sort, rejectednotes);

            // viewbag for count total pages
            ViewBag.TotalPages = Math.Ceiling(rejectednotes.Count() / 10.0);

            // show result based on pagination
            rejectednotes = rejectednotes.Skip((page - 1) * 10).Take(10).ToList();

            // return rejectedd notes result
            return View(rejectednotes);
        }

        //sorting for my rejected notes
        private IEnumerable<SellerNotes> SortTableMyRejectedNotes(string sort, IEnumerable<SellerNotes> table)
        {
            switch (sort)
            {
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.Title);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.Title);
                        break;
                    }
                case "Category_Asc":
                    {
                        table = table.OrderBy(x => x.NoteCategories.Name);
                        break;
                    }
                case "Category_Desc":
                    {
                        table = table.OrderByDescending(x => x.NoteCategories.Name);
                        break;
                    }
                case "Remark_Asc":
                    {
                        table = table.OrderBy(x => x.AdminRemarks);
                        break;
                    }
                case "Remark_Desc":
                    {
                        table = table.OrderByDescending(x => x.AdminRemarks);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.ModifiedDate);
                        break;
                    }
            }
            return table;
        }

        [HttpGet]
        [Authorize(Roles = "Member")]
        [Route("MyRejectedNotes/{noteid}/Clone")]
        public ActionResult CloneNote(int noteid)
        {
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get rejected note by id
            var rejectednote = context.SellerNotes.Find(noteid);

            // create object of sellernote for create clone of note
            SellerNotes clonenote = new SellerNotes();

            clonenote.SellerID = rejectednote.SellerID;
            clonenote.Status = context.ReferenceData.Where(x => x.Value.ToLower() == "draft").Select(x => x.ID).FirstOrDefault();
            clonenote.Title = rejectednote.Title;
            clonenote.Category = rejectednote.Category;
            clonenote.NoteType = rejectednote.NoteType;
            clonenote.NumberOfPages = rejectednote.NumberOfPages;
            clonenote.Description = rejectednote.Description;
            clonenote.UniversityName = rejectednote.UniversityName;
            clonenote.Country = rejectednote.Country;
            clonenote.Course = rejectednote.Course;
            clonenote.CourseCode = rejectednote.CourseCode;
            clonenote.Professor = rejectednote.Professor;
            clonenote.IsPaid = rejectednote.IsPaid;
            clonenote.SellingPrice = rejectednote.SellingPrice;
            clonenote.CreatedBy = user.ID;
            clonenote.CreatedDate = DateTime.Now;
            clonenote.IsActive = true;

            // save note in database
            context.SellerNotes.Add(clonenote);
            context.SaveChanges();

            // get clonenote 
            clonenote = context.SellerNotes.Find(clonenote.ID);

            // if display picture is not null then copy file from rejected note's folder to clone note's new folder
            if (rejectednote.DisplayPicture != null)
            {
                var rejectednotefilepath = Server.MapPath(rejectednote.DisplayPicture);
                var clonenotefilepath = "~/Members/" + user.ID + "/" + clonenote.ID + "/";

                var filepath = Path.Combine(Server.MapPath(clonenotefilepath));

                FileInfo file = new FileInfo(rejectednotefilepath);

                Directory.CreateDirectory(filepath);
                if (file.Exists)
                {
                    System.IO.File.Copy(rejectednotefilepath, Path.Combine(filepath, Path.GetFileName(rejectednotefilepath)));
                }

                clonenote.DisplayPicture = Path.Combine(clonenotefilepath, Path.GetFileName(rejectednotefilepath));
                context.SaveChanges();
            }

            // if note preview is not null then copy file from rejected note's folder to clone note's new folder
            if (rejectednote.NotesPreview != null)
            {
                var rejectednotefilepath = Server.MapPath(rejectednote.NotesPreview);
                var clonenotefilepath = "~/Members/" + user.ID + "/" + clonenote.ID + "/";

                var filepath = Path.Combine(Server.MapPath(clonenotefilepath));

                FileInfo file = new FileInfo(rejectednotefilepath);

                Directory.CreateDirectory(filepath);

                if (file.Exists)
                {
                    System.IO.File.Copy(rejectednotefilepath, Path.Combine(filepath, Path.GetFileName(rejectednotefilepath)));
                }

                clonenote.NotesPreview = Path.Combine(clonenotefilepath, Path.GetFileName(rejectednotefilepath));
                context.SaveChanges();
            }

            // attachment path of rejected note and clone note
            var rejectednoteattachement = Server.MapPath("~/Members/" + user.ID + "/" + rejectednote.ID + "/Attachements/");
            var clonenoteattachement = "~/Members/" + user.ID + "/" + clonenote.ID + "/Attachements/";

            var attachementfilepath = Path.Combine(Server.MapPath(clonenoteattachement));

            // create directory for attachement folder
            Directory.CreateDirectory(attachementfilepath);

            // get attachements files from rejected note and copy to clone note
            foreach (var files in Directory.GetFiles(rejectednoteattachement))
            {

                FileInfo file = new FileInfo(files);

                if (file.Exists)
                {
                    System.IO.File.Copy(file.ToString(), Path.Combine(attachementfilepath, Path.GetFileName(file.ToString())));
                }

                // save attachment in database
                SellerNotesAttachments attachement = new SellerNotesAttachments();
                attachement.NoteID = clonenote.ID;
                attachement.FileName = Path.GetFileName(file.ToString());
                attachement.FilePath = clonenoteattachement;
                attachement.CreatedDate = DateTime.Now;
                attachement.CreatedBy = user.ID;
                attachement.IsActive = true;

                context.SellerNotesAttachments.Add(attachement);
                context.SaveChanges();
            }
            return RedirectToAction("Dashboard", "SellYourNotes");
        }

        [HttpGet]
        [Authorize(Roles = "Member")]
        [Route("MySoldNotes")]
        public ActionResult MySoldNotes(string search, string sort, int page = 1)
        {
            //for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            //get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            //get my sold notes
            IEnumerable<MySoldNotesViewModel> mysoldnotes = from download in context.Downloads
                                                            join users in context.Users on download.Downloader equals users.ID
                                                            where download.Seller == user.ID && download.IsSellerHasAllowedDownload == true && download.AttachmentPath != null
                                                            select new MySoldNotesViewModel
                                                            {
                                                                DownloadID = download.ID,
                                                                NoteID = download.NoteID,
                                                                Title = download.NoteTitle,
                                                                Category = download.NoteCategory,
                                                                Buyer = users.EmailID,
                                                                SellType = download.IsPaid == true ? "Paid" : "Free",
                                                                Price = download.PurchasedPrice,
                                                                DownloadedDate = download.AttachmentDownloadedDate.Value,
                                                                NoteDownloaded = download.IsAttachmentDownloaded
                                                            };

            //get searched result if search is not empty
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                mysoldnotes = mysoldnotes.Where(x => x.Title.ToLower().Contains(search) ||
                                                     x.Category.ToLower().Contains(search) ||
                                                     x.Buyer.ToLower().Contains(search) ||
                                                     x.Price.ToString().ToLower().Contains(search) ||
                                                     x.SellType.ToLower().Contains(search)
                                               ).ToList();
            }

            //sort result
            mysoldnotes = SortTableMySoldNotes(sort, mysoldnotes);

            //count total pages
            ViewBag.TotalPages = Math.Ceiling(mysoldnotes.Count() / 10.0);

            //show result based on pagination
            mysoldnotes = mysoldnotes.Skip((page - 1) * 10).Take(10);

            return View(mysoldnotes);
        }

        //sorting for my my sold notes
        private IEnumerable<MySoldNotesViewModel> SortTableMySoldNotes(string sort, IEnumerable<MySoldNotesViewModel> table)
        {
            switch (sort)
            {
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.Title);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.Title);
                        break;
                    }
                case "Category_Asc":
                    {
                        table = table.OrderBy(x => x.Category);
                        break;
                    }
                case "Category_Desc":
                    {
                        table = table.OrderByDescending(x => x.Category);
                        break;
                    }
                case "Buyer_Asc":
                    {
                        table = table.OrderBy(x => x.Buyer);
                        break;
                    }
                case "Buyer_Desc":
                    {
                        table = table.OrderByDescending(x => x.Buyer);
                        break;
                    }
                case "Type_Asc":
                    {
                        table = table.OrderBy(x => x.SellType);
                        break;
                    }
                case "Type_Desc":
                    {
                        table = table.OrderByDescending(x => x.SellType);
                        break;
                    }
                case "Price_Asc":
                    {
                        table = table.OrderBy(x => x.Price);
                        break;
                    }
                case "Price_Desc":
                    {
                        table = table.OrderByDescending(x => x.Price);
                        break;
                    }
                case "DownloadedDate_Asc":
                    {
                        table = table.OrderBy(x => x.DownloadedDate);
                        break;
                    }
                case "DownloadedDate_Desc":
                    {
                        table = table.OrderByDescending(x => x.DownloadedDate);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DownloadedDate);
                        break;
                    }
            }
            return table;
        }

    }
}