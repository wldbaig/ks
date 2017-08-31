using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KitShoesUpgrade.Managers
{
    public class ColorManager : BaseManager
    {
        public IEnumerable<Color> All()
        {
            db.Configuration.ProxyCreationEnabled = false;

            return db.Colors.ToList();
        }

        public Color One(Func<Color, bool> p)
        {
            return db.Colors.Where(p).FirstOrDefault();
        }

        public void Insert(Color color)
        {
            color.ColorName = color.ColorName.Trim();

            db.Colors.Add(color);
            db.SaveChanges();
        }

        public void Update(Color color)
        {
            var target = db.Colors.Where(p => p.ID == color.ID).FirstOrDefault();
            if (target != null)
            {
                target.ColorName = color.ColorName.Trim();

                db.SaveChanges();
            }
        }

        public void Delete(Color color)
        {
            var target = db.Colors.Find(color.ID);
            if (target != null)
            {
                db.Colors.Remove(target);
                db.SaveChanges();
            }
        }
    }
}