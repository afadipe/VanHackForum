using log4net;
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
    public class TopicCategoryController : BaseController
    {

        private readonly IRepositoryQuery<TopicCategory, int> _topicCategoryQuery;
        private readonly IRepositoryCommand<TopicCategory, int> _topicCategoryCommand;
        private readonly IActivityLogRepositoryCommand _activityRepo;
        private readonly ILog _log;
        private readonly Utility _utility;

        public TopicCategoryController(IActivityLogRepositoryCommand activityRepo, IRepositoryCommand<TopicCategory, int> topicCategoryCommand, Utility utility, IRepositoryQuery<TopicCategory, int> topicCategoryQuery, ILog log)
        {

            _topicCategoryQuery = topicCategoryQuery;
            _topicCategoryCommand = topicCategoryCommand;
            _activityRepo = activityRepo;
            _utility = utility;
            _log = log;
        }


        // GET: Portal/TopicCategory
        public ActionResult Index()
        {
            if (TempData["MESSAGE"] != null)
            {
                ViewBag.Msg = TempData["MESSAGE"] as string;
            }
            var model = _topicCategoryQuery.GetAll().Select(e => new TopicCategoryViewModel()
            {
                Id = e.Id,
                Title = e.Title

            });

            return View(model);
        }

        public ActionResult Create()
        {
            CreateViewBagParams();
            return PartialView("_PartialAddEdit");
        }

        // POST: Class/Create
        [HttpPost]
        public async Task<ActionResult> Create(TopicCategoryViewModel tagVm)
        {
            try
            {
                CreateViewBagParams();
                if (ModelState.IsValid)
                {
                    var titleexist = _topicCategoryQuery.GetAllList(c => c.Title.ToLower().Trim() == tagVm.Title.ToLower().Trim()).ToList();
                    if (titleexist.Any())
                    {
                        ModelState.AddModelError("", "Topic Category already exist");
                        return PartialView("_PartialAddEdit", tagVm);
                    }
                    var topicmodel = new TopicCategory()
                    {
                        Title = tagVm.Title,
                        CreatedBy =GetCurrentUserId()
                    };
                    await _topicCategoryCommand.InsertAsync(topicmodel);
                    await _topicCategoryCommand.SaveChangesAsync();
                    _activityRepo.CreateActivityLog(string.Format("Created Topic category with Title:{0}", tagVm.Title), this.GetContollerName(), this.GetContollerName(), GetCurrentUserId(), topicmodel);

                    TempData["MESSAGE"] = "Topic category " + tagVm.Title + " was successfully created";
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
                var tagmodel = await _topicCategoryQuery.GetAsync(id);
                if (tagmodel == null)
                {
                    return HttpNotFound();
                }
                var tagVm = new TopicCategoryViewModel()
                {
                    Id = tagmodel.Id,
                    Title = tagmodel.Title
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
        public async Task<ActionResult> Edit(int id, TopicCategoryViewModel tagVm)
        {
            try
            {
                if (id <= 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if (ModelState.IsValid)
                {

                    var titleexist = _topicCategoryQuery.GetAllList(c => c.Title.ToLower().Trim() == tagVm.Title.ToLower().Trim() && c.Id != tagVm.Id.GetValueOrDefault()).ToList();
                    if (titleexist.Any())
                    {
                        ModelState.AddModelError("", "Topic Category already exist");
                        return PartialView("_PartialAddEdit", tagVm);
                    }

                    var tagmodel = await _topicCategoryQuery.GetAsync(tagVm.Id.GetValueOrDefault());
                    tagmodel.Title = tagVm.Title;
                    tagmodel.UpdatedBy = GetCurrentUserId();

                    await _topicCategoryCommand.UpdateAsync(tagmodel);
                    await _topicCategoryCommand.SaveChangesAsync();
                    _activityRepo.CreateActivityLog(string.Format("update Topic category with Title:{0}", tagmodel.Title), this.GetContollerName(), this.GetContollerName(), GetCurrentUserId(), tagmodel);

                    TempData["MESSAGE"] = "Topic category " + tagmodel.Title + " was successfully updated";
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