﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using FinalNotesMarketPlace.Models;
using FinalNotesMarketPlace.SendEmail;

namespace FinalNotesMarketPlace.Controllers
{
    public class DashBoardController : Controller
    {
        Notes_MarketPlaceEntities context = new Notes_MarketPlaceEntities();
        // GET: DashBoard

        [HttpGet]
        [Authorize(Roles = "Member")]
        [Route("SellYourNotes")]
        public ActionResult Dashboard(string search1, string search2, string sort1, string sort2, int page1 = 1, int page2 = 1)
        {
            // add active class to navigation bar for this page
            ViewBag.SellYourNotes = "active";

            // viewbag for searching, sorting and pagination
            ViewBag.Sort1 = sort1;
            ViewBag.Sort2 = sort2;
            ViewBag.Page1 = page1;
            ViewBag.Page2 = page2;
            ViewBag.Search1 = search1;
            ViewBag.Search2 = search2;

            // get id for note status submitted for review, in review , draft, rejected, published
            var submittedforreviewid = context.ReferenceData.Where(x => x.Value.ToLower() == "submitted for review").Select(x => x.ID).FirstOrDefault();
            var inreviewid = context.ReferenceData.Where(x => x.Value.ToLower() == "in review").Select(x => x.ID).FirstOrDefault();
            var draftid = context.ReferenceData.Where(x => x.Value.ToLower() == "draft").Select(x => x.ID).FirstOrDefault();
            var rejectedid = context.ReferenceData.Where(x => x.Value.ToLower() == "rejected").Select(x => x.ID).FirstOrDefault();
            var publishedid = context.ReferenceData.Where(x => x.Value.ToLower() == "published").Select(x => x.ID).FirstOrDefault();

            // create object of dashboardviewmodel
            DashboardViewModel dashboardviewmodel = new DashboardViewModel();

            // get logged in user
            Users user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get number of sold notes, money earned, my downloads, my rejected notes and buyer request
            dashboardviewmodel.NumberOfSoldNotes = context.Downloads.Where(x => x.Seller == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).Count();
            dashboardviewmodel.MoneyEarned = context.Downloads.Where(x => x.Seller == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).Select(x => x.PurchasedPrice).Sum();
            dashboardviewmodel.MyDownloads = context.Downloads.Where(x => x.Downloader == user.ID && x.IsSellerHasAllowedDownload == true && x.AttachmentPath != null).Count();
            dashboardviewmodel.MyRejectedNotes = context.SellerNotes.Where(x => x.SellerID == user.ID && x.Status == rejectedid && x.IsActive == true).Count();
            dashboardviewmodel.BuyerRequest = context.Downloads.Where(x => x.Seller == user.ID && x.IsSellerHasAllowedDownload == false && x.AttachmentPath == null).Count();

            // get inprogress notes
            dashboardviewmodel.InProgressNotes = from note in context.SellerNotes
                                                 where (note.Status == draftid || note.Status == submittedforreviewid || note.Status == inreviewid) && note.SellerID == user.ID
                                                 select new InProgressNote
                                                 {
                                                     NoteID = note.ID,
                                                     Title = note.Title,
                                                     Category = note.NoteCategories.Name,
                                                     Status = note.ReferenceData.Value,
                                                     AddedDate = note.CreatedDate.Value
                                                 };
            // if search1 is not empty then get search result in inprogressnote
            if (!string.IsNullOrEmpty(search1))
            {
                search1 = search1.ToLower();
                dashboardviewmodel.InProgressNotes = dashboardviewmodel.InProgressNotes.Where(x => x.Title.ToLower().Contains(search1) ||
                                                                                                   x.Category.ToLower().Contains(search1) ||
                                                                                                   x.Status.ToLower().Contains(search1)
                                                                                             ).ToList();
            }

            // get published notes
            dashboardviewmodel.PublishedNotes = from note in context.SellerNotes
                                                where note.Status == publishedid && note.SellerID == user.ID
                                                select new PublishedNote
                                                {
                                                    NoteID = note.ID,
                                                    Title = note.Title,
                                                    Category = note.NoteCategories.Name,
                                                    SellType = note.IsPaid == true ? "Paid" : "Free",
                                                    Price = note.SellingPrice,
                                                    PublishedDate = note.PublishedDate.Value
                                                };
            // if search2 is not empty get search result in publishednote 
            if (!string.IsNullOrEmpty(search2))
            {
                search2 = search2.ToLower();
                dashboardviewmodel.PublishedNotes = dashboardviewmodel.PublishedNotes.Where(x => x.Title.ToLower().Contains(search2) ||
                                                                                                  x.Category.ToLower().Contains(search2) ||
                                                                                                  x.SellType.ToLower().Contains(search2) ||
                                                                                                  x.Price.ToString().Contains(search2)
                                                                                            ).ToList();
            }

            // sorting table
            dashboardviewmodel.InProgressNotes = SortTableInProgressNote(sort1, dashboardviewmodel.InProgressNotes);
            dashboardviewmodel.PublishedNotes = SortTablePublishNote(sort2, dashboardviewmodel.PublishedNotes);

            // count total results
            ViewBag.TotalPagesInProgress = Math.Ceiling(dashboardviewmodel.InProgressNotes.Count() / 5.0);
            ViewBag.TotalPagesInPublished = Math.Ceiling(dashboardviewmodel.PublishedNotes.Count() / 5.0);

            // show results according to pagination
            dashboardviewmodel.InProgressNotes = dashboardviewmodel.InProgressNotes.Skip((page1 - 1) * 5).Take(5);
            dashboardviewmodel.PublishedNotes = dashboardviewmodel.PublishedNotes.Skip((page2 - 1) * 5).Take(5);

            return View(dashboardviewmodel);
        }

