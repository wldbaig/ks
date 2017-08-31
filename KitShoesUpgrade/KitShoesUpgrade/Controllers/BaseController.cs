using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;


namespace KitShoesUpgrade.Controllers
{
    public class BaseController : Controller
    {
        
        public int[] allowedStores;

        private const string PARAMETER_NAME = "q=";
        private const string ENCRYPTION_KEY = "aSYA36Wr23wdf22";

        public KSEntities db = new KSEntities();

        protected virtual new CustomPrincipal User
        {
            get { return HttpContext.User as CustomPrincipal; }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            

            base.OnActionExecuting(filterContext);
        }

        public void HandleException(Exception exception)
        {
            
        }

        public void HandleException(Exception exception, dynamic model)
        {
            if (model != null && model is ModelMessage)
            {
                if (exception is KSException)
                {
                    model.DisplayMessage = true;
                    model.Message = ((KSException)exception).ErrorMessage;
                }
                else
                {
                    model.DisplayMessage = true;
                    model.Message = exception.Message;
                    // ---- Translate if possible
                }
            }
             
        }

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            return base.BeginExecuteCore(callback, state);

        }

        #region Encryption/decryption

        /// <summary>
        /// Parses the current URL and extracts the virtual path without query string.
        /// </summary>
        /// <returns>The virtual path of the current URL.</returns>
        private string GetVirtualPath()
        {
            string path = HttpContext.Request.RawUrl;
            path = path.Substring(0, path.IndexOf("?"));
            path = path.Substring(path.LastIndexOf("/") + 1);
            return path;
        }

        /// <summary>
        /// Parses a URL and returns the query string.
        /// </summary>
        /// <param name="url">The URL to parse.</param>
        /// <returns>The query string without the question mark.</returns>
        private string ExtractQuery(string url)
        {
            int index = url.IndexOf("?") + 1;
            return url.Substring(index);
        }

        /// <summary>
        /// The salt value used to strengthen the encryption.
        /// </summary>
        private readonly static byte[] SALT = Encoding.ASCII.GetBytes(ENCRYPTION_KEY.Length.ToString());

        /// <summary>
        /// Encrypts any string using the Rijndael algorithm.
        /// </summary>
        /// <param name="inputText">The string to encrypt.</param>
        /// <returns>A Base64 encrypted string.</returns>
        public static string Encrypt(string inputText)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            byte[] plainText = Encoding.Unicode.GetBytes(inputText);
            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(ENCRYPTION_KEY, SALT);

            using (ICryptoTransform encryptor = rijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16)))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainText, 0, plainText.Length);
                        cryptoStream.FlushFinalBlock();
                        return "?" + PARAMETER_NAME + Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts a previously encrypted string.
        /// </summary>
        /// <param name="inputText">The encrypted string to decrypt.</param>
        /// <returns>A decrypted string.</returns>
        public static string Decrypt(string inputText)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            byte[] encryptedData = Convert.FromBase64String(inputText);
            PasswordDeriveBytes secretKey = new PasswordDeriveBytes(ENCRYPTION_KEY, SALT);

            using (ICryptoTransform decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16)))
            {
                using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] plainText = new byte[encryptedData.Length];
                        int decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length);
                        return Encoding.Unicode.GetString(plainText, 0, decryptedCount);
                    }
                }
            }
        }
        #endregion


    }
}