
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyApp.Models;
using myApp.Db.DbOperations;
using Notes_Market_Place_D;
using myApp.Db;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;



namespace MVCwithDb.Controllers

{
    public class HomeController : Controller
    {
        Notes_MarketPlaceEntities context = new Notes_MarketPlaceEntities();
        
        //[Route("Home/SignUp")]
        // GET: Home
        public ActionResult SignUp()
        {
            return View();
        }

        //public bool IsEmailExist(String Email)
        //{
        //    using (var repository = new userrepository())
        //    {
        //        var v = repository.
        //        }
        //}
        [HttpPost]
        public ActionResult SignUp(usermodel model)
        {


            if (ModelState.IsValid)
            {

                bool emailexists = context.Users.Any(x => x.EmailID == model.EmailID);
                if (emailexists)
                {
                    ModelState.AddModelError("EmailID", "Email already exists");
                    return View(model);
                }
                else
                {
                    Users u = new Users
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        EmailID = model.EmailID,
                        Password = model.Password,
                        CreatedDate = DateTime.Now,
                        RoleID = 3,
                    };

                    context.Users.Add(u);
                    context.SaveChanges();
                    string d = u.CreatedDate.Value.ToString("ddMMyyyy");

                    EmailSend(u, d);
                    ModelState.Clear();
                    return View();
                }
            }
            else
            {
                return View(model);
            }        
            }
        public void EmailSend(Users u,String d)
        {
            string body = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Home/")+"EmailVerfication"+".cshtml");

            var url = "https://localhost:44381/" + "Home/VerifyEmail?key=" + u.ID + "&value" + d;

            //var fromemail = context.SystemConfiguration.Where(x => x.);

            string to, from, subject;
            from = "Dev Tank";
            to = u.EmailID.Trim();
            subject = "Email Verification";
            StringBuilder sb = new StringBuilder();
            sb.Append(body);
            body = sb.ToString();

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(from, "Dev");
            mail.To.Add(new MailAddress(to));
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            gmail.gEmail(mail);
        }
        public ActionResult Verify(int key,string val)
        {
            ViewBag.Key = key;
            ViewBag.Value = ValueProvider;
            return View();
        }
        public ActionResult IsConfirmed(int key,string val)
        {
            Users u = context.Users.Where(x => x.ID == key).FirstOrDefault();
            if (u == null)
            {
                return HttpNotFound();
            }
            if (String.Equals(u.CreatedDate.Value.ToString("ddMMyyyy"),val))
            {
                u.IsEmailVerified = true;
                u.ModifiedDate = DateTime.Now;
                context.SaveChanges();
            }
            return RedirectToAction("Login", "Home");
        }

        [Route("Login")]
        public ActionResult Login()
        {
            return View();
        }

        
        [HttpPost]
        public ActionResult Login(Loginusermodel loginuser)
        {
            if (ModelState.IsValid)
            {
                var user = context.Users.FirstOrDefault(x => x.EmailID == loginuser.EmailID);
                if (user != null)
                {
                    if (user.IsEmailVerified)
                    {
                        if (user.Password == loginuser.Password)
                        {
                            if (user.RoleID == 3)
                            {
                                FormsAuthentication.SetAuthCookie(user.EmailID, loginuser.RememberMe);
                                var profiledone = context.UserProfile.Where(x => x.UserID == user.ID).FirstOrDefault();
                                if (profiledone == null)
                                {
                                    return RedirectToAction("UserProfile", "User");
                                }
                                else
                                {
                                    return RedirectToAction("Search", "SearchNotes");
                                }
                            }
                            else
                            {
                                FormsAuthentication.SetAuthCookie(user.EmailID, loginuser.RememberMe);
                                return RedirectToAction("Dashboard", "Admin");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("Password", "Incorrect password");
                            return View(loginuser);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("EmailID", "Verify Please");
                        return View(loginuser);
                    }
                }
                else
                {
                    ModelState.AddModelError("EmailID", "Incorrect EmailID");
                    return View(loginuser);
                }
            }
            else
            {
                return View(loginuser);
            }

                              
                            
                        }
                    }
                }
            }
        }




    }
}
