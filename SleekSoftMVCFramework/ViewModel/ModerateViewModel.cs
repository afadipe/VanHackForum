using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SleekSoftMVCFramework.ViewModel
{
    public class ModerateViewModel
    {
        public IList<TopicViewModel> Topics { get; set; }
        public IList<PostViewModel> Posts { get; set; }
    }
}