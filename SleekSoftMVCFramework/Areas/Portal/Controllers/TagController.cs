using log4net;
using Microsoft.AspNet.Identity;
using SleekSoftMVCFramework.Controllers;
using SleekSoftMVCFramework.Data.AppEntities;
using SleekSoftMVCFramework.Repository;
using SleekSoftMVCFramework.Repository.CoreRepositories;
using SleekSoftMVCFramework.Utilities;
using SleekSoftMVCFramework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SleekSoftMVCFramework.Areas.Portal.Controllers
{
    public class TagController : BaseController
    {

        private readonly IRepositoryQuery<Tag, int> _tagQuery;
        private readonly IRepositoryCommand<Tag, int> _tagCommand;
        private readonly IActivityLogRepositoryCommand _activityRepo;
        private readonly ILog _log;
        private readonly Utility _utility;

        public TagController(IActivityLogRepositoryCommand activityRepo, IRepositoryCommand<Tag, int> tagCommand, Utility utility, IRepositoryQuery<Tag, int> tagQuery, ILog log)
        {

            _tagQuery = tagQuery;
            _tagCommand = tagCommand;
            _activityRepo = activityRepo;
            _utility = utility;
            _log = log;
        }

        // GET: Portal/Tag
        public ActionResult Index()
        {
            if (TempData["MESSAGE"] != null)
            {
                ViewBag.Msg = TempData["MESSAGE"] as string;
            }
            var model = _tagQuery.GetAll().Select(e => new TagViewModel()
            {
                Id = e.Id,
                Title=e.Title,
                Code=e.Code,
                Description=e.Description

            });

            return View(model);
        }

        public ActionResult Create()
        {
            CreateViewBagParams();
            return PartialView("_PartialAddEdit", new TagViewModel {Id =0 });
        }

        // POST: Class/Create
        [HttpPost]
        public async Task<ActionResult> Create(TagViewModel tagVm)
        {
            try
            {
                CreateViewBagParams();
                if (ModelState.IsValid)
                {
                    var titleexist = _tagQuery.GetAllList(c => c.Title.ToLower().Trim() == tagVm.Title.ToLower().Trim()).ToList();
                    if (titleexist.Any())
                    {
                        ModelState.AddModelError("", "Title already exist");
                        return PartialView("_PartialAddEdit", tagVm);
                    }
                    var tagmodel = new Tag()
                    {
                        Title = tagVm.Title,
                        Code = tagVm.Code,
                        Description=tagVm.Description,
                        CreatedBy = User.Identity.GetUserId<Int64>()
                    };
                    await _tagCommand.InsertAsync(tagmodel);
                    await _tagCommand.SaveChangesAsync();
                    _activityRepo.CreateActivityLog(string.Format("Created Tag with Title:{0}", tagVm.Title), this.GetContollerName(), this.GetContollerName(), User.Identity.GetUserId<Int64>(), tagmodel);

                    TempData["MESSAGE"] = "Tag " +  tagVm.Title + " was successfully created";
                    ModelState.Clear();
                    return Json(new { success = true });
                }
                else
                {
                    StringBuilder errorMsg = new StringBuilder();

                    foreach (var modelError in ModelState.Values.SelectMany(modelState => modelState.Errors))
                    {
                        errorMsg.AppendLine(modelError.ErrorMessage);
                        ModelState.AddModelError(string.Empty, modelError.ErrorMessage);
                    }
                    ViewBag.ErrMsg = errorMsg.ToString();
                    return PartialView("_PartialAddEdit", tagVm);
                }

            }
            catch (Exception exp)
            {
                _log.Error(exp);
                return View("Error");
            }


        }

        // GET: Class/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            EditViewBagParams();
            try
            {
                if (id <= 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var tagmodel = await _tagQuery.GetAsync(id);
                if (tagmodel == null)
                {
                    return HttpNotFound();
                }
                var tagVm = new TagViewModel()
                {
                   Id = tagmodel.Id,
                   Title=tagmodel.Title,
                   Code=tagmodel.Code,
                   Description=tagmodel.Description
                };

                return PartialView("_PartialAddEdit", tagVm);
            }
            catch (Exception exp)
            {
                _log.Error(exp);
                return View("Error");
            }

        }

        // POST: Class/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(int id, TagViewModel tagVm)
        {
            try
            {
                if (id <= 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if (ModelState.IsValid)
                {
                    int Id = tagVm.Id.GetValueOrDefault();
                    var titleexist = _tagQuery.GetAllList(c => c.Title.ToLower().Trim() == tagVm.Title.ToLower().Trim() && c.Id!= Id).ToList();
                    if (titleexist.Any())
                    {
                        ModelState.AddModelError("", "Title already exist");
                        return PartialView("_PartialAddEdit", tagVm);
                    }

                    var tagmodel = await _tagQuery.GetAsync(Id);
                    tagmodel.Title = tagVm.Title;
                    tagmodel.Code = tagVm.Code;
                    tagmodel.Description = tagVm.Description;
                    tagmodel.UpdatedBy = GetCurrentUserId();

                    await _tagCommand.UpdateAsync(tagmodel);
                    await _tagCommand.SaveChangesAsync();
                    _activityRepo.CreateActivityLog(string.Format("update Tag with Title:{0}", tagmodel.Title), this.GetContollerName(), this.GetContollerName(), User.Identity.GetUserId<Int64>(), tagmodel);

                    TempData["MESSAGE"] = "Tag " + tagmodel.Title + " was successfully updated";
                    ModelState.Clear();
                    return Json(new { success = true });
                }
                else
                {

                    StringBuilder errorMsg = new StringBuilder();

                    foreach (var modelError in ModelState.Values.SelectMany(modelState => modelState.Errors))
                    {
                        errorMsg.AppendLine(modelError.ErrorMessage);
                        ModelState.AddModelError(string.Empty, modelError.ErrorMessage);
                    }
                    ViewBag.ErrMsg = errorMsg.ToString();
                    return PartialView("_PartialAddEdit", tagVm);
                }

            }
            catch (Exception exp)
            {
                _log.Error(exp);
                return View("Error");
            }


        }
    }
}