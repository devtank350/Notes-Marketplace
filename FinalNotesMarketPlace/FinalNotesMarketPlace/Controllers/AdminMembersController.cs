using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FinalNotesMarketPlace.Models;
using System.Web.Mvc;
using FinalNotesMarketPlace;

namespace NotesMarketplace.Controllers
{
    [OutputCache(Duration = 0)]
    [RoutePrefix("Admin")]
    public class AdminMembersController : Controller
    {
        Notes_MarketPlaceEntities context = new Notes_MarketPlaceEntities();

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Member")]
        public ActionResult Members(string search, string sort, int page = 1)
        {
            // viewbag for searching, sorting and page
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;

            // get id of user role member
            var userrolememberid = context.UserRoles.Where(x => x.Name.ToLower() == "member").Select(x => x.ID).FirstOrDefault();

            // get id for note status submitted for review, in review, published
            var submittedforreviewid = context.ReferenceData.Where(x => x.Value.ToLower() == "submitted for review").Select(x => x.ID).FirstOrDefault();
            var inreviewid = context.ReferenceData.Where(x => x.Value.ToLower() == "in review").Select(x => x.ID).FirstOrDefault();
            var publishednoteid = context.ReferenceData.Where(x => x.Value.ToLower() == "published").Select(x => x.ID).FirstOrDefault();

            // get member list for member table
            IEnumerable<MembersViewModel> memberlist = from member in context.Users
                                                       where member.IsActive == true && member.IsEmailVerified == true && member.RoleID == userrolememberid
                                                       select new MembersViewModel
                                                       {
                                                           ID = member.ID,
                                                           FirstName = member.FirstName,
                                                           LastName = member.LastName,
                                                           Email = member.EmailID,
                                                           JoiningDate = member.CreatedDate.Value,
                                                           UnderReviewNotes = context.SellerNotes.Where(x => x.SellerID == member.ID && (x.Status == submittedforreviewid || x.Status == inreviewid)).Count(),
                                                           PublishedNotes = context.SellerNotes.Where(x => x.SellerID == member.ID && x.Status == publishednoteid).Count(),
                                                           DownloadedNotes = context.Downloads.Where(x => x.Downloader == member.ID && x.IsSellerHasAllowedDownload == true).Count(),
                                                           TotalExpenses = context.Downloads.Where(x => x.Downloader == member.ID && x.IsSellerHasAllowedDownload == true).Select(x => x.PurchasedPrice).Sum(),
                                                           TotalEarning = context.Downloads.Where(x => x.Seller == member.ID && x.IsSellerHasAllowedDownload == true).Select(x => x.PurchasedPrice).Sum()
                                                       };

            // search form result
            if (!String.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                memberlist = memberlist.Where(x => x.FirstName.ToLower().Contains(search) ||
                                                   x.LastName.ToLower().Contains(search) ||
                                                   x.Email.ToLower().Contains(search) ||
                                                   x.JoiningDate.ToString("dd-MM-yyyy, hh:mm").Contains(search) ||
                                                   x.TotalExpenses.ToString().Contains(search) ||
                                                   x.TotalEarning.ToString().Contains(search)).ToList();
            }

            // sorting result
            memberlist = SortTableMembers(sort, memberlist);

            // get total pages
            ViewBag.TotalPages = Math.Ceiling(memberlist.Count() / 5.0);

            // show result according to pagination
            memberlist = memberlist.Skip((page - 1) * 5).Take(5);

            return View(memberlist);
        }

