using Microsoft.AspNet.Identity;
using proiectDAW.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace proiectDAW.Controllers
{
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "User,Organizer,Admin")]
        public ActionResult Index()
        {
            /*
             Aici o sa dam show numai la proiectele din care fac parte userii, ma rog.. si admin-ul le vede pe toate
             */

            ViewBag.Projects = db.Projects;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View();
        }

        //New
        [Authorize(Roles = "User,Organizer,Admin")]
        public ActionResult New()
        {
            Project project = new Project();
            project.OrganizerId = User.Identity.GetUserId();
            return View(project);
        }
        [HttpPost]
        [Authorize(Roles = "User,Organizer,Admin")]
        public ActionResult New(Project NewProject)
        {
            NewProject.OrganizerId = User.Identity.GetUserId();
            try
            {
                if(ModelState.IsValid)
                {
                    db.Projects.Add(NewProject);
                    db.SaveChanges();
                    TempData["message"] = "Adaugare efectuata cu succes!";
                    return RedirectToAction("Index");
                }
                return View(NewProject);
            }
            catch (Exception e)
            {
                return View(NewProject);
            }
        }

        //Edit
        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult Edit(int id)
        {
            Project project = db.Projects.Find(id);
            if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(project);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine!";
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult Edit(int id, Project EditedProject)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Project project = db.Projects.Find(id);
                    if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                    {
                        if (TryUpdateModel(project))
                        {
                            project.ProjectTitle = EditedProject.ProjectTitle;
                            project.ProjectDesc = EditedProject.ProjectDesc;
                            db.SaveChanges();
                            TempData["message"] = "Proiectul a fost actualizat cu succes!";
                            return RedirectToAction("Index");
                        }
                        return View(EditedProject);
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui proiect care nu va apartine!";
                        return RedirectToAction("Index");
                    }
                }
                else
                    return View(EditedProject);

            }
            catch (Exception e)
            {
                return View(EditedProject);
            }
        }

        //Show
        [Authorize(Roles = "User,Organizer,Admin")]
        public ActionResult Show(int id)
        {
            Project project = db.Projects.Find(id);
            ViewBag.StateList = GetStateList();
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["Message"];
            }
            return View(project);
        }

        //Delete
        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult Delete(int id)
        {
            Project project = db.Projects.Find(id);
            if (project.OrganizerId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Projects.Remove(project);
                db.SaveChanges();
                TempData["message"] = "Proiectul a fost sters cu succes!";
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un proiect care nu va apartine!";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
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
    }
}