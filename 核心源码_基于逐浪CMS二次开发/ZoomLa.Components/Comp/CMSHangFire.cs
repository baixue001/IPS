using System;
using System.Collections.Generic;
using System.Text;

//namespace ZoomLa.Components.Comp
//{

//}
//---------------------------------------自定义方法
//public class CustomTaskFunc
//{
//    public void Execute()
//    {
//        Random ran = new Random();
//        int a = ran.Next(1, 10000);
//        int b = a;
//        //context.Wait(TimeSpan.FromMinutes(1));
//    }
//}
//RecurringJob.AddOrUpdate<CustomTaskFunc>(x=>x.Execute(), Cron.Minutely);
//---------------------------------------单行代码
//RecurringJob.AddOrUpdate(() => Console.WriteLine("Daily Job"), Cron.Minutely);//循环执行
//BackgroundJob.Enqueue(()=>Console.WriteLine("test for you "));//只执行一次