        // sorting for Member Table
        private IEnumerable<MembersViewModel> SortTableMembers(string sort, IEnumerable<MembersViewModel> table)
        {
            switch (sort)
            {
                case "FirstName_Asc":
                    {
                        table = table.OrderBy(x => x.FirstName);
                        break;
                    }
                case "FirstName_Desc":
                    {
                        table = table.OrderByDescending(x => x.FirstName);
                        break;
                    }
                case "LastName_Asc":
                    {
                        table = table.OrderBy(x => x.LastName);
                        break;
                    }
                case "LastName_Desc":
                    {
                        table = table.OrderByDescending(x => x.LastName);
                        break;
                    }
                case "Email_Asc":
                    {
                        table = table.OrderBy(x => x.Email);
                        break;
                    }
                case "Email_Desc":
                    {
                        table = table.OrderByDescending(x => x.Email);
                        break;
                    }
                case "JoiningDate_Asc":
                    {
                        table = table.OrderBy(x => x.JoiningDate);
                        break;
                    }
                case "JoiningDate_Desc":
                    {
                        table = table.OrderByDescending(x => x.JoiningDate);
                        break;
                    }
                case "UnderReviewNotes_Asc":
                    {
                        table = table.OrderBy(x => x.UnderReviewNotes);
                        break;
                    }
                case "UnderReviewNotes_Desc":
                    {
                        table = table.OrderByDescending(x => x.UnderReviewNotes);
                        break;
                    }
                case "PublishedNotes_Asc":
                    {
                        table = table.OrderBy(x => x.PublishedNotes);
                        break;
                    }
                case "PublishedNotes_Desc":
                    {
                        table = table.OrderByDescending(x => x.PublishedNotes);
                        break;
                    }
                case "DownloadedNotes_Asc":
                    {
                        table = table.OrderBy(x => x.DownloadedNotes);
                        break;
                    }
                case "DownloadedNotes_Desc":
                    {
                        table = table.OrderByDescending(x => x.DownloadedNotes);
                        break;
                    }
                case "TotalExpenses_Asc":
                    {
                        table = table.OrderBy(x => x.TotalExpenses);
                        break;
                    }
                case "TotalExpenses_Desc":
                    {
                        table = table.OrderByDescending(x => x.TotalExpenses);
                        break;
                    }
                case "TotalEarning_Asc":
                    {
                        table = table.OrderBy(x => x.TotalEarning);
                        break;
                    }
                case "TotalEarning_Desc":
                    {
                        table = table.OrderByDescending(x => x.TotalEarning);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.JoiningDate);
                        break;
                    }
            }
            return table;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Member/{member}")]
        public ActionResult MemberDetail(int member, string sort, int page = 1)
        {
            // viewbag for searching, sorting and pagination
            ViewBag.Sort = sort;
            ViewBag.PageNumber = page;
            ViewBag.Member = member;

            // get id of note status draft
            var draftid = context.ReferenceData.Where(x => x.Value.ToLower() == "draft").Select(x => x.ID).FirstOrDefault();

            // get member
            Users users = context.Users.Where(x => x.ID == member).FirstOrDefault();
            // get member's profile
            UserProfile userprofiles = context.UserProfile.Where(x => x.UserID == users.ID).FirstOrDefault();
            // get member's notes excluding note status draft
            var notes = context.SellerNotes.Where(x => x.SellerID == users.ID && x.Status != draftid).ToList();

            // create list of MemberDetailViewModel.MembersNote
            var notelist = new List<MemberDetailViewModel.MembersNote>();

            foreach (var note in notes)
            {
                // get data of downloaded notes for count how many people downloaded notes and get total earning
                var downloadednoteslist = context.Downloads.Where(x => x.NoteID == note.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null);

                // create membernote object of MemberDetailViewModel.MembersNote
                var membernote = new MemberDetailViewModel.MembersNote();
                membernote.ID = note.ID;
                membernote.Title = note.Title;
                membernote.Category = note.NoteCategories.Name;
                membernote.Status = note.ReferenceData.Value;
                membernote.DownloadedNotes = downloadednoteslist.Count();
                membernote.TotalEarning = downloadednoteslist.Select(x => x.PurchasedPrice).Sum();
                membernote.DateAdded = note.CreatedDate;
                membernote.PublishedDate = note.PublishedDate;

                // add membernote object to notelist
                notelist.Add(membernote);
            }

            // create object of MemberDetailViewModel
            MemberDetailViewModel members = new MemberDetailViewModel();
            members.FirstName = users.FirstName;
            members.LastName = users.LastName;
            members.Email = users.EmailID;
            if (userprofiles != null)
            {
                members.DOB = userprofiles.DOB;
                members.PhoneNumberCountryCode = userprofiles.PhoneNumberCountryCode;
                members.PhoneNumber = userprofiles.PhoneNumber;
                members.College = userprofiles.University;
                members.Address1 = userprofiles.AddressLine1;
                members.Address2 = userprofiles.AddressLine2;
                members.City = userprofiles.City;
                members.State = userprofiles.State;
                members.ZipCode = userprofiles.ZipCode;
                members.Country = userprofiles.Country;
                members.ProfilePicture = userprofiles.ProfilePicture;
            }
            members.Notes = notelist.AsEnumerable();

            // sorting member notes result
            members.Notes = SortTableMemberNotes(sort, members.Notes);

            // count total pages
            ViewBag.TotalPages = Math.Ceiling(members.Notes.Count() / 5.0);

            // show result according to pagination
            members.Notes = members.Notes.Skip((page - 1) * 5).Take(5);

            return View(members);
        }

