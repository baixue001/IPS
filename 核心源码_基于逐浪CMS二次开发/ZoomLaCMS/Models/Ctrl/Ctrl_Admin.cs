using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.BLL;
using ZoomLa.Model;

namespace ZoomLaCMS.Ctrl
{
    public class Ctrl_Admin:Ctrl_Base
    {
        private B_User _buser = null;
        public B_User buser
        {
            get
            {
                if (_buser == null) { _buser = new B_User(HttpContext); }
                return _buser;
            }
        }
        public M_AdminInfo adminMod { get { return B_Admin.GetLogin(HttpContext); } }

    }
}
