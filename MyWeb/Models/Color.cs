namespace WebApplication2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Color
    {
        [Key]
        [StringLength(20)]
        public string id_color { get; set; }

        [Required]
        [StringLength(50)]
        public string color_name { get; set; }
    }
}
