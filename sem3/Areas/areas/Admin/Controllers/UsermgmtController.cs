using sem3.Models.Entities;
using sem3.Models.ModelViews;
using sem3.Models.Repositories;
using System;
using System.Linq;
using System.Web.Mvc;
using User = sem3.Models.Entities.User;

namespace sem3.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class UsermgmtController : Controller
    {
        private readonly UserRepository _repository = new UserRepository();

        public ActionResult Index()
        {
            var users = _repository.GetAll();
            return View(users);
        }

        public ActionResult Create()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _repository.Add(model);
                    TempData["SuccessMessage"] = "User created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error while creating user: {ex.Message}");
                }
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var user = _repository.GetById(id);
            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _repository.Update(model);
                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error while updating user: {ex.Message}");
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                _repository.Delete(id);
                return Json(new { success = true, message = "User deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error while deleting: {ex.Message}" });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _repository.Dispose();

            base.Dispose(disposing);
        }
    }
}
