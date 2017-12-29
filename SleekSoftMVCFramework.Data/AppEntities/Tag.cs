﻿using SleekSoftMVCFramework.Data.EntityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SleekSoftMVCFramework.Data.AppEntities
{
   public  class Tag : BaseEntityWithAudit<int>
    {
        public string Title { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }
    }
}
