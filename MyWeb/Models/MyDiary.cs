using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace WebApplication2.Models
{
    public partial class MyDiary : DbContext
    {
        public MyDiary()
            : base("name=MyDiary")
        {
        }

        public virtual DbSet<Color> Colors { get; set; }
        public virtual DbSet<Content> Contents { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
