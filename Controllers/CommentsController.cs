using Microsoft.AspNet.Identity;
using proiectDAW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace proiectDAW.Controllers
{
    [Authorize(Roles = "User,Organizer,Admin")]
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        
        //Edit
        public ActionResult Edit(int id)
        {
            Comment comment = db.Comments.Find(id);
            if (comment.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(comment);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                return Redirect("/Tasks/Show/" + comment.TaskId.ToString());
            }
        }

        [HttpPut]
        public ActionResult Edit(int id, Comment EditedComment)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    Comment comment = db.Comments.Find(id);
                    if (comment.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(comment))
                        {
                            comment = EditedComment;
                            db.SaveChanges();
                            TempData["message"] = "Comentariu editat cu succes!";
                            return Redirect("/Tasks/Show/" + comment.TaskId.ToString());
                        }
                        return View(EditedComment);
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                        return Redirect("/Tasks/Show/" + comment.TaskId.ToString());
                    }
                }
                return View(EditedComment);
            }
            catch(Exception e)
            {
                return View(EditedComment);
            }
        }
        //Delete
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Comment comment = db.Comments.Find(id);
            if (comment.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comment);
                db.SaveChanges();
                TempData["message"] = "Comentariu sters cu succes!";
                return Redirect("/Tasks/Show/" + comment.TaskId.ToString());
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti";
                return Redirect("/Tasks/Show/" + comment.TaskId.ToString());
            }
        }
    }
}