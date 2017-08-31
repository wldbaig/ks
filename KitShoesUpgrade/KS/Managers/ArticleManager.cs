using KS.Classes;
using KS.Models;
using System;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace KS.Managers
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
                model.Article.CreatedBy = User.ID;
                model.Article.IsActive = true;
                model.Article.CreatedOn = DateTime.UtcNow;
                db.WHArticles.Add(model.Article);
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

                var bpArticle = new BPArticle();
                bpArticle.ID = model.Article.ID;
                bpArticle.ArticleName = model.Article.ArticleName;
                bpArticle.CategoryId = model.Article.CategoryId;
                bpArticle.Image = model.Article.Image;
                bpArticle.Price = model.Article.Price;
                bpArticle.CreatedBy = User.ID;
                bpArticle.IsActive = true;
                bpArticle.CreatedOn = DateTime.UtcNow;
                db.BPArticles.Add(bpArticle);
                db.SaveChanges();

                var cmArticle = new CMArticle();
                cmArticle.ID = model.Article.ID;
                cmArticle.ArticleName = model.Article.ArticleName;
                cmArticle.CategoryId = model.Article.CategoryId;
                cmArticle.Image = model.Article.Image;
                cmArticle.Price = model.Article.Price;
                cmArticle.CreatedBy = User.ID;
                cmArticle.IsActive = true;
                cmArticle.CreatedOn = DateTime.UtcNow;
                db.CMArticles.Add(cmArticle);
                db.SaveChanges();

                var tsArticle = new TSArticle();
                tsArticle.ID = model.Article.ID;
                tsArticle.ArticleName = model.Article.ArticleName;
                tsArticle.CategoryId = model.Article.CategoryId;
                tsArticle.Image = model.Article.Image;
                tsArticle.Price = model.Article.Price;
                tsArticle.CreatedBy = User.ID;
                tsArticle.IsActive = true;
                tsArticle.CreatedOn = DateTime.UtcNow;
                db.TSArticles.Add(tsArticle);
                db.SaveChanges();

                trans.Complete();
            }

            return model.Article.ID;
        }

        public int UpdateArticle(ArticleViewModel model, CustomPrincipal User)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                // ---- Save Product
                var article = db.WHArticles.Find(model.Article.ID);
                article.ArticleName = model.Article.ArticleName.Trim();
                article.Price = Convert.ToDecimal(model.Article.Price);
                article.CategoryId = Convert.ToInt32(model.Article.CategoryId);
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


                var bpArticle = db.BPArticles.Find(model.Article.ID);
                bpArticle.ArticleName = article.ArticleName;
                bpArticle.Price = article.Price;
                bpArticle.CategoryId = article.CategoryId;
                bpArticle.Image = article.Image;
                bpArticle.UpdatedBy = User.ID;
                bpArticle.UpdatedOn = DateTime.UtcNow;
                db.Entry(bpArticle).State = EntityState.Modified;
                db.SaveChanges();

                var cmArticle = db.CMArticles.Find(model.Article.ID);
                cmArticle.ArticleName = article.ArticleName;
                cmArticle.Price = article.Price;
                cmArticle.CategoryId = article.CategoryId;
                cmArticle.Image = article.Image;
                cmArticle.UpdatedBy = User.ID;
                cmArticle.UpdatedOn = DateTime.UtcNow;
                db.Entry(cmArticle).State = EntityState.Modified;
                db.SaveChanges();

                var tsArticle = db.TSArticles.Find(model.Article.ID);
                tsArticle.ArticleName = article.ArticleName;
                tsArticle.Price = article.Price;
                tsArticle.CategoryId = article.CategoryId;
                tsArticle.Image = article.Image;
                tsArticle.UpdatedBy = User.ID;
                tsArticle.UpdatedOn = DateTime.UtcNow;
                db.Entry(tsArticle).State = EntityState.Modified;
                db.SaveChanges();

                trans.Complete();
            }

            return model.Article.ID;
        }

        public void AddArticleDetail(ArticleDetailViewModel model)//, string path)
        {
            using (var trans = new System.Transactions.TransactionScope())
            {
                var artDet = new WHArticleDetail();
                artDet.ColorId = model.ArticleDetails.ColorId;
                artDet.DateAdded = DateTime.UtcNow;
                artDet.TotalStock = 0;
                artDet.WHArticleId = model.ArticleID; 
                db.WHArticleDetails.Add(artDet);
                db.SaveChanges();

                var bpArtDet = new BPArticleDetail();
                bpArtDet.ID = artDet.ID;
                bpArtDet.ColorId = model.ArticleDetails.ColorId;
                bpArtDet.DateAdded = DateTime.UtcNow;
                bpArtDet.TotalStock = 0;
                bpArtDet.BPArticleId = model.ArticleID; 
                db.BPArticleDetails.Add(bpArtDet);
                db.SaveChanges();

                var cmArtDet = new CMArticleDetail();
                cmArtDet.ID = artDet.ID;
                cmArtDet.ColorId = model.ArticleDetails.ColorId;
                cmArtDet.DateAdded = DateTime.UtcNow;
                cmArtDet.TotalStock = 0;
                cmArtDet.CMArticleId = model.ArticleID; 
                db.CMArticleDetails.Add(cmArtDet);
                db.SaveChanges();

                var tsArtDet = new TSArticleDetail();
                tsArtDet.ID = artDet.ID;
                tsArtDet.ColorId = model.ArticleDetails.ColorId;
                tsArtDet.DateAdded = DateTime.UtcNow;
                tsArtDet.TotalStock = 0;
                tsArtDet.TSArticleId = model.ArticleID; 
                db.TSArticleDetails.Add(tsArtDet);
                db.SaveChanges();

                trans.Complete();
            }
        }

        public void UpdateArticleDetail(ArticleDetailViewModel model)
        {

            using (var trans = new System.Transactions.TransactionScope())
            {
                var existingArticleDetail = db.WHArticleDetails.Find(model.ArticleDetails.ID);

                if (!db.WHArticleDetails.Any(c => c.WHArticleId == existingArticleDetail.WHArticleId && c.ColorId == model.ArticleDetails.ColorId))
                {
                    existingArticleDetail.ColorId = model.ArticleDetails.ColorId;
                     
                    db.Entry(existingArticleDetail).State = EntityState.Modified;
                    db.SaveChanges();
                }

                var existingBPArticleDetail = db.BPArticleDetails.Find(model.ArticleDetails.ID);

                if (!db.BPArticleDetails.Any(c => c.BPArticleId == existingBPArticleDetail.BPArticleId && c.ColorId == model.ArticleDetails.ColorId))
                {
                    existingBPArticleDetail.ColorId = model.ArticleDetails.ColorId;

                    db.Entry(existingBPArticleDetail).State = EntityState.Modified;
                    db.SaveChanges();
                }


                var existingCMArticleDetail = db.CMArticleDetails.Find(model.ArticleDetails.ID);

                if (!db.CMArticleDetails.Any(c => c.CMArticleId == existingCMArticleDetail.CMArticleId && c.ColorId == model.ArticleDetails.ColorId))
                {
                    existingCMArticleDetail.ColorId = model.ArticleDetails.ColorId;

                    db.Entry(existingCMArticleDetail).State = EntityState.Modified;
                    db.SaveChanges();
                }


                var existingTSArticleDetail = db.TSArticleDetails.Find(model.ArticleDetails.ID);

                if (!db.TSArticleDetails.Any(c => c.TSArticleId == existingTSArticleDetail.TSArticleId && c.ColorId == model.ArticleDetails.ColorId))
                {
                    existingTSArticleDetail.ColorId = model.ArticleDetails.ColorId;

                    db.Entry(existingTSArticleDetail).State = EntityState.Modified;
                    db.SaveChanges();
                }

                trans.Complete();
            }
        }

        //public void UpdateStock(ArticleDetailViewModel model)
        //{
        //    using (var trans = new System.Transactions.TransactionScope())
        //    {
        //        var existingArticleDetail = db.ArticleDetails.Find(model.ArticleDetails.ID);

        //        //    existingArticleDetail.Stock = existingArticleDetail.Stock + model.newItem;
        //        existingArticleDetail.TotalStock += (model.Carton * existingArticleDetail.Article.PairInCarton) + model.Pair;
        //        existingArticleDetail.Carton = existingArticleDetail.TotalStock / existingArticleDetail.Article.PairInCarton;
        //        existingArticleDetail.Pairs = existingArticleDetail.TotalStock % existingArticleDetail.Article.PairInCarton;

        //        db.Entry(existingArticleDetail).State = EntityState.Modified;
        //        db.SaveChanges();

        //        trans.Complete();
        //    }
        //}

        private void UploadImage(HttpPostedFileBase image, int imageID)
        {
            var origionalFileName = "image_o_" + imageID + Path.GetExtension(image.FileName);
            var imagePath = HttpContext.Current.Server.MapPath("~/images/" + (int)eDirectory.WHArticle + "/" + imageID % 10 + "/");

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