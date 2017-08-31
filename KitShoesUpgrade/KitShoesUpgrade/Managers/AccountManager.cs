using KitShoesUpgrade.Classes;
using KitShoesUpgrade.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace KitShoesUpgrade.Managers
{
    public class AccountManager : BaseManager
    {
        public bool Login(LoginViewModel model)
        {
            string loginID = model.LoginID.ToUpper();
            string password = model.Password;
            string hashedPassword = Encrypt(password);
         //   short ApprovalStatus = (short)eApprovalStatus.Approved;

            bool userExist = db.UserCredentials.Any(u => u.LoginID.ToUpper() == loginID);// || u.EmailAddress.ToUpper() == loginID);

            if (userExist && (string.Compare(hashedPassword, db.UserCredentials.First(u => u.LoginID.ToUpper() == loginID ).Password.ToString()) == 0))
            {
                // ---- Update User fields
                var user = db.UserCredentials.Where(u => u.LoginID.ToUpper() == loginID  && u.Password == hashedPassword).First();
                if (user != null)
                {
                    user.PreviousLogin = user.LastLogin;
                    user.LastLogin = DateTime.UtcNow;
                    user.LoginCount = (user.LoginCount == null) ? 1 : user.LoginCount + 1;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //---- Generate Authentication Ticket
                DateTime cookieIssuedDate = DateTime.UtcNow;

                LoggedInUser loginUser = new LoggedInUser();
                loginUser.NAME = user.Name;
                loginUser.ID = user.ID;
                loginUser.ISACTIVE = true;
                loginUser.LASTLOGIN =  user.LastLogin;
                loginUser.PREVIOUSLOGIN =  user.PreviousLogin ;
                loginUser.LOGINCOUNT = user.LoginCount ;
                loginUser.USERSINCE =  user.UserSince;
                loginUser.LOGINID = user.LoginID;
                loginUser.ROLEID = user.RoleID;
                loginUser.ROLE = db.Roles.Find(user.RoleID).RoleType;

                string userData = JsonConvert.SerializeObject(loginUser);

                var ticket = new FormsAuthenticationTicket(0,
                    model.LoginID,
                    cookieIssuedDate,
                    (model.RememberMe) ? cookieIssuedDate.AddDays(7) : cookieIssuedDate.AddMinutes(30),//FormsAuthentication.Timeout.TotalMinutes),
                    model.RememberMe,
                    userData,
                    FormsAuthentication.FormsCookiePath);

                string encryptedCookieContent = FormsAuthentication.Encrypt(ticket);

                var formsAuthenticationTicketCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedCookieContent)
                {
                    Domain = FormsAuthentication.CookieDomain,
                    Path = FormsAuthentication.FormsCookiePath,
                    HttpOnly = true,
                    Secure = FormsAuthentication.RequireSSL
                };

                // ---- if remember me is checked then the cookie will expire after 7 days else at end of session
                if (model.RememberMe)
                    formsAuthenticationTicketCookie.Expires = cookieIssuedDate.AddDays(7);

                System.Web.HttpContext.Current.Response.Cookies.Add(formsAuthenticationTicketCookie);

                // ---- Load Group Permissions and assign the list to security permissions model
                //List<RolePermission> permissions = db.RolePermissions.ToList();
                //SecurityPermissions.RPermissions = permissions;

                //sLogger.Info(loginID + " Logged In. IP: " + Request.UserHostAddress +
                //    ", User Agent: " + Request.UserAgent);

                //return RedirectToLocal(returnUrl);
                return true;
            }
            else
            {
                if (!userExist)
                    throw new KSException(eErrorCode.InvalidLoginID);
                else
                    throw new KSException(eErrorCode.InvalidCredentials);
            }
        }

    }
}