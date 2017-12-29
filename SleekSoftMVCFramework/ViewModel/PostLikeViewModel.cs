using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SleekSoftMVCFramework.ViewModel
{
    public class PostLikeViewModel
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public int TopicId { get; set; }
        public long? CreatedBy { get; set; }
        public string DateCreated { get; set; }
        public string TimeCreated { get; set; }
        public string CreateByFullName { get; set; }
        public string CreateByUserName { get; set; }
        public string CreateByAvatar { get; set; }
    }
}