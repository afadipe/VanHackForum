using log4net;
using SleekSoftMVCFramework.Controllers;
using SleekSoftMVCFramework.Data.AppEntities;
using SleekSoftMVCFramework.Repository;
using SleekSoftMVCFramework.Repository.CoreRepositories;
using SleekSoftMVCFramework.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SleekSoftMVCFramework.Areas.Portal.Controllers
{
    public class ModerateController : BaseController
    {

        private readonly IRepositoryQuery<Topic, int> _topicQuery;
        private readonly IRepositoryCommand<Topic, int> _topicCommand;
        private readonly IRepositoryQuery<Post, int> _postQuery;
        private readonly IRepositoryCommand<Post, int> _postCommand;
        private readonly IActivityLogRepositoryCommand _activityRepo;
        private readonly ILog _log;
        private readonly Utility _utility;

        public ModerateController(IActivityLogRepositoryCommand activityRepo,
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

        // GET: Portal/Moderate
        public ActionResult Index()
        {
            return View();
        }
    }
}