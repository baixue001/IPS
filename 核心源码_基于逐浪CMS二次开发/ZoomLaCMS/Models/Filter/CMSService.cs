using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZoomLaCMS.Models.Filter
{
    /*
     * services.AddSingleton<ICMSService, CMSService>();   //单例模式
     * services.AddScoped<ICMSService, CMSService>();      //一个请求一个实例
     * services.AddTransient<ICMSService, CMSService>();   //每次均创建新实例
     */
    public interface ICMSService
    {
        string ShowText();
    }
    public class CMSService: ICMSService
    {
        public string ShowText()
        {
            return "This is a Service";
        }
    }
}
