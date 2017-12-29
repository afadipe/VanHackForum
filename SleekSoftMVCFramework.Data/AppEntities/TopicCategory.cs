using SleekSoftMVCFramework.Data.EntityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SleekSoftMVCFramework.Data.AppEntities
{
   public  class TopicCategory : BaseEntityWithAudit<int>
    {
        public string Title { get; set; }
    }
}
