using System;

namespace Danstagram.Interactions.Service.Entities{
    public class Comment : Interaction {

        #region Properties
        public string Message{get;set;}
        public DateTimeOffset CreatedDate{get;set;}

        #endregion
    }
}