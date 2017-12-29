using log4net;
using SleekSoftMVCFramework.Data.AppEntities;
using SleekSoftMVCFramework.Data.Constant;
using SleekSoftMVCFramework.Data.Extension;
using SleekSoftMVCFramework.Repository;
using SleekSoftMVCFramework.Repository.CoreRepositories;
using SleekSoftMVCFramework.Utilities;
using SleekSoftMVCFramework.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SleekSoftMVCFramework.Controllers
{
    public class TopicController : BaseController
    {

        private readonly IRepositoryQuery<Topic, int> _topicQuery;
        private readonly IRepositoryCommand<Topic, int> _topicCommand;
        private readonly IRepositoryQuery<Post, int> _postQuery;
        private readonly IRepositoryCommand<Post, int> _postCommand;
        private readonly IActivityLogRepositoryCommand _activityRepo;
        private readonly ILog _log;
        private readonly Utility _utility;

        public TopicController(IActivityLogRepositoryCommand activityRepo,
            IRepositoryCommand<Topic, int> topicCommand, 
            Utility utility, 
            IRepositoryQuery<Topic, int> topicQuery,
            IRepositoryQuery<Post, int> postQuery,
            IRepositoryCommand<Post, int> postCommand,
            ILog log)
        {

            _topicQuery = topicQuery;
            _topicCommand = topicCommand;
            _postQuery = postQuery;
            _postCommand = postCommand;
            _activityRepo = activityRepo;
            _utility = utility;
            _log = log;
        }
        // GET: Topic
        [Authorize]
        public async Task<ActionResult> Index()
        {
            if (TempData["MESSAGE"] != null)
            {
                ViewBag.Msg = TempData["MESSAGE"] as string;
            }
            if (User.IsInRole(AppConstant.AdminRole))
            {
                return View(await _topicQuery.ExecuteStoredProdure<TopicViewModel>("SpGetAllTopic").ToListAsync());
            }
            else
            {
                return View(await _topicQuery.ExecuteStoredProdure<TopicViewModel>("SpGetAllTopicByUserId  @UserId", new SqlParameter("UserId",GetCurrentUserId())).ToListAsync());
            }
           
        }

        [Authorize]
        public ActionResult Create()
        {
            CreateViewBagParams();
            return PartialView("_PartialAddEdit", new TopicViewModel { Categorys= _utility.GetTopicCategorys() });
        }

        // POST: Class/Create
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Create(TopicViewModel tagVm,HttpPostedFileBase profileImage)
        {
            long CurrentUserId  = GetCurrentUserId();
            tagVm.Categorys = _utility.GetTopicCategorys();
            string profilePath = string.Empty;
            string HostIPAddress = string.Empty;
            try
            {
                HostIPAddress = Request.UserHostAddress;
            }
            catch { }
           
            try
            {
                CreateViewBagParams();
                if (ModelState.IsValid)
                {
                    if (profileImage != null && profileImage.ContentLength > 0)
                    {
                        var ext = Path.GetExtension(profileImage.FileName).Trim().ToLower();
                        string[] allowedExtension = new string[] { ".jpeg", ".jpg", ".png" };
                        if (allowedExtension.Contains(ext))
                        {
                            profilePath = _utility.Upload(profileImage, ExtentionUtility.GetAppSetting("AppUploadFolder"));

                        }
                        else
                        {
                            ModelState.AddModelError("", string.Format("Invalid image extension,allowed extension are: .jpeg,.jpg,.png ", allowedExtension));
                            return PartialView("_PartialAddEdit", tagVm);
                        }
                    }


                    var topicexist = _topicQuery.GetAllList(c => c.Title.ToLower().Trim() == tagVm.Title.ToLower().Trim() && c.CategoryId==tagVm.CategoryId && c.CreatedBy== CurrentUserId).ToList();
                    if (topicexist.Any())
                    {
                        ModelState.AddModelError("", "Topic already exist");
                        return PartialView("_PartialAddEdit", tagVm);
                    }
                    var topicmodel = new Topic()
                    {
                        Title = tagVm.Title,
                        Description=tagVm.Description,
                        CategoryId=tagVm.CategoryId,
                        TopicImage= Path.GetFileName(profilePath),
                        CreatedBy = CurrentUserId
                    };

                    var topicPost = new Post()
                    {
                        Content = tagVm.PostContent,
                        IPAddress = HostIPAddress,
                        IsTopicStarter=true,
                        CreatedBy = CurrentUserId
                    };
                    await _topicCommand.InsertAsync(topicmodel);
                    await _topicCommand.SaveChangesAsync();

                    topicPost.TopicId = topicmodel.Id;
                    await _postCommand.InsertAsync(topicPost);
                    await _postCommand.SaveChangesAsync();
                    _activityRepo.CreateActivityLog(string.Format("Created Topic with Title:{0}", tagVm.Title), this.GetContollerName(), this.GetContollerName(), CurrentUserId, topicmodel);

                    TempData["MESSAGE"] = "Topic " + tagVm.Title + " was successfully created";
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


        [Authorize]
        public ActionResult CreateTopic()
        {
            CreateViewBagParams();
            return View(new TopicViewModel { Categorys = _utility.GetTopicCategorys() });
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateTopic(TopicViewModel tagVm, HttpPostedFileBase profileImage)
        {
            long CurrentUserId = GetCurrentUserId();
            tagVm.Categorys = _utility.GetTopicCategorys();
            string profilePath = string.Empty;
            string HostIPAddress = string.Empty;
            try
            {
                HostIPAddress = Request.UserHostAddress;
            }
            catch { }

            try
            {
                CreateViewBagParams();
                if (ModelState.IsValid)
                {
                    if (profileImage != null && profileImage.ContentLength > 0)
                    {
                        var ext = Path.GetExtension(profileImage.FileName).Trim().ToLower();
                        string[] allowedExtension = new string[] { ".jpeg", ".jpg", ".png" };
                        if (allowedExtension.Contains(ext))
                        {
                            profilePath = _utility.Upload(profileImage, ExtentionUtility.GetAppSetting("AppUploadFolder"));

                        }
                        else
                        {
                            ModelState.AddModelError("", string.Format("Invalid image extension,allowed extension are: .jpeg,.jpg,.png ", allowedExtension));
                            return View(tagVm);
                        }
                    }


                    var topicexist = _topicQuery.GetAllList(c => c.Title.ToLower().Trim() == tagVm.Title.ToLower().Trim() && c.CategoryId == tagVm.CategoryId && c.CreatedBy == CurrentUserId).ToList();
                    if (topicexist.Any())
                    {
                        ModelState.AddModelError("", "Topic already exist");
                        return View(tagVm);
                    }
                    var topicmodel = new Topic()
                    {
                        Title = tagVm.Title,
                        Description = tagVm.Description,
                        CategoryId = tagVm.CategoryId,
                        TopicImage = Path.GetFileName(profilePath),
                        CreatedBy = CurrentUserId
                    };

                    var topicPost = new Post()
                    {
                        Content = tagVm.PostContent,
                        IPAddress = HostIPAddress,
                        IsTopicStarter = true,
                        CreatedBy = CurrentUserId
                    };
                    await _topicCommand.InsertAsync(topicmodel);
                    await _topicCommand.SaveChangesAsync();

                    topicPost.TopicId = topicmodel.Id;
                    await _postCommand.InsertAsync(topicPost);
                    await _postCommand.SaveChangesAsync();
                    _activityRepo.CreateActivityLog(string.Format("Created Topic with Title:{0}", tagVm.Title), this.GetContollerName(), this.GetContollerName(), CurrentUserId, topicmodel);

                    TempData["MESSAGE"] = "Topic " + tagVm.Title + " was successfully created";
                    ModelState.Clear();
                    return RedirectToAction("Index", "Home");
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
                    return View(tagVm);
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
                var topicmodel = await _topicQuery.GetAsync(id);
                if (topicmodel == null)
                {
                    return HttpNotFound();
                }
                var starterPost = _postQuery.ExecuteStoredProdure<TopicPostContentVm>("SpGetTopicContentByTopicId @TopicId", new SqlParameter("TopicId", topicmodel.Id)).SingleOrDefault() ?? new TopicPostContentVm();
               
                var tagVm = new TopicViewModel()
                {
                    Id = topicmodel.Id,
                    Title = topicmodel.Title,
                    Description = topicmodel.Description,
                    PostContent = starterPost.Content,
                    TopicImage = topicmodel.TopicImage,
                    CategoryId = topicmodel.CategoryId,
                    Categorys = _utility.GetTopicCategorys()

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
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Edit(int id, TopicViewModel tagVm, HttpPostedFileBase profileImage)
        {
            long CurrentUserId = GetCurrentUserId();
            tagVm.Categorys = _utility.GetTopicCategorys();
            string profilePath = string.Empty;
            string HostIPAddress = string.Empty;
            try
            {
                HostIPAddress = Request.UserHostAddress;
            }
            catch { }

            try
            {
                if (tagVm.Id <= 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if (ModelState.IsValid)
                {

                    if (profileImage != null && profileImage.ContentLength > 0)
                    {
                        var ext = Path.GetExtension(profileImage.FileName).Trim().ToLower();
                        string[] allowedExtension = new string[] { ".jpeg", ".jpg", ".png" };
                        if (allowedExtension.Contains(ext))
                        {
                            profilePath = _utility.Upload(profileImage, ExtentionUtility.GetAppSetting("AppUploadFolder"));

                        }
                        else
                        {
                            ModelState.AddModelError("", string.Format("Invalid image extension,allowed extension are: .jpeg,.jpg,.png ", allowedExtension));
                            return PartialView("_PartialAddEdit", tagVm);
                        }
                    }
                    var topicexist = _topicQuery.GetAllList(c => c.Title.ToLower().Trim() == tagVm.Title.ToLower().Trim() && c.CategoryId == tagVm.CategoryId && c.CreatedBy == CurrentUserId && c.Id!=tagVm.Id.GetValueOrDefault()).ToList();
                    if (topicexist.Any())
                    {
                        ModelState.AddModelError("", "Topic already exist");
                        return PartialView("_PartialAddEdit", tagVm);
                    }

                    var topicmodel = await _topicQuery.GetAsync(tagVm.Id.GetValueOrDefault());
                    topicmodel.Title = tagVm.Title;
                    topicmodel.UpdatedBy = GetCurrentUserId();
                    topicmodel.Description = tagVm.Description;
                    topicmodel.CategoryId = tagVm.CategoryId;
                    topicmodel.TopicImage = Path.GetFileName(profilePath);
                    topicmodel.UpdatedBy = CurrentUserId;
                    await _topicCommand.UpdateAsync(topicmodel);
                    await _topicCommand.SaveChangesAsync();
                    _activityRepo.CreateActivityLog(string.Format("update Topic with Title:{0}", topicmodel.Title), this.GetContollerName(), this.GetContollerName(), CurrentUserId, topicmodel);

                    TempData["MESSAGE"] = "Topic " + topicmodel.Title + " was successfully updated";
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


        #region MyLastestTopicsRegion
        [ChildActionOnly]
        public ActionResult LatestTopics(int? p)
        {
            // Set the page index
            var pageIndex = p ?? 1;
            int amountToTake = 100;
            var total = _topicQuery.Count(); 
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics
            var topics = GetRecentTopics(pageIndex,3,100);
            if (topics.Any())
            {
                foreach(TopicViewModel topic in topics)
                {
                    List<PostViewModel> AllPost = _postQuery.ExecuteStoredProdure<PostViewModel>("SpGetAllPostByTopicId  @TopicId", new SqlParameter("TopicId", topic.Id)).ToList();
                    topic.LastPost = _postQuery.ExecuteStoredProdure<LastPost>("SpGetLastPostUserByTopicId  @TopicId", new SqlParameter("TopicId", topic.Id)).SingleOrDefault();
                    topic.StarterPost = AllPost.Where(m => m.IsTopicStarter == true).SingleOrDefault();
                    topic.Posts = AllPost.Where(m => m.IsTopicStarter == false).ToList();
                }
            }
            // create the view model
            var viewModel = new ActiveTopicsViewModel
            {
                Topics = topics,
                PageIndex = pageIndex,
                TotalCount = topics.TotalCount,
                TotalPages = topics.TotalPages
            };

            return PartialView(viewModel);
        }


        public PagedList<TopicViewModel> GetRecentTopics(int pageIndex, int pageSize, int amountToTake)
        {

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _topicQuery.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _topicQuery.ExecuteStoredProdure<TopicViewModel>("SpGetAllTopic")
                        .OrderByDescending(x=>x.DateCreated)
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            // Return a paged list
            return new PagedList<TopicViewModel>(results, pageIndex, pageSize, total);
        }

        public List<TopicViewModel> CreateTopicViewModels(List<Topic> topics)
        {
            // Get all topic Ids
            var topicIds = topics.Select(x => x.Id).ToList();

            // Gets posts for topics
            var posts = _postQuery.GetAllList(x => topicIds.Contains(x.TopicId))
                        .OrderByDescending(x => x.DateCreated)
                        .ToList();

            var groupedPosts = posts.ToLookup(x => x.TopicId);

            // Create the view models
            var viewModels = new List<TopicViewModel>();
            foreach (var topic in topics)
            {
                var id = topic.Id;
                var topicPosts = (groupedPosts.Contains(id) ? groupedPosts[id].ToList() : new List<Post>());

               // viewModels.Add(CreateTopicViewModel(topic, permission, topicPosts, null, null, null, null, loggedOnUser, settings));
            }
            return viewModels;
        }

        #endregion
        #region MyTodaysTopicRegion
        public ActionResult TodaysTopic(int? p)
        {
            // Set the page index
            var pageIndex = p ?? 1;
            int amountToTake = 100;
            var total = _topicQuery.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics
            var topics = GetTodaysTopic(pageIndex,10, 100);
            if (topics.Any())
            {
                foreach (TopicViewModel topic in topics)
                {
                    List<PostViewModel> AllPost = _postQuery.ExecuteStoredProdure<PostViewModel>("SpGetAllPostByTopicId  @TopicId", new SqlParameter("TopicId", topic.Id)).ToList();
                    topic.LastPost = _postQuery.ExecuteStoredProdure<LastPost>("SpGetLastPostUserByTopicId  @TopicId", new SqlParameter("TopicId", topic.Id)).SingleOrDefault();
                    topic.StarterPost = AllPost.Where(m => m.IsTopicStarter == true).SingleOrDefault();
                    topic.Posts = AllPost.Where(m => m.IsTopicStarter == false).ToList();
                }
            }
            // create the view model
            var viewModel = new ActiveTopicsViewModel
            {
                Topics = topics,
                PageIndex = pageIndex,
                TotalCount = topics.TotalCount,
                TotalPages = topics.TotalPages
            };

            return PartialView(viewModel);
        }


        public PagedList<TopicViewModel> GetTodaysTopic(int pageIndex, int pageSize, int amountToTake)
        {

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _topicQuery.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _topicQuery.ExecuteStoredProdure<TopicViewModel>("SpGetAllTopicCeatedToday")
                        .OrderByDescending(x => x.DateCreated)
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            // Return a paged list
            return new PagedList<TopicViewModel>(results, pageIndex, pageSize, total);
        }

        #endregion


        #region MyTodaysTopicRegion
        public ActionResult HighestViewedTopics(int? p)
        {
            // Set the page index
            var pageIndex = p ?? 1;
            int amountToTake = 100;
            var total = _topicQuery.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics
            var topics = GetHighestViewedTopics(pageIndex,10, 100);
            if (topics.Any())
            {
                foreach (TopicViewModel topic in topics)
                {
                    List<PostViewModel> AllPost = _postQuery.ExecuteStoredProdure<PostViewModel>("SpGetAllPostByTopicId  @TopicId", new SqlParameter("TopicId", topic.Id)).ToList();
                    topic.LastPost = _postQuery.ExecuteStoredProdure<LastPost>("SpGetLastPostUserByTopicId  @TopicId", new SqlParameter("TopicId", topic.Id)).SingleOrDefault();
                    topic.StarterPost = AllPost.Where(m => m.IsTopicStarter == true).SingleOrDefault();
                    topic.Posts = AllPost.Where(m => m.IsTopicStarter == false).ToList();
                }
            }
            // create the view model
            var viewModel = new ActiveTopicsViewModel
            {
                Topics = topics,
                PageIndex = pageIndex,
                TotalCount = topics.TotalCount,
                TotalPages = topics.TotalPages
            };

            return PartialView(viewModel);
        }


        public PagedList<TopicViewModel> GetHighestViewedTopics(int pageIndex, int pageSize, int amountToTake)
        {

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _topicQuery.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _topicQuery.ExecuteStoredProdure<TopicViewModel>("SpGetAllMostViewedTopic")
                        .OrderByDescending(x => x.DateCreated)
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            // Return a paged list
            return new PagedList<TopicViewModel>(results, pageIndex, pageSize, total);
        }

        #endregion

        #region MyTopicByCategoryRegion
        //TopicByCategory

        public ActionResult TopicByCategory(string code, int? p)
        {
            int topicCategoryId = code.DecryptIntID();
            // Set the page index
            var pageIndex = p ?? 1;
            int amountToTake = 100;
            var total = _topicQuery.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics
            var topics = GetTopicByCategory(topicCategoryId,pageIndex, 10, 100);
            if (topics.Any())
            {
                foreach (TopicViewModel topic in topics)
                {
                    List<PostViewModel> AllPost = _postQuery.ExecuteStoredProdure<PostViewModel>("SpGetAllPostByTopicId  @TopicId", new SqlParameter("TopicId", topic.Id)).ToList();
                    topic.LastPost = _postQuery.ExecuteStoredProdure<LastPost>("SpGetLastPostUserByTopicId  @TopicId", new SqlParameter("TopicId", topic.Id)).SingleOrDefault();
                    topic.StarterPost = AllPost.Where(m => m.IsTopicStarter == true).SingleOrDefault();
                    topic.Posts = AllPost.Where(m => m.IsTopicStarter == false).ToList();
                }
            }
            // create the view model
            var viewModel = new ActiveTopicsViewModel
            {
                Topics = topics,
                PageIndex = pageIndex,
                TotalCount = topics.TotalCount,
                TotalPages = topics.TotalPages
            };

            return PartialView(viewModel);
        }


        public PagedList<TopicViewModel> GetTopicByCategory(int categoryId,int pageIndex, int pageSize, int amountToTake)
        {

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _topicQuery.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _topicQuery.ExecuteStoredProdure<TopicViewModel>("SpGetAllTopicByCategoryId  @CategoryId", new SqlParameter("CategoryId", categoryId))
                        .OrderByDescending(x => x.DateCreated)
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            // Return a paged list
            return new PagedList<TopicViewModel>(results, pageIndex, pageSize, total);
        }

        #endregion
    }
}