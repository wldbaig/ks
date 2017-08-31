using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KitShoesUpgrade.Managers
{
    public class BuyerManager : BaseManager
    {

        public void Add(Buyer model, CustomPrincipal User)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                model.Name = model.Name.Trim();
                model.Phone = model.Phone.Trim();
                model.IsActive = true;
              
                model.AddedBy = User.ID;
                model.CreatedOn = DateTime.UtcNow;
                db.Buyers.Add(model);
                db.SaveChanges();

                var account = new BuyerAccount();
                account.BuyerID = model.ID;
                account.CreatedBy = User.ID;
                account.OutStandingAmount = 0;
                account.TotalPaid = 0;
                account.TotalBalance = 0;
                account.CreatedOn = DateTime.UtcNow;
                db.BuyerAccounts.Add(account);
                db.SaveChanges();

                trans.Complete();
            }
        }

        public void Update(Buyer model, CustomPrincipal User)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                var buy = db.Buyers.Find(model.ID);
                buy.Name = model.Name.Trim();
                buy.Phone = model.Phone.Trim();
                buy.Address = model.Address;
                buy.UpdatedOn = DateTime.UtcNow;
                buy.UpdatedBy = User.ID;                      
                db.Entry(buy).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                trans.Complete();
            }
        }

        public void AddAccount(BuyerAccount model, decimal amount, CustomPrincipal User)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                var acntdtl = new BuyerAccountDetail();

                acntdtl.BAccountID = db.BuyerAccounts.FirstOrDefault(c=>c.BuyerID == model.BuyerID).ID;
                acntdtl.CreatedBy = User.ID;
                acntdtl.TotalAmount = amount;
                acntdtl.CreatedOn = DateTime.UtcNow;
                db.BuyerAccountDetails.Add(acntdtl);
                db.SaveChanges();

                var buyAccnt = db.BuyerAccounts.FirstOrDefault(c => c.BuyerID == model.BuyerID);

                buyAccnt.UpdatedOn = DateTime.UtcNow;
                buyAccnt.UpdatedBy = User.ID;
                buyAccnt.TotalPaid = buyAccnt.BuyerAccountDetails.Sum(c => c.TotalAmount);
                buyAccnt.OutStandingAmount = buyAccnt.TotalBalance - buyAccnt.TotalPaid; 

                db.Entry(buyAccnt).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                trans.Complete();
            }
        }

    }
}