        // sorting for inprogress table
        private IEnumerable<InProgressNote> SortTableInProgressNote(string sort, IEnumerable<InProgressNote> table)
        {
            switch (sort)
            {
                case "CreatedDate_Asc":
                    {
                        table = table.OrderBy(x => x.AddedDate);
                        break;
                    }
                case "CreatedDate_Desc":
                    {
                        table = table.OrderByDescending(x => x.AddedDate);
                        break;
                    }
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
                default:
                    {
                        table = table.OrderByDescending(x => x.AddedDate);
                        break;
                    }
            }
            return table;
        }

        // sorting for published note table
        private IEnumerable<PublishedNote> SortTablePublishNote(string sort, IEnumerable<PublishedNote> table)
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
                case "IsPaid_Asc":
                    {
                        table = table.OrderBy(x => x.SellType);
                        break;
                    }
                case "IsPaid_Desc":
                    {
                        table = table.OrderByDescending(x => x.SellType);
                        break;
                    }
                case "SellingPrice_Asc":
                    {
                        table = table.OrderBy(x => x.Price);
                        break;
                    }
                case "SellingPrice_Desc":
                    {
                        table = table.OrderByDescending(x => x.Price);
                        break;
                    }
                default:
                    {
                        table = table.OrderByDescending(x => x.PublishedDate);
                        break;
                    }
            }
            return table;
        }

        [Authorize(Roles = "Member")]
        [Route("SellYourNotes/DeleteDraft/{id}")]
        public ActionResult DeleteDraft(int id)
        {
            // get notes using id
            SellerNotes note = context.SellerNotes.Where(x => x.ID == id && x.IsActive == true).FirstOrDefault();
            // if note is not found
            if (note == null)
            {
                return HttpNotFound();
            }
            // get attachement files using note id
            IEnumerable<SellerNotesAttachments> noteattachement = context.SellerNotesAttachments.Where(x => x.NoteID == id && x.IsActive == true).ToList();
            // if noteattachement count is 0
            if (noteattachement.Count() == 0)
            {
                return HttpNotFound();
            }
            // filepaths for note and note attachements
            string notefolderpath = Server.MapPath("~/Members/" + note.SellerID + "/" + note.ID);
            string noteattachmentfolderpath = Server.MapPath("~/Members/" + note.SellerID + "/" + note.ID + "/Attachements");

            // get directory 
            DirectoryInfo notefolder = new DirectoryInfo(notefolderpath);
            DirectoryInfo attachementnotefolder = new DirectoryInfo(noteattachmentfolderpath);
            // empty directory
            EmptyFolder(attachementnotefolder);
            EmptyFolder(notefolder);
            // delete directory
            Directory.Delete(notefolderpath);

            // remove note from database
            context.SellerNotes.Remove(note);

            // remove attachement from database
            foreach (var item in noteattachement)
            {
                SellerNotesAttachments attachement = context.SellerNotesAttachments.Where(x => x.ID == item.ID).FirstOrDefault();
                context.SellerNotesAttachments.Remove(attachement);
            }

            // save changes
            context.SaveChanges();

            return RedirectToAction("Dashboard");
        }




        [HttpGet]
        [Authorize(Roles = "Member")]
        [Route("SellYourNotes/AddNotes")]
        public ActionResult AddNotes()
        {
            // create add note viewmodel and set values in dropdown list
            AddNotesViewModel viewModel = new AddNotesViewModel
            {
                //NoteCategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList(),
                NoteCategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList(),
                NoteTypeList = context.NoteTypes.Where(x => x.IsActive == true).ToList(),
                CountryList = context.Countries.Where(x => x.IsActive == true).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        [Route("SellYourNotes/AddNotes")]
        public ActionResult AddNotes(AddNotesViewModel addnotesviewmodel)
        {
            // check if upload note is null or not
            if (addnotesviewmodel.UploadNotes[0] == null)
            {
                ModelState.AddModelError("UploadNotes", "This field is required");
                addnotesviewmodel.NoteCategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList();
                addnotesviewmodel.NoteTypeList = context.NoteTypes.Where(x => x.IsActive == true).ToList();
                addnotesviewmodel.CountryList = context.Countries.Where(x => x.IsActive == true).ToList();
                return View(addnotesviewmodel);
            }
            // check and raise error for note preview is null for paid notes
            if (addnotesviewmodel.IsPaid == true && addnotesviewmodel.NotesPreview == null)
            {
                ModelState.AddModelError("NotesPreview", "This field is required if selling type is paid");
                addnotesviewmodel.NoteCategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList();
                addnotesviewmodel.NoteTypeList = context.NoteTypes.Where(x => x.IsActive == true).ToList();
                addnotesviewmodel.CountryList = context.Countries.Where(x => x.IsActive == true).ToList();
                return View(addnotesviewmodel);
            }

            foreach (HttpPostedFileBase file in addnotesviewmodel.UploadNotes)
            {
                if (!System.IO.Path.GetExtension(file.FileName).Equals(".pdf"))
                {
                    ModelState.AddModelError("UploadNotes", "Only PDF Format is allowed");
                    addnotesviewmodel.NoteCategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList();
                    addnotesviewmodel.NoteTypeList = context.NoteTypes.Where(x => x.IsActive == true).ToList();
                    addnotesviewmodel.CountryList = context.Countries.Where(x => x.IsActive == true).ToList();
                    return View(addnotesviewmodel);
                }
            }


            // check model state
            if (ModelState.IsValid)
            {
                // create seller note object
                SellerNotes sellernotes = new SellerNotes();

                Users user = context.Users.FirstOrDefault(x => x.EmailID == User.Identity.Name);

                sellernotes.SellerID = user.ID;
                sellernotes.Title = addnotesviewmodel.Title.Trim();
                sellernotes.Status = context.ReferenceData.Where(x => x.Value.ToLower() == "draft").Select(x => x.ID).FirstOrDefault();
                sellernotes.Category = addnotesviewmodel.Category;
                sellernotes.NoteType = addnotesviewmodel.NoteType;
                sellernotes.NumberOfPages = addnotesviewmodel.NumberofPages;
                sellernotes.Description = addnotesviewmodel.Description.Trim();
                sellernotes.UniversityName = addnotesviewmodel.UniversityName.Trim();
                sellernotes.Country = addnotesviewmodel.Country;
                sellernotes.Course = addnotesviewmodel.Course.Trim();
                sellernotes.CourseCode = addnotesviewmodel.CourseCode.Trim();
                sellernotes.Professor = addnotesviewmodel.Professor.Trim();
                sellernotes.IsPaid = addnotesviewmodel.IsPaid;
                if (sellernotes.IsPaid)
                {
                    sellernotes.SellingPrice = addnotesviewmodel.SellingPrice;
                }
                else
                {
                    sellernotes.SellingPrice = 0;
                }
                sellernotes.CreatedDate = DateTime.Now;
                sellernotes.CreatedBy = user.ID;
                sellernotes.IsActive = true;

                // add note in database and save
                context.SellerNotes.Add(sellernotes);
                context.SaveChanges();

                // get seller note
                sellernotes = context.SellerNotes.Find(sellernotes.ID);

                // if display picture is not null then save picture into directory and directory path into database
                if (addnotesviewmodel.DisplayPicture != null)
                {
                    string displaypicturefilename = System.IO.Path.GetFileName(addnotesviewmodel.DisplayPicture.FileName);
                    string displaypicturepath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";
                    CreateDirectoryIfMissing(displaypicturepath);
                    string displaypicturefilepath = Path.Combine(Server.MapPath(displaypicturepath), displaypicturefilename);
                    sellernotes.DisplayPicture = displaypicturepath + displaypicturefilename;
                    addnotesviewmodel.DisplayPicture.SaveAs(displaypicturefilepath);
                }

                // if note preview is not null then save picture into directory and directory path into database
                if (addnotesviewmodel.NotesPreview != null)
                {
                    string notespreviewfilename = System.IO.Path.GetFileName(addnotesviewmodel.NotesPreview.FileName);
                    string notespreviewpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";
                    CreateDirectoryIfMissing(notespreviewpath);
                    string notespreviewfilepath = Path.Combine(Server.MapPath(notespreviewpath), notespreviewfilename);
                    sellernotes.NotesPreview = notespreviewpath + notespreviewfilename;
                    addnotesviewmodel.NotesPreview.SaveAs(notespreviewfilepath);
                }

                // update note preview path and display picture path and save changes
                context.SellerNotes.Attach(sellernotes);
                context.Entry(sellernotes).Property(x => x.DisplayPicture).IsModified = true;
                context.Entry(sellernotes).Property(x => x.NotesPreview).IsModified = true;
                context.SaveChanges();

                // attachement files
                foreach (HttpPostedFileBase file in addnotesviewmodel.UploadNotes)
                {
                    // check if file is null or not
                    if (file != null)
                    {
                        // save file in directory
                        string notesattachementfilename = System.IO.Path.GetFileName(file.FileName);
                        string notesattachementpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/Attachements/";
                        CreateDirectoryIfMissing(notesattachementpath);
                        string notesattachementfilepath = Path.Combine(Server.MapPath(notesattachementpath), notesattachementfilename);
                        file.SaveAs(notesattachementfilepath);

                        // create object of sellernotesattachement 
                        SellerNotesAttachments notesattachements = new SellerNotesAttachments
                        {
                            NoteID = sellernotes.ID,
                            FileName = notesattachementfilename,
                            FilePath = notesattachementpath,
                            CreatedDate = DateTime.Now,
                            CreatedBy = user.ID,
                            IsActive = true
                        };

                        // save seller notes attachement
                        context.SellerNotesAttachments.Add(notesattachements);
                        context.SaveChanges();
                    }
                }

                return RedirectToAction("Dashboard", "SellYourNotes");
            }
            // if model state is not valid
            else
            {
                // create object of addnotesviewmodel
                AddNotesViewModel viewModel = new AddNotesViewModel
                {
                    NoteCategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList(),
                    NoteTypeList = context.NoteTypes.Where(x => x.IsActive == true).ToList(),
                    CountryList = context.Countries.Where(x => x.IsActive == true).ToList()
                };

                return View(viewModel);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Member")]
        [Route("SellYourNotes/EditNotes/{id}")]
        public ActionResult EditNotes(int id)
        {
            // get logged in user
            Users user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get note
            SellerNotes note = context.SellerNotes.Where(x => x.ID == id && x.IsActive == true && x.SellerID == user.ID).FirstOrDefault();
            // get note attachement
            SellerNotesAttachments attachement = context.SellerNotesAttachments.Where(x => x.NoteID == id).FirstOrDefault();
            if (note != null)
            {
                // create object of edit note viewmodel
                EditNotesViewModel viewModel = new EditNotesViewModel
                {
                    ID = note.ID,
                    NoteID = note.ID,
                    Title = note.Title,
                    Category = note.Category,
                    Picture = note.DisplayPicture,
                    Note = attachement.FilePath,
                    NumberofPages = note.NumberOfPages,
                    Description = note.Description,
                    NoteType = note.NoteType,
                    UniversityName = note.UniversityName,
                    Course = note.Course,
                    CourseCode = note.CourseCode,
                    Country = note.Country,
                    Professor = note.Professor,
                    IsPaid = note.IsPaid,
                    SellingPrice = note.SellingPrice,
                    Preview = note.NotesPreview,
                    NoteCategoryList = context.NoteCategories.Where(x => x.IsActive == true).ToList(),
                    NoteTypeList = context.NoteTypes.Where(x => x.IsActive == true).ToList(),
                    CountryList = context.Countries.Where(x => x.IsActive == true).ToList()
                };

                // return viewmodel to edit notes page
                return View(viewModel);
            }
            else
            {
                // if note not found
                return HttpNotFound();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        [Route("SellYourNotes/EditNotes/{id}")]
        public ActionResult EditNotes(int id, EditNotesViewModel notes)
        {
            // check if model state is valid or not
            if (ModelState.IsValid)
            {
                // get logged in user
                var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();
                // get note 
                var sellernotes = context.SellerNotes.Where(x => x.ID == id && x.IsActive == true && x.SellerID == user.ID).FirstOrDefault();
                // if sellernote null
                if (sellernotes == null)
                {
                    return HttpNotFound();
                }
                // check if note is paid or preview is not null
                if (notes.IsPaid == true && notes.Preview == null && sellernotes.NotesPreview == null)
                {
                    ModelState.AddModelError("NotesPreview", "This field is required if selling type is paid");
                    return View(notes);
                }
                // get note attachement 
                var notesattachement = context.SellerNotesAttachments.Where(x => x.NoteID == notes.NoteID && x.IsActive == true).ToList();

                // attache note object and update
                context.SellerNotes.Attach(sellernotes);
                sellernotes.Title = notes.Title.Trim();
                sellernotes.Category = notes.Category;
                sellernotes.NoteType = notes.NoteType;
                sellernotes.NumberOfPages = notes.NumberofPages;
                sellernotes.Description = notes.Description.Trim();
                sellernotes.Country = notes.Country;
                sellernotes.UniversityName = notes.UniversityName.Trim();
                sellernotes.Course = notes.Course.Trim();
                sellernotes.CourseCode = notes.CourseCode.Trim();
                sellernotes.Professor = notes.Professor.Trim();
                if (notes.IsPaid == true)
                {
                    sellernotes.IsPaid = true;
                    sellernotes.SellingPrice = notes.SellingPrice;
                }
                else
                {
                    sellernotes.IsPaid = false;
                    sellernotes.SellingPrice = 0;
                }
                sellernotes.ModifiedDate = DateTime.Now;
                sellernotes.ModifiedBy = user.ID;
                context.SaveChanges();

                // if display picture is not null
                if (notes.DisplayPicture != null)
                {
                    // if note object has already previously uploaded picture then delete it
                    if (sellernotes.DisplayPicture != null)
                    {
                        string path = Server.MapPath(sellernotes.DisplayPicture);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    // save updated profile picture in directory and save directory path in database
                    string displaypicturefilename = System.IO.Path.GetFileName(notes.DisplayPicture.FileName);
                    string displaypicturepath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";
                    CreateDirectoryIfMissing(displaypicturepath);
                    string displaypicturefilepath = Path.Combine(Server.MapPath(displaypicturepath), displaypicturefilename);
                    sellernotes.DisplayPicture = displaypicturepath + displaypicturefilename;
                    notes.DisplayPicture.SaveAs(displaypicturefilepath);
                }

                // if note preview is not null
                if (notes.NotesPreview != null)
                {
                    // if note object has already previously uploaded note preview then delete it
                    if (sellernotes.NotesPreview != null)
                    {
                        string path = Server.MapPath(sellernotes.NotesPreview);
                        FileInfo file = new FileInfo(path);
                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    // save updated note preview in directory and save directory path in database
                    string notespreviewfilename = System.IO.Path.GetFileName(notes.NotesPreview.FileName);
                    string notespreviewpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/";
                    CreateDirectoryIfMissing(notespreviewpath);
                    string notespreviewfilepath = Path.Combine(Server.MapPath(notespreviewpath), notespreviewfilename);
                    sellernotes.NotesPreview = notespreviewpath + notespreviewfilename;
                    notes.NotesPreview.SaveAs(notespreviewfilepath);
                }

                // check if user upload notes or not
                if (notes.UploadNotes[0] != null)
                {
                    // if user upload notes then delete directory that have previously uploaded notes
                    string path = Server.MapPath(notesattachement[0].FilePath);
                    DirectoryInfo dir = new DirectoryInfo(path);
                    EmptyFolder(dir);

                    // remove previously uploaded attachement from database
                    foreach (var item in notesattachement)
                    {
                        SellerNotesAttachments attachement = context.SellerNotesAttachments.Where(x => x.ID == item.ID).FirstOrDefault();
                        context.SellerNotesAttachments.Remove(attachement);
                    }

                    // add newly uploaded attachement in database and save it in database
                    foreach (HttpPostedFileBase file in notes.UploadNotes)
                    {
                        // check if file is null or not
                        if (file != null)
                        {
                            // save file in directory
                            string notesattachementfilename = System.IO.Path.GetFileName(file.FileName);
                            string notesattachementpath = "~/Members/" + user.ID + "/" + sellernotes.ID + "/Attachements/";
                            CreateDirectoryIfMissing(notesattachementpath);
                            string notesattachementfilepath = Path.Combine(Server.MapPath(notesattachementpath), notesattachementfilename);
                            file.SaveAs(notesattachementfilepath);

                            // create object of sellernotesattachement 
                            SellerNotesAttachments notesattachements = new SellerNotesAttachments
                            {
                                NoteID = sellernotes.ID,
                                FileName = notesattachementfilename,
                                FilePath = notesattachementpath,
                                CreatedDate = DateTime.Now,
                                CreatedBy = user.ID,
                                IsActive = true
                            };

                            // save seller notes attachement
                            context.SellerNotesAttachments.Add(notesattachements);
                            context.SaveChanges();
                        }
                    }
                }

                return RedirectToAction("Dashboard", "SellYourNotes");
            }
            else
            {
                return RedirectToAction("EditNotes", new { id = notes.ID });
            }

        }

        [Authorize(Roles = "Member")]
        [Route("Notes/Publish")]
        public ActionResult PublishNote(int id)
        {
            // get note
            var note = context.SellerNotes.Find(id);
            // if note is not found
            if (note == null)
            {
                return HttpNotFound();
            }
            // get logged in user
            var user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // seller full name
            string sellername = user.FirstName + " " + user.LastName;

            if (user.ID == note.SellerID)
            {
                // update note status from draft to submitted for review
                context.SellerNotes.Attach(note);
                note.Status = context.ReferenceData.Where(x => x.Value.ToLower() == "submitted for review").Select(x => x.ID).FirstOrDefault(); ;
                note.ModifiedDate = DateTime.Now;
                note.ModifiedBy = user.ID;
                context.SaveChanges();

                // send mail to admin for publish note request
                PublishNote(note.Title, sellername);
            }

            return RedirectToAction("Dashboard");
        }

        // send mail to admin for publish note request
        public void PublishNote(string note, string seller)
        {
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "PublishNote" + ".cshtml");
            body = body.Replace("ViewBag.SellerName", seller);
            body = body.Replace("ViewBag.NoteTitle", note);
            body = body.ToString();

            // get support email
            // get support email
            //var fromemail = context.SystemConfiguration.Where(x => x.Key == "supportemail").FirstOrDefault();
            var fromemail = context.SystemConfiguration.Where(x => x.Key == "supportemail").FirstOrDefault();
            var tomail = context.SystemConfiguration.Where(x => x.Key == "notifyemail").FirstOrDefault();

            // set from, to, subject, body
            string from, to, subject;
            from = fromemail.Value.Trim();
            to = tomail.Value.Trim();
            subject = seller + " sent his note for review";
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

            // send mail (NotesMarketplace/SendMail/)
            SendingEmail.SendEmail(mail);
        }





































        // delete files from directory or empty directory
        private void EmptyFolder(DirectoryInfo directory)
        {
            // check if directory have files
            if (directory.GetFiles() != null)
            {
                // delete all files
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }
            }

            // check if directory have subdirectory
            if (directory.GetDirectories() != null)
            {
                // call emptyfolder and delete subdirectory
                foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                {
                    EmptyFolder(subdirectory);
                    subdirectory.Delete();
                }
            }

        }

        // create directory
        private void CreateDirectoryIfMissing(string folderpath)
        {
            // check if directory exists
            bool folderalreadyexists = Directory.Exists(Server.MapPath(folderpath));
            // if directory does not exists then create
            if (!folderalreadyexists)
                Directory.CreateDirectory(Server.MapPath(folderpath));
        }
    }
}