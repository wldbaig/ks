using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KitShoesUpgrade.Managers
{
    public class CustomerManager : BaseManager
    {

        public void Add(Customer model, CustomPrincipal User)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                model.Name = model.Name.Trim();
                model.Phone = model.Phone.Trim();
                model.IsActive = true;
                model.CustomerType = "CREDIT";
                model.AddedBy = User.ID;
                model.CreatedOn = DateTime.UtcNow;
                db.Customers.Add(model);
                db.SaveChanges();

                var account = new CustomerAccount();
                account.CustomerID = model.ID;
                account.CreatedBy = User.ID;
                account.OutStandingAmount = 0;
                account.TotalPaid = 0;
                account.TotalBalance = 0;
                account.CreatedOn = DateTime.UtcNow;
                db.CustomerAccounts.Add(account);
                db.SaveChanges();

                trans.Complete();
            }
        }

        public void Update(Customer model, CustomPrincipal User)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                var cus = db.Customers.Find(model.ID);
                cus.Name = model.Name.Trim();
                cus.Phone = model.Phone.Trim();
                cus.Address = model.Address;
                cus.UpdatedOn = DateTime.UtcNow;
                cus.UpdatedBy = User.ID;
                cus.CustomerCategoryID = model.CustomerCategoryID;
                db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                trans.Complete();
            } 
        }

        public void AddAccount(CustomerAccount model, decimal amount, CustomPrincipal User)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                var acntdtl = new CustomerAccountDetail();

                acntdtl.CAccountID = db.CustomerAccounts.FirstOrDefault(m => m.CustomerID == model.ID).ID;
                acntdtl.CreatedBy = User.ID;
                acntdtl.TotalAmount = amount;
                acntdtl.Description = (amount < 0) ? "PAY BY SHOP" : "RECIEVE FROM CUSTOMER";
                acntdtl.CreatedOn = DateTime.UtcNow;
                db.CustomerAccountDetails.Add(acntdtl);
                db.SaveChanges();

                var cusAccnt = db.CustomerAccounts.FirstOrDefault(m => m.CustomerID == model.ID);

                cusAccnt.UpdatedOn = DateTime.UtcNow;
                cusAccnt.UpdatedBy = User.ID;
                cusAccnt.TotalPaid = cusAccnt.CustomerAccountDetails.Sum(c => c.TotalAmount);
                cusAccnt.OutStandingAmount = cusAccnt.TotalBalance - cusAccnt.TotalPaid;

                db.Entry(cusAccnt).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();

                trans.Complete();
            }
        }

    }
}