        // sorting for Member Detail Table
        private IEnumerable<MemberDetailViewModel.MembersNote> SortTableMemberNotes(string sort, IEnumerable<MemberDetailViewModel.MembersNote> table)
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
                case "Status_Asc":
                    {
                        table = table.OrderBy(x => x.Status);
                        break;
                    }
                case "Status_Desc":
                    {
                        table = table.OrderByDescending(x => x.Status);
                        break;
                    }
                case "DownloadedNotes_Asc":
                    {
                        table = table.OrderBy(x => x.DownloadedNotes);
                        break;
                    }
                case "DownloadedNotes_Desc":
                    {
                        table = table.OrderByDescending(x => x.DownloadedNotes);
                        break;
                    }
                case "TotalEarning_Asc":
                    {
                        table = table.OrderBy(x => x.TotalEarning);
                        break;
                    }
                case "TotalEarning_Desc":
                    {
                        table = table.OrderByDescending(x => x.TotalEarning);
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
                case "PublishedDate_Asc":
                    {
                        table = table.OrderBy(x => x.PublishedDate);
                        break;
                    }
                case "PublishedDate_Desc":
                    {
                        table = table.OrderByDescending(x => x.PublishedDate);
                        break;
                    }
                default:
                    {
                        table = table.OrderBy(x => x.DateAdded);
                        break;
                    }
            }
            return table;
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [Route("Member/Deactive/{memberid}")]
        public ActionResult DeactiveMember(int memberid)
        {
            // get logged in admin
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).First();

            // get ids of note status removed and published
            var removedid = context.ReferenceData.Where(x => x.Value.ToLower() == "removed").Select(x => x.ID).FirstOrDefault();
            var publishedid = context.ReferenceData.Where(x => x.Value.ToLower() == "published").Select(x => x.ID).FirstOrDefault();

            // get member by member id
            var member = context.Users.Where(x => x.ID == memberid && x.IsActive == true).First();

            // make member inactive
            member.IsActive = false;
            member.ModifiedDate = DateTime.Now;
            member.ModifiedBy = user.ID;

            // save updated member record
            context.Entry(member).State = EntityState.Modified;
            context.SaveChanges();

            // get member's published notes list
            var notelist = context.SellerNotes.Where(x => x.SellerID == member.ID && x.Status == publishedid && x.IsActive == true).ToList();

            // make member's each published note status removed
            foreach (var note in notelist)
            {
                note.Status = removedid;
                note.ModifiedDate = DateTime.Now;
                note.ModifiedBy = user.ID;

                context.Entry(note).State = EntityState.Modified;
                context.SaveChanges();
            }

            return RedirectToAction("Members", "AdminMembers");
        }
    }
}