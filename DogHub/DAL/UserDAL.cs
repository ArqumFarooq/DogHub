using DogHub.HelpingClasses;
using DogHub.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DogHub.DAL
{
    public class UserDAL
    {
        public List<User> GetAllUsers(DogHubEntities db)
        {
            return db.Users.ToList();
        }

        public List<User> GetActiveUsers(DogHubEntities db)
        {
            return db.Users.Where(x => x.IsActive == true).ToList();
        }

        public User GetUserById(int id, DogHubEntities db)
        {
            return db.Users.FirstOrDefault(x => x.PK_UserId == id);
        }

        public User GetUserByEmail(string email, DogHubEntities db)
        {
            return db.Users.FirstOrDefault(x => x.Email == email && x.IsActive == true);
        }

        public bool AddUser(User user, DogHubEntities db)
        {
            try
            {
                db.Users.Add(user);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateUser(User user, DogHubEntities db)
        {
            try
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Seed_AdminUser
        public bool Seed_AddAdminUser(DogHubEntities db)
        {
            if (db.Users.Any(u => u.IsAdmin == true)) return false;

            var encryptedPassword = StringCipher.Encrypt("123"); //  Use encryption

            var admin = new User
            {
                UserName = "Admin",
                Password = encryptedPassword,
                Email = "imarqum@gmail.com",
                IsAdmin = true,
                SysCreatedDate = DateTime.Now,
                IsActive = true
            };

            return AddUser(admin, db);
        }
        #endregion

    }
}