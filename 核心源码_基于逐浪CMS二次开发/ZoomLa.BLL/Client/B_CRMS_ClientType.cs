using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ZoomLa.Components;

namespace ZoomLa.BLL.Client
{
    public class B_CRMS_ClientType
    {
        public string XmlName = "CRM_ClientType.xml";
        public DataTable Sel()
        {
            return XMLDB.SelDT(XmlName);
        }
        public void Del(DataTable dt, int id)
        {
            XMLDB.DelByID(dt, id.ToString());
        }
        public void Update(DataTable dt)
        {
            XMLDB.Update(XmlName, dt);
        }
    }
}
