using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoomLa.BLL.User;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using ZoomLa.BLL;

namespace ZoomLaCMS.Ctrl
{
    public class Ctrl_Base : Controller
    {
        public virtual int Mid { get { return DataConvert.CLng(Request.Query["id"]); } }
        //a:action
        public string action { get { return DataConvert.CStr(Request.Query["action"]); } }
        public string ids { get { return DataConvert.CStr(RequestEx["ids"]); } }
        private int _psize = 10;
        public int CPage
        {
            get
            {
                return DataConvert.CLng(GetParam("cpage"), 1);
            }
        }
        public int PSize
        {
            get
            {
                if (!string.IsNullOrEmpty(GetParam("psize")))
                {
                    _psize = DataConvert.CLng(GetParam("psize"));
                }
                return _psize;
            }
            set { _psize = value; }
        }
        public string err = "";
        public int Success = 1, Failed = -1;
        private RequestExtend _RequestEx = null;
        public RequestExtend RequestEx
        {
            get
            {
                if (_RequestEx == null)
                {
                    _RequestEx = new RequestExtend();
                    _RequestEx.Request = HttpContext.Request;
                }
                return _RequestEx;
            }
        }
        //--------------
        private ErrorViewModel errMod = new ErrorViewModel();
        //获取路由或地址栏传参
        public string GetParam(string name)
        {
            string value = Request.GetParam(name);
            if (string.IsNullOrEmpty(value) && RouteData.Values[name] != null)
            {
                value = RouteData.Values[name].ToString();
            }
            return value;
        }
        public IActionResult WriteErr(string msg, string url = "", string title = "")
        {
            errMod.Message = msg;
            errMod.ReturnUrl = url;
            return View("~/Views/Shared/Prompt/Error.cshtml", errMod);
        }
        public IActionResult WriteOK(string msg, string url = "")
        {
            errMod.Message = msg;
            errMod.ReturnUrl = url;
            return View("~/Views/Shared/Prompt/OK.cshtml", errMod);
        }
        //前端执行JS
        public ContentResult JavaScript(string js)
        {
            ContentResult Content = new ContentResult();
            Content.Content = "<script>parent.mybind();</script>";
            Content.ContentType = "text/html";
            return Content;
        }
    }
    public class RequestExtend
    {
        public HttpRequest Request = null;
        public string this[string name]
        {
            get
            {
                return this.Request.GetParam(name);
            }
        }
    }

    /// <summary>
    /// [Authorize(Policy = "Admin")]
    /// </summary>
    public class AdminRequirement : AuthorizationHandler<AdminRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            //if (!context.User.HasClaim(c => c.Type == ClaimTypes.DateOfBirth))
            //{
            //    context.Fail();
            //}

            //获取当前http请求的context对象
            var mvcContext = context.Resource as AuthorizationFilterContext;
            //var dateOfBirth = Convert.ToDateTime(context.User.FindFirst(c => c.Type == ClaimTypes.DateOfBirth).Value);
            //var age = mvcContext.HttpContext.Request.Query.FirstOrDefault(u => u.Key == "age");
            var model = ZoomLa.BLL.B_Admin.GetLogin(mvcContext.HttpContext);
            if (model != null)
            {
                context.Succeed(requirement);
            }
            else
            {

                mvcContext.Result = new RedirectResult("/Admin/Login");
                context.Succeed(requirement);
                //context.Fail();
            }
            //return Task.FromResult(1);
            return Task.CompletedTask;
        }
    }
    /// <summary>
    /// [Authorize(Policy = "User")]
    /// </summary>
    public class UserRequirement : AuthorizationHandler<UserRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRequirement requirement)
        {
            var mvcContext = context.Resource as AuthorizationFilterContext;
            B_User buser = new B_User(mvcContext.HttpContext);

            var model = buser.GetLogin();
            if (model.UserID > 0)
            {
                context.Succeed(requirement);
            }
            else
            {

                mvcContext.Result = new RedirectResult("/User/Login");
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
    /// <summary>
    /// [Authorize(Policy = "Plat")]
    /// </summary>
    public class PlatRequirement : AuthorizationHandler<PlatRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PlatRequirement requirement)
        {
            var mvcContext = context.Resource as AuthorizationFilterContext;
            B_User buser = new B_User(mvcContext.HttpContext);

            var model = buser.GetLogin();
            if (model.UserID > 0)
            {
                context.Succeed(requirement);
            }
            else
            {

                mvcContext.Result = new RedirectResult("/User/Login");
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }


}