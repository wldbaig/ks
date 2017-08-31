using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace KS.Classes
{
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }
        public bool IsInRole(string role)
        {
            return false;
           
        }

        public CustomPrincipal(string Username)
        {
            this.Identity = new GenericIdentity(Username);
        }

        public int ID { get; set; }
        public string NAME { get; set; }
        public string LOGINID { get; set; }
        public Nullable<System.DateTime> LASTLOGIN { get; set; }
        public Nullable<int> LOGINCOUNT { get; set; }
        public Nullable<System.DateTime> USERSINCE { get; set; }
        public int ROLEID { get; set; }
        public int[] COUNTRYID { get; set; }
        public string ROLE { get; set; }
        public bool ISACTIVE { get; set; }

       
    }
}