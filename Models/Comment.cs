using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace proiectDAW.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required(ErrorMessage ="Continutul comentariului este obligatoriu!")]
        public string Content { get; set; }
        public string UserId { get; set; }
        public virtual Task Task { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}