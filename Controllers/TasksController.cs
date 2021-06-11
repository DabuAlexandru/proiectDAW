using Microsoft.AspNet.Identity;
using proiectDAW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace proiectDAW.Controllers
{
    public class TasksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Tasks
        //New
        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult New(int id)
        {
            Project project = db.Projects.Find(id);
            // un alt organizator nu poate adauga un nou task in proiectul altui organizator
            if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                Task task = new Task();
                task.ProjectId = id;
                task.StartDate = DateTime.Now;
                task.EndDate = task.StartDate.AddDays(1);
                task.State = 0;

                return View(task);
            }
            else
            {
                return Redirect("/Home/Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult New(Task NewTask)
        {
            try
            {
                ViewBag.StateList = GetStateList();
                Project project = db.Projects.Find(NewTask.ProjectId);
                if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                {
                    if (DateTime.Compare(NewTask.StartDate, NewTask.EndDate) >= 0)
                    {
                        ViewBag.Message = "Data de final trebuie sa fie mai mare decat data de inceput!!!";
                        return View(NewTask);
                    }
                    if (ModelState.IsValid)
                    {
                        
                        db.Tasks.Add(NewTask);
                        db.SaveChanges();
                        TempData["message"] = "Task adaugat cu succes";
                        return Redirect("/Projects/Show/" + NewTask.ProjectId.ToString());
                    }
                    return View(NewTask);
                }
                else
                {
                    return Redirect("/Home/Index");
                }
            }
            catch (Exception e)
            {
                return View(NewTask);
            }
        }
        //Show
        [Authorize(Roles = "User,Organizer,Admin")]
        public ActionResult Show(int id)
        {
            // aici poate sa punem sa faca parte din echipa proiectului pentru a putea vizualiza
            ViewBag.StateList = GetStateList();
            Task task = db.Tasks.Find(id);
            if (task.Project.OrganizerId != User.Identity.GetUserId() && !User.IsInRole("Admin") && !IsInTeam(task.Project, User.Identity.GetUserId()))
            {
                TempData["message"] = "Nu aveti dreptul sa vizualizati task-ul unui proiect din care nu faceti parte!";
                return Redirect("/Projects/Index");
            }
            // ViewBag.DisplayState = GetStateList().SingleOrDefault(m => m.Value == task.State.ToString()).Text;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            ViewBag.isAdmin = User.IsInRole("Admin");
            ViewBag.isOrganizer = User.IsInRole("Organizer");
            ViewBag.currentUserId = User.Identity.GetUserId();

            return View(task);
        }
        //Edit
        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult Edit(int id)
        {
            Task task = db.Tasks.Find(id);
            Project project = db.Projects.Find(task.ProjectId);
            ViewBag.StateList = GetStateList();
            if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(task);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine!";
                return Redirect("/Projects/Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult Edit(int id, Task EditedTask)
        {
            try
            {
                // ViewBag.StateList = new List<String>{ "Not Started", "In Progress", "Done"};
                ViewBag.StateList = GetStateList();
                if (DateTime.Compare(EditedTask.StartDate, EditedTask.EndDate) >= 0)
                {
                    ViewBag.Message = "Data de final trebuie sa fie mai mare decat data de inceput!!!";
                    return View(EditedTask);
                }
                if (ModelState.IsValid)
                {
                    Task task = db.Tasks.Find(id);
                    Project project = db.Projects.Find(task.ProjectId);
                    // un alt organizator nu poate edita un task din proiectul altui organizator
                    if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(task))
                        {
                            task = EditedTask;
                            db.SaveChanges();
                            TempData["message"] = "Taskul a fost actualizat cu succes!";
                            return Redirect("/Tasks/Show/" + EditedTask.TaskId.ToString());
                        }
                        return View(EditedTask);
                    }
                    else
                    {
                        return Redirect("/Home/Index");
                    }
                }
                else
                    return View(EditedTask);
                
            }
            catch(Exception e)
            {
                return View(EditedTask);
            }
        }

        //Delete
        [HttpDelete]
        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult Delete(int id)
        {
            Task task = db.Tasks.Find(id);
            Project project = db.Projects.Find(task.ProjectId);
            // un alt organizator nu sterge un task din proiectul altui organizator
            if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Tasks.Remove(task);
                db.SaveChanges();
                TempData["message"] = "Taskul a fost sters cu succes";

                return Redirect("/Projects/Show/" + task.ProjectId.ToString());
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine!";
                return Redirect("/Home/Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Organizer, Admin, User")]
        public ActionResult ChangeState(int id, Task EditedTask)
        {
            // aici tot ca la Show, deoarece practic fac parte din aceeasi serie de metode
            try
            {
                Project project = db.Projects.Find(EditedTask.ProjectId);
                if (project.OrganizerId != User.Identity.GetUserId() && !User.IsInRole("Admin") && !IsInTeam(project, User.Identity.GetUserId()))
                {
                    TempData["message"] = "Nu aveti dreptul sa modificati state-ul unui task al unui proiect din care nu faceti parte!";
                    return Redirect("/Home/Index");
                }

                ViewBag.StateList = GetStateList();
                ViewBag.isAdmin = User.IsInRole("Admin");
                ViewBag.isOrganizer = User.IsInRole("Organizer");
                ViewBag.currentUserId = User.Identity.GetUserId();

                if (ModelState.IsValid)
                {
                    Task task = db.Tasks.Find(id);
                    if (TryUpdateModel(task))
                    {
                        task = EditedTask;
                        db.SaveChanges();
                        TempData["message"] = "Taskul a fost actualizat cu succes!";
                        return Redirect("/Tasks/Show/" + EditedTask.TaskId.ToString());
                    }
                    return Redirect("/Tasks/Show/" + EditedTask.TaskId.ToString());
                }
                else
                    return Redirect("/Tasks/Show/" + EditedTask.TaskId.ToString());
            }
            catch (Exception e)
            {
                return View(EditedTask);
            }
        }

        [Authorize(Roles = "Organizer, Admin")]
        public ActionResult Assign(int id)
        {
            Task task = db.Tasks.Find(id);
            return View(task);
        }

        [HttpPut]
        [Authorize(Roles = "Organizer, Admin")]
        public ActionResult Assign(int id, Task EditedTask)
        {
            try
            {
                // ViewBag.StateList = new List<String>{ "Not Started", "In Progress", "Done"};
                ViewBag.StateList = GetStateList();

                if (ModelState.IsValid)
                {
                    Task task = db.Tasks.Find(id);
                    Project project = db.Projects.Find(task.ProjectId);
                    // un alt organizator nu poate edita un task din proiectul altui organizator
                    if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(task))
                        {
                            task = EditedTask;
                            db.SaveChanges();
                            TempData["message"] = "Taskul a fost actualizat cu succes!";
                            return Redirect("/Tasks/Show/" + EditedTask.TaskId.ToString());
                        }
                        return View(EditedTask);
                    }
                    else
                    {
                        return Redirect("/Home/Index");
                    }
                }
                else
                    return View(EditedTask);

            }
            catch (Exception e)
            {
                return View(EditedTask);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Organizer, Admin, User")]
        public ActionResult Show(Comment NewComment)
        {
            NewComment.UserId = User.Identity.GetUserId();
            ViewBag.StateList = GetStateList();
            ViewBag.isAdmin = User.IsInRole("Admin");
            ViewBag.isOrganizer = User.IsInRole("Organizer");
            ViewBag.currentUserId = User.Identity.GetUserId();

            try
            {
                Task a = db.Tasks.Find(NewComment.TaskId);
                // nu se poate accesa un task al proiectului de care nu apartineti
                if (a.Project.OrganizerId != User.Identity.GetUserId() && !User.IsInRole("Admin") && !IsInTeam(a.Project, User.Identity.GetUserId()))
                {
                    return Redirect("/Home/Index");
                }
                if (ModelState.IsValid)
                {
                    db.Comments.Add(NewComment);
                    db.SaveChanges();
                    TempData["message"] = "Comentariu adaugat cu succes!";

                    return Redirect("/Tasks/Show/" + NewComment.TaskId.ToString());
                }
                return View(a);

            }
            catch (Exception e)
            {
                Task a = db.Tasks.Find(NewComment.TaskId);
                return View(a);
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetStateList()
        {
            var selectList = new List<SelectListItem>();

            selectList.Add(new SelectListItem
            {
                Value = "0",
                Text = "Not Started"
            });

            selectList.Add(new SelectListItem
            {
                Value = "1",
                Text = "In Progress"
            });

            selectList.Add(new SelectListItem
            {
                Value = "2",
                Text = "Done"
            });

            return selectList;
        }

        [NonAction]
        public bool IsInTeam(Project project, string currentUserId)
        {
            if (project.Users.FirstOrDefault(m => m.Id == currentUserId) is null)
                return false;
            return true;
        }
    }
}