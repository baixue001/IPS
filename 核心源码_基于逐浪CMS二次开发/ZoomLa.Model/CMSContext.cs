using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ZoomLa.Model
{
    public class CMSContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseMySQL(@"server=localhost;database=zoomlacms;uid=root;pwd=111111aa;SslMode=None;");
            //optionsBuilder = optionsBuilder.UseMySQL(@"server=localhost;database=sfsf;uid=sfsf;pwd=1313213;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ZL_User>().HasKey(m => m.UserID);
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<ZL_User> ZL_User { get; set; }
    }
    [Table("ZL_User")]
    public partial class ZL_User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserPwd { get; set; }
        public string Remind { get; set; }
    }
}
