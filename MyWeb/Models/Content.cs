namespace WebApplication2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Content
    {
        public int id { get; set; }

        [StringLength(30)]
        [Required]
        public string main_content { get; set; }

        [Required]
        [StringLength(100)]
        public string main_image { get; set; }

        [StringLength(20)]
        public string id_color { get; set; }

        [Column(TypeName = "date")]
        public DateTime? date_upload { get; set; }

        [Column("content")]
        public string content1 { get; set; }
    }
}
