using DogHub.DAL;
using DogHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DogHub.BL
{
    public class UserBL
    {

        public List<User> GetActiveUsers(DogHubEntities db)
        {
            return new UserDAL().GetActiveUsers(db);
        }

        public List<User> GetAllUsers(DogHubEntities db)
        {
            return new UserDAL().GetAllUsers(db);
        }

        public User GetUserById(int id, DogHubEntities db)
        {
            return new UserDAL().GetUserById(id, db);
        }

        public User GetUserByEmail(string email, DogHubEntities db)
        {
            return new UserDAL().GetUserByEmail(email, db);
        }

        public bool AddUser(User user, DogHubEntities db)
        {
            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(user.Email))
            {
                return false;
            }

            return new UserDAL().AddUser(user, db);
        }

        public bool UpdateUser(User user, DogHubEntities db)
        {
            return new UserDAL().UpdateUser(user, db);
        }

        #region Seed_AdminUser
        public bool Seed_AdminUser(DogHubEntities db)
        {
            return new UserDAL().Seed_AddAdminUser(db);
        }
        #endregion
    }
}