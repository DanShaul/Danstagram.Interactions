using System;
using Danstagram.Common;

namespace Danstagram.Identity.Service.Entities{
    public class IdentityItem:IEntity{
        
        #region Properties

        public Guid Id{get;set;}
        
        public string UserName{get;set;}

        #endregion
    }
}