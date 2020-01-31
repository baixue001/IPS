using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Data;
using System.IO;
using ZoomLa.BLL;
using ZoomLa.Components;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using Hangfire;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Rewrite;

namespace ZoomLaCMS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //DBCenter.DB = SqlBase.CreateHelper("mysql");
            //DBCenter.DB.ConnectionString = "server=localhost;database=zoomlacms;uid=root;pwd=111111aa;SslMode=None;";
            CompHelper.Startup();
            DBCenter.DB = SqlBase.CreateHelper("mssql");
            DBCenter.DB.ConnectionString = SiteConfig.SiteInfo.ConnectionString;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 添加 Cook 服务
            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //.AddCookie(options =>
            //{
            //    options.SlidingExpiration = true;
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.SameSite = SameSiteMode.Lax;
            //    options.Cookie.Path = "/";
            //    //options.LoginPath = "/Admin/Index/Login";
            //    //options.LogoutPath = "/Admin/Index/Logout";
            //});

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.Configure<AuthorizationOptions>(options => {
                options.AddPolicy("Admin", policy => policy.Requirements.Add(new ZoomLaCMS.Ctrl.AdminRequirement()));
                options.AddPolicy("User",  policy => policy.Requirements.Add(new ZoomLaCMS.Ctrl.UserRequirement()));
                options.AddPolicy("Plat", policy => policy.Requirements.Add(new ZoomLaCMS.Ctrl.PlatRequirement()));
                // options.AddPolicy("ManageStore", policy => policy.RequireClaim("Action", "ManageStore"));
            });
            //Configuration.GetConnectionString("DefaultConnection")获取了配置文件appsettings.json中的DefaultConnection节点数
            services.AddMvc(options =>
            {
                options.Filters.Add<ZoomLaCMS.Filter.CMSFilter>();
                //options.Filters.Add<ZoomLaCMS.Filter.HttpGlobalExceptionFilter>(); //加入全局异常类
            }).AddRazorPagesOptions(options =>
            {
                //options.RootDirectory = "/Content";
            });
            services.AddSession();
            //HttpContext   依赖注入-->单例注入
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //Html.Action
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            //文件读取支持
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));

            try
            {
                services.AddHangfire(x => x.UseSqlServerStorage(SiteConfig.SiteInfo.ConnectionString));
            }
            catch { }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    //开发模式下,显示更丰富的信息
            //app.UseBrowserLink();
            app.UseDeveloperExceptionPage();
            //    //app.UseDatabaseErrorPage();
            //}
            //else
            //{
            //    //app.UseExceptionHandler("/Home/Error");
            //}
            //默认起始页
            DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("index.shtml");
            options.DefaultFileNames.Add("index.html");
            options.DefaultFileNames.Add("index.htm");
            app.UseDefaultFiles(options);
            //MIME
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".shtml"] = "text/html";
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider,
                //静态文件处理
                OnPrepareResponse = ctx =>
                {
                    //ctx.Context.Response.Headers.Append("token", "");
                    if (!SiteConfig.SiteOption.SafeDomain.Equals("1") || ctx.Context.Request.IsHttps) { }
                    else
                    {
                        string name = ctx.Context.Request.Path.ToString();
                        if (name.Contains(".htm") || name.Contains(".html") || name.Contains(".shtml"))
                        {
                            ZoomLaCMS.Filter.CMSFilter.ForceToHttps(ctx.Context);
                        }
                    }
                }
            });
            app.UseSession();
            // 使用Cook的中间件
            app.UseAuthentication();
            try
            {
                app.UseHangfireServer();
                app.UseHangfireDashboard("/Admin/Hangfire", new DashboardOptions { Authorization = new[] { new Filter.HangfireAuthFilter() } });
            }
            catch { }
            //强制https
            //app.UseForwardedHeaders(new ForwardedHeadersOptions
            //{
            //    ForwardedHeaders = ForwardedHeaders.XForwardedProto
            //});
            //var options = new RewriteOptions().AddRedirectToHttpsPermanent();
            //app.UseRewriter(options);


            app.UseMvc(routes =>
            {
                //routes.MapRoute(name: "default",template: "{controller=Home}/{action=Index}/{id?}");
                //routes.MapRoute("Default", "{controller=Front}/{action=Default}");
                //routes.MapRoute("Front", "{action:regex(^Default|Content|NodePage$)}/{id?}", new { controller = "Front", action = "Default" });

                //区域路由,所有存在的区域
                //routes.MapRoute(name: "Admin",template: "{area:exists=Admin}/{controller=Index}/{action=Default}");

                //内容页
                routes.MapRoute("Content", "Item/{id}", new { controller = "Front", action = "Content" });
                routes.MapRoute("Content2", "Item/{id}_{cpage}", new { controller = "Front", action = "Content" });
                //栏目列表等页面
                routes.MapRoute("Class", "Class_{id}/{action:regex(^NodePage|NodeNews|NodeHot|NodeElite$)}",
                    new { controller = "Front" });
                routes.MapRoute("Class2", "Class_{id}/Default", new { controller = "Front", action = "ColumnList" });
                //商城店铺
                routes.MapRoute("Shop", "Shop/{id}", new { controller = "Front", action = "ShopContent" });
                routes.MapRoute("Store", "Store/{action}", new { controller = "FrontStore" });
                //-------------贴吧
                routes.MapRoute("Bar", "Bar/{action}", defaults: new { controller = "Bar", action = "Default" });
                //routes.MapRoute("Bar_Index", "Index", defaults: new { controller = "Bar", action = "Index" });
                routes.MapRoute("Bar_PClass", "PClass", defaults: new { controller = "Bar", action = "PClass" });
                routes.MapRoute("Bar_PItem", "PItem", defaults: new { controller = "Bar", action = "PItem" });
                routes.MapRoute("Bar_EditContent", "EditContent", defaults: new { controller = "Bar", action = "EditContent" });
                routes.MapRoute("Bar_PostSearch", "PostSearch", defaults: new { controller = "Bar", action = "PostSearch" });
                //-------------支持方法
                routes.MapRoute("Install", "Install/{action}", new { controller = "Install", action = "Index" });
                routes.MapRoute("Common", "Common/{action}", new { controller = "Common" });
                routes.MapRoute("FrontCom", "com/{action}", new { controller = "FrontCom" });
                routes.MapRoute("FrontSearch", "Search/{action}", new { controller = "FrontSearch", action = "Default" });
                routes.MapRoute("PreView", "PreView/{action}", new { controller = "PreView", action = "Index" });
                routes.MapRoute("IO", "IO/{action}", new { controller = "IO" });
                routes.MapRoute("API", "API/{action}", new { controller = "API" });
                routes.MapRoute("Cart", "Cart/{action}", new { controller = "Cart" });
                routes.MapRoute("PayOnline", "PayOnline/{action}", new { controller = "PayOnline" });
                routes.MapRoute("Vote", "Vote/{action}", new { controller = "Vote" });
                routes.MapRoute("Guest", "Guest/{action}", new { controller = "Guest",action="Default" });
                routes.MapRoute("Baike", "Baike/{action}", new { controller = "Baike", action = "Default" });
                //routes.MapRoute("Bar", "Bar/{action}", new { controller = "Bar", action = "Default" });

                routes.MapAreaRoute("Admin", "Admin", "Admin/{controller=Index}/{action=Default}");
                routes.MapAreaRoute("User", "User", "User/{controller=Index}/{action=Default}");
                routes.MapAreaRoute("Plat", "Plat", "Plat/{controller=Blog}/{action=Default}");
                //首页
                routes.MapRoute("Default", "{action}", new { controller = "Front", action = "Index" });
          
            });


        }

    }
}
