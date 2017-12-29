using log4net;
using SleekSoftMVCFramework.Data.AppEntities;
using SleekSoftMVCFramework.Data.Extension;
using SleekSoftMVCFramework.Data.IdentityModel;
using SleekSoftMVCFramework.Repository;
using SleekSoftMVCFramework.Repository.CoreRepositories;
using SleekSoftMVCFramework.Utilities;
using SleekSoftMVCFramework.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SleekSoftMVCFramework.Controllers
{
    public class PostController : BaseController
    {
        private readonly IRepositoryQuery<Topic, int> _topicQuery;
        private readonly IRepositoryCommand<Topic, int> _topicCommand;
        private readonly IRepositoryQuery<Post, int> _postQuery;
        private readonly IRepositoryCommand<Post, int> _postCommand;
        private readonly IActivityLogRepositoryCommand _activityRepo;
        private readonly IRepositoryQuery<ApplicationUser, long> _applicationUser;
        private readonly ILog _log;
        private readonly Utility _utility;

        public PostController(IActivityLogRepositoryCommand activityRepo,
            IRepositoryCommand<Topic, int> topicCommand,
            Utility utility,
            IRepositoryQuery<Topic, int> topicQuery,
            IRepositoryQuery<Post, int> postQuery,
            IRepositoryCommand<Post, int> postCommand,
             IRepositoryQuery<ApplicationUser, long> applicationUser,
            ILog log)
        {

            _topicQuery = topicQuery;
            _topicCommand = topicCommand;
            _postQuery = postQuery;
            _postCommand = postCommand;
            _activityRepo = activityRepo;
            _utility = utility;
            _applicationUser = applicationUser;
            _log = log;
        }
        // GET: Post
        [Authorize]
        public async Task<ActionResult> Index(string code, int? p)
        {
            if (TempData["MESSAGE"] != null)
            {
                ViewBag.Msg = TempData["MESSAGE"] as string;
            }

            var pageIndex = p ?? 1;
            int amountToTake = 100;
            int pageSize = 10;
            long currentUserId = GetCurrentUserId();
            int TopicId = ExtentionUtility.DecryptIntID(code);
            var posts = _postQuery.ExecuteStoredProdure<PostViewModel>("SpGetAllPostByTopicIdByUserId  @TopicId,@UserId", new SqlParameter("TopicId", TopicId), new SqlParameter("UserId", currentUserId))
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var total = posts.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            var results=new  PagedList<PostViewModel>(posts, pageIndex, pageSize, total);
            var viewModel = new ActivePostsViewModel
            {
                Posts = posts,
                PageIndex = pageIndex,
                TotalCount = results.TotalCount,
                TotalPages = results.TotalPages,
                TopicViewModel= _postQuery.ExecuteStoredProdure<TopicViewModel>("SpGetTopicByTopicIdByUserId  @TopicId,@UserId", new SqlParameter("TopicId", TopicId), new SqlParameter("UserId", currentUserId)).SingleOrDefault(),
                PostCommentViewModel = new PostCommentViewModel { TopicId = TopicId }
        };
            return View(viewModel);
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> SavePostComment(PostCommentViewModel model)
        {
            long CurrentUserId = GetCurrentUserId();
            string HostIPAddress = string.Empty;
            try
            {
                HostIPAddress = Request.UserHostAddress;
            }
            catch { }
            try
            {
                Post postmodel = new Post();
                postmodel.TopicId = model.TopicId;
                postmodel.Content = model.PostComment;
                postmodel.IPAddress = HostIPAddress;
                postmodel.IsTopicStarter = false;
                postmodel.CreatedBy = CurrentUserId;
                await _postCommand.InsertAsync(postmodel);
                await _postCommand.SaveChangesAsync();
                if (postmodel.Id >= 1)
                {
                    NotifyNewTopics(model.TopicId,postmodel.Id);
                }
                ModelState.Clear();
                return RedirectToAction("Index", new { code = model.TopicId.EncryptIntID() });
            }
            catch (Exception exp)
            {
                _log.Error(exp);
                return View("Error");
            }
        }


        public ActionResult DeletePost(string  code)
        {
            try
            {
                int postId = ExtentionUtility.DecryptIntID(code);
                Post postmodel = _postQuery.Get(postId);
                int TopicId = postmodel.TopicId;
                postmodel.IsDeleted = true;
                _postCommand.Delete(postmodel);
                _postCommand.SaveChanges();
                TempData["MESSAGE"] = "Post was successfully deleted";
                return RedirectToAction("Index", new { code = TopicId.EncryptIntID() });

            }
            catch (Exception exp)
            {
                _log.Error(exp);
                return View("Error");
            }
        }


        private void NotifyNewTopics(int topicId,int currentpostId)
        {
            try
            {
                // Get all user that has posted to this topic 
                // remove the current user from the notification, don't want to notify yourself that you 
                var postUserToNotify = _postQuery.GetAllList(m=>m.TopicId== topicId && m.Id!=currentpostId).Select(x => x.CreatedBy).ToList();

                if (postUserToNotify.Any())
                {
                    //Get users details
                    var usersToNotify = _applicationUser.GetAllList(m => postUserToNotify.Contains(m.Id)) .ToList();
                    if (usersToNotify.Any())
                    {
                        var TopicDetails = _topicQuery.Get(topicId);
                        var currentPostDetails = _postQuery.Get(currentpostId);
                        ApplicationUser ApplicationUserPostedBy = _applicationUser.Get(currentPostDetails.CreatedBy.GetValueOrDefault());
                        foreach(ApplicationUser muser in usersToNotify)
                        {
                            // Create the email
                            _utility.SendDiscussionNotificationEmail(muser, TopicDetails.Title,currentPostDetails.Content, ApplicationUserPostedBy.UserName);
                        }
                       
                    }

                }


            }
            catch (Exception)
            {
            }


        }


        public async Task<ActionResult> Edit(int id)
        {
            EditViewBagParams();
            try
            {
                if (id <= 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var postmodel = await _postQuery.GetAsync(id);
                if (postmodel == null)
                {
                    return HttpNotFound();
                }
                var postVm = new EditPostViewModel()
                {
                    Id = postmodel.Id,
                    PostComment = postmodel.Content
                };

                return PartialView("_EditPost", postVm);
            }
            catch (Exception exp)
            {
                _log.Error(exp);
                return View("Error");
            }

        }

        // POST: Class/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(int id, EditPostViewModel Vm)
        {
            try
            {
                if (id <= 0)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if (ModelState.IsValid)
                {
                    var postmodel = await _postQuery.GetAsync(id);
                    if (postmodel != null)
                    {
                        postmodel.Content = Vm.PostComment;
                        _postCommand.Update(postmodel);
                        _postCommand.SaveChanges();
                    }
                    TempData["MESSAGE"] = "Post  was successfully updated";
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
                    return PartialView("_EditPost", Vm);
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