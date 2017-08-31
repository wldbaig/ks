using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace KitShoesUpgrade.Managers
{
    public class UsersManager : BaseManager
    {
        public IEnumerable<UserCredential> GetUsers()
        {

            var query = db.UserCredentials.OrderBy(u => u.ID);

            return query;
        }

        public bool AddUser(UserViewModel model, int userID)
        {
            var checkLoginIDs = db.UserCredentials.Where(u => u.LoginID == model.userCredentials.LoginID.Trim()).Count();

            if (checkLoginIDs >= 1)
            {
                throw new KSException(eErrorCode.DuplicateLoginID);
            }


            using (var trans = new System.Transactions.TransactionScope())
            {
                model.userCredentials.Password = Encrypt(model.PASSWORD);

                model.userCredentials.CreatedOn = System.DateTime.UtcNow;
                model.userCredentials.CreatedBy = userID;
                model.userCredentials.UpdatedBy = userID;
                model.userCredentials.UserSince = DateTime.UtcNow;

                db.UserCredentials.Add(model.userCredentials);
                db.SaveChanges();

                trans.Complete();
                return true;
            }
        }

        public bool EditUser(UserViewModel model, int userID)
        {

            using (var trans = new System.Transactions.TransactionScope())
            {
                var editedUser = db.UserCredentials.Find(model.userCredentials.ID);
                editedUser.Name = model.userCredentials.Name;
                editedUser.Phone = model.userCredentials.Phone;

                editedUser.Address = model.userCredentials.Address;
                editedUser.UpdatedBy = userID;
                editedUser.UpdatedOn = DateTime.UtcNow;
                //    editedUser.Password = Encrypt(model.PASSWORD);
                db.Entry(editedUser).State = EntityState.Modified;

                db.SaveChanges();


                trans.Complete();
                return true;
            }
        }

        #region Helpers
        public List<Role> AllowedRoles(CustomPrincipal User)
        {
            return db.Roles.OrderBy(c => c.ID).ToList();
        }

        #endregion
    }
}