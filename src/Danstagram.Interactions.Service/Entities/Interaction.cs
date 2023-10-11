using System;
using Danstagram.Common;

namespace Danstagram.Interactions.Service.Entities{
    public abstract class Interaction : IEntity{
        #region Properties
        public Guid Id{get;set;}
        public Guid UserId{get;set;}
        public Guid FeedItemId{get;set;}
        #endregion
    }
}