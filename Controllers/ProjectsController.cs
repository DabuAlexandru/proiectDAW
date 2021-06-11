using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
            ViewBagRoles();
            ViewBagProjects();              

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
                    //db.SaveChanges();
                    
                    //ADAUGARE ROL DE ORGANIZATOR
                    if (!User.IsInRole("Organizer") && !User.IsInRole("Admin"))
                    {
                        string currentUserId = User.Identity.GetUserId();
                        ApplicationUser currentUser = db.Users.Find(currentUserId);
                        ApplicationDbContext context = new ApplicationDbContext();
                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                        var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                        
                        
                        var roles = from role in db.Roles select role;
                            
                        foreach(var role in roles)
                        {
                            UserManager.RemoveFromRole(currentUserId, role.Name);
                        }

                        UserManager.AddToRole(currentUserId, "Organizer");
                    }

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
        [Authorize(Roles = "Organizer, Admin")]
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
        [Authorize(Roles = "User, Organizer, Admin")]
        public ActionResult Show(int id)
        {
            Project project = db.Projects.Find(id);
            if (project.OrganizerId != User.Identity.GetUserId() && !User.IsInRole("Admin") && !IsInTeam(project, User.Identity.GetUserId()))
            {
                TempData["message"] = "Nu aveti dreptul sa vizualizati un proiect din care nu faceti parte!";
                return RedirectToAction("Index");
            }
            ViewBag.StateList = GetStateList();
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["Message"];
            }

            ViewBag.isAdmin = User.IsInRole("Admin");
            ViewBag.isOrganizer = User.IsInRole("Organizer");
            ViewBag.currentUserId = User.Identity.GetUserId();

            return View(project);
        }

        //Delete
        [HttpDelete]
        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult Delete(int id)
        {
            Project project = db.Projects.Find(id);
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);

            if (project.OrganizerId == currentUserId || User.IsInRole("Admin"))
            {
                db.Projects.Remove(project);
                db.SaveChanges();

                //UPDATAM ROLUL UTILIZATORULUI (IN CAZUL IN CARE NU ESTE ADMIN)
                //TO DO: EVITARE STERGERE LISTA ROLURI DACA ESTE ORGANIZATOR AL ALTOR PROIECTE
                if(User.IsInRole("Organizer"))
                {
                    int nr = currentUser.OrgProjects.Count();
                    ApplicationDbContext context = new ApplicationDbContext();
                    var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                    var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


                    var roles = from role in db.Roles select role;

                    foreach (var role in roles)
                    {
                        UserManager.RemoveFromRole(currentUserId, role.Name);
                    }

                    //VERIFICAM DACA UTILIZATORUL MAI ESTE ORGANIZATOR AL ALTOR PROIECTE
                   
                    if (nr > 0)
                        UserManager.AddToRole(currentUserId, "Organizer");
                    else
                        UserManager.AddToRole(currentUserId, "User");

                    db.SaveChanges();

                }
                TempData["message"] = "Proiectul a fost sters cu succes!";
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un proiect care nu va apartine!";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult MemberPanel(int id)
        {
            Project project = db.Projects.Find(id);
            ViewBag.memberCount = project.Users.Count();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }

            return View(project);
        }

        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult AddMember(int id, string username)
        {
            Project project = db.Projects.Find(id);
            ApplicationDbContext context = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            
            var query = from user in db.Users
                            where user.UserName == username
                            select user;
            var newMember = query.FirstOrDefault<ApplicationUser>();

            //FirstOrDefault VA RETURNA NULL IN CAZUL IN CARE QUERYUL NU A GASIT NICI UN USER
            if (newMember != null)
            {
                bool isAdmin = UserManager.IsInRole(newMember.Id, "Admin");
                bool isMember = project.Users.Contains(newMember);

                if (username == currentUser.UserName || isMember || isAdmin)
                {
                    TempData["message"] = "Acest utilizator are deja acces la acest proiect!";
                }
                else
                {
                    try
                    {
                        project.Users.Add(newMember);
                        db.SaveChanges();
                        TempData["message"] = "Adaugare efectuata cu succes!";
                    }
                    catch (Exception e)
                    {
                        TempData["message"] = "Eroare la adaugare!";
                        return Redirect("/Projects/MemberPanel/" + id.ToString());
                    }
                }
            }
            else
                TempData["message"] = "Nu exista niciun utilizator cu numele: " + username;
            return Redirect("/Projects/MemberPanel/" + id.ToString());
        }

        [HttpDelete]
        [Authorize(Roles = "Organizer,Admin")]
        public ActionResult RemoveMember(string id, int projectId)
        {
            Project currentProject = db.Projects.Find(projectId);
            ApplicationUser toBeRemoved = db.Users.Find(id);

            currentProject.Users.Remove(toBeRemoved);
            db.SaveChanges();
            return Redirect("/Projects/MemberPanel/" + projectId.ToString());
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
        public void ViewBagRoles()
        {
            string currentUserId = User.Identity.GetUserId();

            ApplicationDbContext context = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            ViewBag.isAdmin = UserManager.IsInRole(currentUserId, "Admin");
            ViewBag.isOrganizer = UserManager.IsInRole(currentUserId, "Organizer");


        }

        [NonAction]
        public void ViewBagProjects()
        {
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            ApplicationDbContext context = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (UserManager.IsInRole(currentUserId, "Admin"))
                ViewBag.Projects = db.Projects;
            else
            {
                ViewBag.UserTeams = currentUser.Projects;
                ViewBag.teamCount = currentUser.Projects.Count();
            }

            if (UserManager.IsInRole(currentUserId, "Organizer"))
                ViewBag.UserOrgProjects = currentUser.OrgProjects;
        }

        [NonAction]
        public bool IsInTeam(Project project, string currentUserId)
        {
            if(project.Users.FirstOrDefault(m => m.Id == currentUserId) is null)
                return false;
            return true;
        }
    }
}