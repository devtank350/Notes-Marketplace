using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Models;

namespace myApp.Db.DbOperations
{
    public class userrepository
    {
        public int addUser(usermodel model)
        {
            using (var context = new Notes_MarketPlaceEntities())
            {
                Users emp = new Users()
                {   RoleID = 3,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Password = model.Password,
                    EmailID = model.EmailID
                };
                //context.Users.Add(emp);
                //context.SaveChanges();
                //return emp.ID;
                
                context.Users.Add(emp);
                context.SaveChanges();
                return emp.ID;
            }
        }
       

    }
}


