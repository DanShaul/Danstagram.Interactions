using System;
using Danstagram.Common;

namespace Danstagram.Interactions.Service.Entities{
    public class Identity:IEntity{
        
        #region Properties

        public Guid Id{get;set;}
        
        public string UserName{get;set;}

        #endregion
    }
}