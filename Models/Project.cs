using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace proiectDAW.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required(ErrorMessage ="Titlul proiectului este obligatoriu")]
        [StringLength(100,ErrorMessage ="Lungimea titlului nu poate depasi 100 de caractere")]
        public string ProjectTitle { get; set; }

        [Required(ErrorMessage ="Descrierea proiectului este obligatorie")]
        [DataType(DataType.MultilineText)]
        public string ProjectDesc { get; set; }

        public string OrganizerId { get; set; }
        public virtual ApplicationUser Organizer { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }

        public virtual ICollection<Task> Tasks { get; set; }
    }
}