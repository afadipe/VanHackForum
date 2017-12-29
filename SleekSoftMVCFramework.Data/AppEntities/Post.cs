using SleekSoftMVCFramework.Data.EntityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SleekSoftMVCFramework.Data.AppEntities
{

    public enum PostOrderBy
    {
        Standard,
        Newest,
        Votes
    }

    public class Post : BaseEntityWithAudit<int>
    {
        public Post()
        {
            IsTopicStarter = false;
        }
        public int TopicId { get; set; }

        public string IPAddress { get; set; }

        public string Content { get; set; }

        public int? VoteCount { get; set; }

        public bool IsTopicStarter { get; set; }
    }
}
