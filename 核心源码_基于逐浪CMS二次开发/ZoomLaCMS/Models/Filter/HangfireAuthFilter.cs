using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.BLL;
using ZoomLa.Model;

namespace ZoomLaCMS.Filter
{
    public class HangfireAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            M_AdminInfo adminMod = B_Admin.GetLogin(context.GetHttpContext());
            if (adminMod == null || adminMod.AdminId != 1) { return false; }
            else { return true; }
        }
    }
}
