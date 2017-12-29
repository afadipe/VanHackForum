using SleekSoftMVCFramework.Data.EntityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SleekSoftMVCFramework.Data.AppEntities
{
   public class PostLike : BaseEntityWithAudit<int>
    {
        public int PostId { get; set; }

        public int TopicId { get; set; }
    }
}
