using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KitShoesUpgrade.Managers
{
    public class ArticleManager : BaseManager
    {
        int[] Sizes = new int[] { 550, 300 };

        public int AddArticle(ArticleViewModel model, CustomPrincipal User)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                // ---- Save Product
                model.Article.ArticleName = model.Article.ArticleName.Trim();
                model.Article.CategoryId = Convert.ToInt32(model.Article.CategoryId);
                model.Article.Image = "";
                model.Article.Price = Convert.ToDecimal(model.Article.Price);
                model.Article.Cost = Convert.ToDecimal(model.Article.Cost);
                model.Article.CreatedBy = User.ID;
                model.Article.IsActive = true;
                model.Article.CreatedOn = DateTime.UtcNow;
                db.Articles.Add(model.Article);
                db.SaveChanges();

                if (model.Files != null)
                {
                    foreach (var image in model.Files)
                    {
                        UploadImage(image, model.Article.ID);
                        var fileName = "image_o_" + model.Article.ID + Path.GetExtension(image.FileName);
                        model.Article.Image = fileName;
                        db.Entry(model.Article).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                trans.Complete();
            }

            return model.Article.ID;
        }

        public int UpdateArticle(ArticleViewModel model, CustomPrincipal User)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                // ---- Save Product
                var article = db.Articles.Find(model.Article.ID);
                article.ArticleName = model.Article.ArticleName.Trim();
                article.Price = Convert.ToDecimal(model.Article.Price);
                article.Cost = Convert.ToDecimal(model.Article.Cost);
                article.CategoryId = Convert.ToInt32(model.Article.CategoryId);
                article.PairInCarton = Convert.ToInt32(model.Article.PairInCarton);
                if (model.Files != null)
                    foreach (var image in model.Files)
                    {
                        // ---- Upload Images
                        UploadImage(image, article.ID);

                        var fileName = "image_o_" + article.ID + Path.GetExtension(image.FileName);

                        article.Image = fileName;
                    }
                article.UpdatedBy = User.ID;
                article.UpdatedOn = DateTime.UtcNow;
                db.Entry(article).State = EntityState.Modified;
                db.SaveChanges();

                trans.Complete();
            }

            return model.Article.ID;
        }

        public void AddArticleDetail(ArticleDetailViewModel model)//, string path)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                var article = db.Articles.Find(model.ArticleID);

                // ---- Add product item record
                model.ArticleDetails.DateAdded = DateTime.UtcNow;

                model.ArticleDetails.TotalStock = (model.ArticleDetails.Carton * article.PairInCarton) + model.ArticleDetails.Pairs;
                model.ArticleDetails.Carton = model.ArticleDetails.TotalStock / article.PairInCarton;
                model.ArticleDetails.Pairs = model.ArticleDetails.TotalStock % article.PairInCarton;

                db.ArticleDetails.Add(model.ArticleDetails);
                db.SaveChanges();

                trans.Complete();
            }
        }

        public void UpdateArticleDetail(ArticleDetailViewModel model)
        {

            using (var trans = new System.Transactions.TransactionScope())
            {
                var existingArticleDetail = db.ArticleDetails.Find(model.ArticleDetails.ID);

                if (existingArticleDetail.ColorId == model.ArticleDetails.ColorId && existingArticleDetail.TotalStock != ((model.ArticleDetails.Carton * existingArticleDetail.Article.PairInCarton) + model.ArticleDetails.Pairs))
                {
                    existingArticleDetail.TotalStock = (model.ArticleDetails.Carton * existingArticleDetail.Article.PairInCarton) + model.ArticleDetails.Pairs;
                    existingArticleDetail.Carton = existingArticleDetail.TotalStock / existingArticleDetail.Article.PairInCarton;
                    existingArticleDetail.Pairs = existingArticleDetail.TotalStock % existingArticleDetail.Article.PairInCarton;

                    db.Entry(existingArticleDetail).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else if (!db.ArticleDetails.Any(c => c.ArticleId == existingArticleDetail.ArticleId && c.ColorId == model.ArticleDetails.ColorId))
                {
                    existingArticleDetail.ColorId = model.ArticleDetails.ColorId;

                    existingArticleDetail.TotalStock = (model.ArticleDetails.Carton * existingArticleDetail.Article.PairInCarton) + model.ArticleDetails.Pairs;
                    existingArticleDetail.Carton = existingArticleDetail.TotalStock / existingArticleDetail.Article.PairInCarton;
                    existingArticleDetail.Pairs = existingArticleDetail.TotalStock % existingArticleDetail.Article.PairInCarton;

                    db.Entry(existingArticleDetail).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("An article with this color already exist");
                }
                trans.Complete();
            }
        }

        public void UpdateStock(ArticleDetailViewModel model)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                var existingArticleDetail = db.ArticleDetails.Find(model.ArticleDetails.ID);

                //    existingArticleDetail.Stock = existingArticleDetail.Stock + model.newItem;
                existingArticleDetail.TotalStock += (model.Carton * existingArticleDetail.Article.PairInCarton) + model.Pair;
                existingArticleDetail.Carton = existingArticleDetail.TotalStock / existingArticleDetail.Article.PairInCarton;
                existingArticleDetail.Pairs = existingArticleDetail.TotalStock % existingArticleDetail.Article.PairInCarton;

                db.Entry(existingArticleDetail).State = EntityState.Modified;
                db.SaveChanges();

                trans.Complete();
            }
        }

        private void UploadImage(HttpPostedFileBase image, int imageID)
        {
            var origionalFileName = "image_o_" + imageID + Path.GetExtension(image.FileName);
            var imagePath = HttpContext.Current.Server.MapPath("~/images/" + (int)eDirectory.Article + "/" + imageID % 10 + "/");

            // ---- Save original
            image.SaveAs(Path.Combine(imagePath, origionalFileName));

            // ---- Save resized images
            foreach (var size in Sizes)
            {
                var fileName = "image_" + size + "_" + imageID + Path.GetExtension(image.FileName);

                // ---- Resize
                var buffer = GetResizedImage(Path.Combine(imagePath, origionalFileName), size, size);

                var path = Path.Combine(imagePath, fileName);
                File.WriteAllBytes(path.ToString(), buffer);
            }

        }

        byte[] GetResizedImage(string path, int width, int height)
        {
            Bitmap imgIn = new Bitmap(path);
            double y = imgIn.Height;
            double x = imgIn.Width;

            double factor = 1;
            if (width > 0 && width > height)
            {
                factor = width / x;
            }
            else if (height > 0)
            {
                factor = height / y;
            }
            System.IO.MemoryStream outStream = new System.IO.MemoryStream();
            Bitmap imgOut = new Bitmap((int)(x * factor), (int)(y * factor));

            // Set DPI of image (xDpi, yDpi)
            imgOut.SetResolution(72, 72);

            Graphics g = Graphics.FromImage(imgOut);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(System.Drawing.Color.White);
            g.DrawImage(imgIn, new Rectangle(0, 0, (int)(factor * x), (int)(factor * y)),
              new Rectangle(0, 0, (int)x, (int)y), GraphicsUnit.Pixel);

            imgOut.Save(outStream, GetImageFormat(path));

            imgOut.Dispose();
            imgIn.Dispose();
            return outStream.ToArray();
        }

        ImageFormat GetImageFormat(String path)
        {
            switch (Path.GetExtension(path))
            {
                case ".bmp": return ImageFormat.Bmp;
                case ".gif": return ImageFormat.Gif;
                case ".jpg": return ImageFormat.Jpeg;
                case ".png": return ImageFormat.Png;
                default: break;
            }
            return ImageFormat.Jpeg;
        }                                   
    }                               
}