using FinalNotesMarketPlace.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace FinalNotesMarketPlace.Controllers
{
    [OutputCache(Duration = 0)]
    [RoutePrefix("Admin")]
    public class AdminProfileController : Controller
    {
        Notes_MarketPlaceEntities context = new Notes_MarketPlaceEntities();

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("Admin/Profile")]
        public ActionResult MyProfile()
        {
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get logged in user profile
            var userprofile = context.UserProfile.Where(x => x.UserID == user.ID).FirstOrDefault();

            // create AdminProfileViewModel
            AdminProfileViewModel adminProfileViewModel = new AdminProfileViewModel();
            adminProfileViewModel.FirstName = user.FirstName;
            adminProfileViewModel.LastName = user.LastName;
            adminProfileViewModel.Email = user.EmailID;
            adminProfileViewModel.PhoneNumberCountryCode = userprofile.PhoneNumberCountryCode;
            adminProfileViewModel.PhoneNumber = userprofile.PhoneNumber;

            if (userprofile.SecondaryEmailAddress != null)
            {
                adminProfileViewModel.SecondaryEmail = userprofile.SecondaryEmailAddress;
            }

            if (userprofile.ProfilePicture != null)
            {
                adminProfileViewModel.ProfilePictureUrl = userprofile.ProfilePicture;
            }

            // get country code list
            adminProfileViewModel.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();

            return View(adminProfileViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Admin/Profile")]
        public ActionResult MyProfile(AdminProfileViewModel obj)
        {
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get  logged in user profile
            var userprofile = context.UserProfile.Where(x => x.UserID == user.ID).FirstOrDefault();

            // check if secondary email is already exists in User or UserProfile table or not
            // if email already exists then give error
            bool secondaryemailalreadyexistsinusers = context.Users.Where(x => x.EmailID == obj.SecondaryEmail).Any();
            bool secondaryemailalreadyexistsinuserprofile = context.UserProfile.Where(x => x.SecondaryEmailAddress == obj.Email && x.UserID != user.ID).Any();
            if (secondaryemailalreadyexistsinusers || secondaryemailalreadyexistsinuserprofile)
            {
                ModelState.AddModelError("SecondaryEmail", "This email address is already exists");
                obj.CountryCodeList = context.Countries.Where(x => x.IsActive).OrderBy(x => x.CountryCode).Select(x => x.CountryCode).ToList();
                return View(obj);
            }

            // update user's data            
            user.FirstName = obj.FirstName.Trim();
            user.LastName = obj.LastName.Trim();

            // update userprofile's data
            userprofile.SecondaryEmailAddress = obj.SecondaryEmail.Trim();
            userprofile.PhoneNumberCountryCode = obj.PhoneNumberCountryCode.Trim();
            userprofile.PhoneNumber = obj.PhoneNumber.Trim();

            // user upploaded profile picture and there is also previous profile picture then delete previous profile picture
            if (userprofile.ProfilePicture != null && obj.ProfilePicture != null)
            {
                string path = Server.MapPath(userprofile.ProfilePicture);
                FileInfo file = new FileInfo(path);
                if (file.Exists)
                {
                    file.Delete();
                }
            }

            // save new profile picture and update data in userprofile table
            if (obj.ProfilePicture != null)
            {
                // get extension
                string fileextension = System.IO.Path.GetExtension(obj.ProfilePicture.FileName);
                // set new name of file
                string newfilename = "DP_" + DateTime.Now.ToString("ddMMyyyy_hhmmss") + fileextension;
                // set where to save picture
                string profilepicturepath = "~/Members/" + userprofile.UserID + "/";
                // create directory if not exists
                CreateDirectoryIfMissing(profilepicturepath);
                // get physical path and save profile picture there
                string path = Path.Combine(Server.MapPath(profilepicturepath), newfilename);
                obj.ProfilePicture.SaveAs(path);
                // save path in database
                userprofile.ProfilePicture = profilepicturepath + newfilename;
            }

            // update userprofile's data
            userprofile.ModifiedDate = DateTime.Now;
            userprofile.ModifiedBy = user.ID;

            // update data and save data in database
            context.Entry(user).State = EntityState.Modified;
            context.Entry(userprofile).State = EntityState.Modified;
            context.SaveChanges();

            return RedirectToAction("Dashboard", "Admin");
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

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("SpamReport")]
        public ActionResult SpamReport(string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get spam report data for showing in table
            IEnumerable<SpamReportsViewModel> reportlist = from spam in context.SellerNotesReportedIssues
                                                          join note in context.SellerNotes on spam.NoteID equals note.ID
                                                          join reportedby in context.Users on spam.ReportedByID equals reportedby.ID
                                                          select new SpamReportsViewModel
                                                          {
                                                              ID = spam.ID,
                                                              NoteID = note.ID,
                                                              ReportedBy = reportedby.FirstName + " " + reportedby.LastName,
                                                              NoteTitle = note.Title,
                                                              Category = note.NoteCategories.Name,
                                                              Remark = spam.Remarks,
                                                              DateAdded = spam.CreatedDate.Value
                                                          };

            // get search result
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                reportlist = reportlist.Where(x => x.ReportedBy.ToLower().Contains(search) ||
                                                   x.NoteTitle.ToLower().Contains(search) ||
                                                   x.Category.ToLower().Contains(search) ||
                                                   x.Remark.ToLower().Contains(search) ||
                                                   x.DateAdded.ToString("dd-MM-yyyy, hh:mm").Contains(search)).ToList();
            }

            // sort result
            reportlist = SortTableReportedIssue(sort, reportlist);

            // get total pages
            ViewBag.TotalPages = Math.Ceiling(reportlist.Count() / 5.0);

            // show data according to pagination
            reportlist = reportlist.Skip((page - 1) * 5).Take(5);

            return View(reportlist);
        }

        // sorting for spam report
        private IEnumerable<SpamReportsViewModel> SortTableReportedIssue(string sort, IEnumerable<SpamReportsViewModel> table)
        {
            switch (sort)
            {
                case "ReportedBy_Asc":
                    {
                        table = table.OrderBy(x => x.ReportedBy);
                        break;
                    }
                case "ReportedBy_Desc":
                    {
                        table = table.OrderByDescending(x => x.ReportedBy);
                        break;
                    }
                case "Title_Asc":
                    {
                        table = table.OrderBy(x => x.NoteTitle);
                        break;
                    }
                case "Title_Desc":
                    {
                        table = table.OrderByDescending(x => x.NoteTitle);
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
                case "Remark_Asc":
                    {
                        table = table.OrderBy(x => x.Remark);
                        break;
                    }
                case "Remark_Desc":
                    {
                        table = table.OrderByDescending(x => x.Remark);
                        break;
                    }
                case "DateAdded_Asc":
                    {
                        table = table.OrderBy(x => x.DateAdded);
                        break;
                    }
                case "DateAdded_Desc":
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.DateAdded);
                        break;
                    }
            }
            return table;
        }


        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("SpamReport/Delete/{id}")]
        public ActionResult DeleteSpamReport(int id)
        {
            // get spam report object by id
            var spamreport = context.SellerNotesReportedIssues.Where(x => x.ID == id).FirstOrDefault();

            // check if object is null or not
            if (spamreport == null)
            {
                return HttpNotFound();
            }

            // delete object
            context.SellerNotesReportedIssues.Remove(spamreport);
            context.SaveChanges();

            return RedirectToAction("SpamReport");
        }
    }
}