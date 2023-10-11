using System;
using Danstagram.Common;

namespace Danstagram.Identity.Service.Entities{
    public class Identity:IEntity{
        #region Properties
        
        public Guid Id{get;set;}
        
        public string UserName{get;set;}

        #endregion
    }
}