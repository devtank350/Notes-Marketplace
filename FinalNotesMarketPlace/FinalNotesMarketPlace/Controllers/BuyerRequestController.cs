using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    [OutputCache(Duration = 0)]
    public class BuyerRequestController : Controller
    {
        Notes_MarketPlaceEntities context = new Notes_MarketPlaceEntities();

        [Authorize(Roles = "Member")]
        [Route("BuyerRequest")]
        public ActionResult BuyerRequest(string search, string sort, int page = 1)
        {
            // viewbag for active class in navigation
            ViewBag.BuyerRequest = "active";

            // viewbag for sorting, searching and pagination
            ViewBag.Sort = sort;
            ViewBag.Search = search;
            ViewBag.PageNumber = page;

            //get logged in user
            Users user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();

            // get buyer requests
            IEnumerable<BuyerRequestViewModel> buyerrequest = from download in context.Downloads
                                                              join users in context.Users on download.Downloader equals users.ID
                                                              join userprofile in context.UserProfile on download.Downloader equals userprofile.UserID
                                                              where download.Seller == user.ID && download.IsSellerHasAllowedDownload == false && download.AttachmentPath == null
                                                              select new BuyerRequestViewModel
                                                              {
                                                                  NoteID = download.NoteID,
                                                                  DownloadID = download.ID,
                                                                  Title = download.NoteTitle,
                                                                  Category = download.NoteCategory,
                                                                  Buyer = users.EmailID,
                                                                  PhoneNo = userprofile.PhoneNumberCountryCode + " " + userprofile.PhoneNumber,
                                                                  SellType = download.IsPaid == true ? "Paid" : "Free",
                                                                  Price = download.PurchasedPrice,
                                                                  DownloadedDate = download.CreatedDate.Value
                                                              };

            // if search is not empty
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                buyerrequest = buyerrequest.Where(
                                                    x => x.Title.ToLower().Contains(search) ||
                                                         x.Category.ToLower().Contains(search) ||
                                                         x.Buyer.ToLower().Contains(search) ||
                                                         x.Price.ToString().ToLower().Contains(search) ||
                                                         x.PhoneNo.ToLower().Contains(search) ||
                                                         x.SellType.ToLower().Contains(search)
                                                 ).ToList();
            }

            // sort results
            buyerrequest = SortTableBuyerRequest(sort, buyerrequest);
            // get total pages
            ViewBag.TotalPages = Math.Ceiling(buyerrequest.Count() / 10.0);
            // get result according to pagination
            buyerrequest = buyerrequest.Skip((page - 1) * 10).Take(10);

            return View(buyerrequest);
        }

        private IEnumerable<BuyerRequestViewModel> SortTableBuyerRequest(string sort, IEnumerable<BuyerRequestViewModel> table)
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
                case "Phone_Asc":
                    {
                        table = table.OrderBy(x => x.PhoneNo);
                        break;
                    }
                case "Phone_Desc":
                    {
                        table = table.OrderByDescending(x => x.PhoneNo);
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

        [Authorize(Roles = "Member")]
        [Route("BuyerRequest/AllowDownload/{id}")]
        public ActionResult AllowDownload(int id)
        {
            // get logged in user
            Users user = context.Users.Where(x => x.EmailID == User.Identity.Name).FirstOrDefault();
            // get download object by id
            Downloads download = context.Downloads.Find(id);
            // check if logged in user and note seller is same or not
            if (user.ID == download.Seller)
            {
                // get sellernoteattachement object
                SellerNotesAttachments attachement = context.SellerNotesAttachments.Where(x => x.NoteID == download.NoteID && x.IsActive == true).FirstOrDefault();

                // update data in download table
                context.Downloads.Attach(download);
                download.AttachmentDownloadedDate = DateTime.Now;
                download.IsSellerHasAllowedDownload = true;
                download.AttachmentPath = attachement.FilePath;
                download.NodifiedBy = user.ID;
                download.ModifiedDate = DateTime.Now;
                context.SaveChanges();

                // send mail
                AllowDownloadTemplate(download, user);

                return RedirectToAction("BuyerRequest");

            }
            else
            {
                return RedirectToAction("BuyerRequest");

            }
        }

        public void AllowDownloadTemplate(Downloads download, Users seller)
        {
            // get text from allowdownload template from emailtemplate directory
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/EmailTemplate/") + "SellerAllowDownloadTemplate" + ".cshtml");
            // get downloader user object
            var downloader = context.Users.Where(x => x.ID == download.Downloader).FirstOrDefault();
            // replace seller and buyer name
            body = body.Replace("ViewBag.SellerName", seller.FirstName);
            body = body.Replace("ViewBag.BuyerName", downloader.FirstName);
            body = body.ToString();

            // get support email
            var fromemail = context.SystemConfiguration.Where(x => x.Key == "supportemail").FirstOrDefault();

            // set from, to, subject, body
            string from, to, subject;
            from = fromemail.Value.Trim();
            to = downloader.EmailID.Trim();
            subject = seller.FirstName + " Allows you to download a note";
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

    }
}