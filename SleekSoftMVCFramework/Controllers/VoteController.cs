using log4net;
using SleekSoftMVCFramework.Data.AppEntities;
using SleekSoftMVCFramework.Repository;
using SleekSoftMVCFramework.Repository.CoreRepositories;
using SleekSoftMVCFramework.Utilities;
using SleekSoftMVCFramework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SleekSoftMVCFramework.Controllers
{
    public class VoteController : BaseController
    {
        private readonly IRepositoryQuery<PostLike, int> _postLikeQuery;
        private readonly IRepositoryCommand<PostLike, int> _postLikeCommand;
        private readonly IRepositoryCommand<FollowTopic, int> _followTopicCommand;
        private readonly IRepositoryQuery<FollowTopic, int> _followTopicQuery;
        private readonly IRepositoryQuery<Post, int> _postQuery;
        private readonly IRepositoryCommand<Post, int> _postCommand;
        private readonly IActivityLogRepositoryCommand _activityRepo;
        private readonly ILog _log;
        private readonly Utility _utility;

        public VoteController(IActivityLogRepositoryCommand activityRepo,
            IRepositoryCommand<PostLike, int> postLikeCommand,
            Utility utility,
            IRepositoryQuery<PostLike, int> postLikeQuery,
            IRepositoryQuery<Post, int> postQuery,
            IRepositoryCommand<Post, int> postCommand,
            IRepositoryCommand<FollowTopic, int> followTopicCommand,
            IRepositoryQuery<FollowTopic, int> followTopicQuery,
        ILog log)
        {

            _postLikeQuery = postLikeQuery;
            _postLikeCommand = postLikeCommand;
            _postQuery = postQuery;
            _postCommand = postCommand;
            _activityRepo = activityRepo;
            _utility = utility;
            _followTopicCommand = followTopicCommand;
            _followTopicQuery = followTopicQuery;
            _log = log;
        }
        // GET: Vote
        [HttpPost]
        [Authorize]
        public void VoteUpPost(VoteViewModel voteUpViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    //get the post details
                    long currentuserId = GetCurrentUserId();
                    Post postmodel = _postQuery.Get(voteUpViewModel.PostId);
                    if (postmodel != null)
                    {
                        // Check this user is not the post owner
                        if (currentuserId != postmodel.CreatedBy)
                        {
                            PostLike postlikemodel = new PostLike();
                            postlikemodel.PostId = postmodel.Id;
                            postlikemodel.TopicId = postmodel.TopicId;
                            postlikemodel.CreatedBy = currentuserId;

                            postmodel.VoteCount = postmodel.VoteCount++;
                            _postCommand.Update(postmodel);

                            _postLikeCommand.Insert(postlikemodel);
                            _postLikeCommand.SaveChanges();
                        }
                         
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }

        [HttpPost]
        [Authorize]
        public void VoteDownPost(VoteViewModel voteUpViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    //get the post details
                    Post postmodel = _postQuery.Get(voteUpViewModel.PostId);
                    long currentuserId = GetCurrentUserId();
                    if (postmodel != null)
                    {
                        if (currentuserId != postmodel.CreatedBy)
                        {
                            PostLike postlikemodel = _postLikeQuery.GetAllList(m => m.PostId == postmodel.Id && m.TopicId == postmodel.TopicId && m.CreatedBy == currentuserId).SingleOrDefault();
                            if (postlikemodel != null)
                            {
                                postmodel.VoteCount = postmodel.VoteCount--;
                                _postCommand.Update(postmodel);


                                _postLikeCommand.Delete(postlikemodel);
                                _postLikeCommand.SaveChanges();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }
        }

        [HttpPost]
        [Authorize]
        public void FollowTopic(FollowViewModel followViewModel)
        {
            long currentuserId = GetCurrentUserId();
            if (Request.IsAjaxRequest())
            {
                try
                {
                    FollowTopic followtopicmodel = new FollowTopic();
                    followtopicmodel.TopicId = followViewModel.TopicId;
                    followtopicmodel.UserId= currentuserId;
                    followtopicmodel.CreatedBy = currentuserId;
                    _followTopicCommand.Insert(followtopicmodel);
                    _followTopicCommand.SaveChanges();
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }

        }


        [HttpPost]
        [Authorize]
        public void UnFollowTopic(FollowViewModel followViewModel)
        {
            long currentuserId = GetCurrentUserId();
            if (Request.IsAjaxRequest())
            {
                try
                {

                    FollowTopic followtopicmodel = _followTopicQuery.GetAllList(m=>m.TopicId== followViewModel.TopicId && m.UserId==currentuserId).FirstOrDefault();
                    followtopicmodel.IsDeleted = true;
                    _followTopicCommand.Delete(followtopicmodel);
                    _followTopicCommand.SaveChanges();
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            }

        }



    }
}