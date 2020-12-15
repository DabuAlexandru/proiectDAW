using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace proiectDAW.Models
{
    public class Task
    {
        [Key]
        public int TaskId { get; set; }

        [Required(ErrorMessage ="Titlul taskului este obligatoriu")]
        [StringLength(100,ErrorMessage ="Titlul taskului nu poate depasi lungimea de 100 de caractere")]
        public string Title { get; set; }

        //[DataType(DataType.MultilineText)]
        public string Description { get; set; }
        
        [Required]
        public int ProjectId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
        
        //[0-2]
        public int State { get; set; }
        /*
        0 - NotStarted
        1 - InProgress
        2 - Done
        */

        public virtual Project Project { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
    }
}