using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KitShoesUpgrade.Managers
{
    public class CategoryManager : BaseManager
    {
        public IEnumerable<Category> All()
        {
            db.Configuration.ProxyCreationEnabled = false;

            return db.Categories.ToList();

        }

        public Category One(Func<Category, bool> p)
        {
            return db.Categories.Where(p).FirstOrDefault();
        }

        public void Insert(Category category)
        {
            category.CategoryName = category.CategoryName.Trim();

            db.Categories.Add(category);
            db.SaveChanges();
        }

        public void Update(Category category)
        {
            var target = db.Categories.Where(p => p.ID == category.ID).FirstOrDefault();
            if (target != null)
            {
                target.CategoryName = category.CategoryName.Trim();

                db.SaveChanges();
            }
        }

        public void Delete(Category category)
        {
            var target = db.Categories.Find(category.ID);
            if (target != null)
            {
                //if (!db.UserCountries.Any(c => c.CountryID == target.ID))
                //{
                db.Categories.Remove(target);
                db.SaveChanges();
                //}
            }
        }
    }
}