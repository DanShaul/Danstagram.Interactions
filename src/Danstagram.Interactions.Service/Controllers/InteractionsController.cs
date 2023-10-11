using Danstagram.Common;
using Danstagram.Interactions.Service.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Danstagram.Interactions.Service.Controllers{
    [ApiController]
    
    public class InteractionsController : ControllerBase{
        #region Cosntructors
        public InteractionsController(){

        }
        #endregion
        #region Properties
        private readonly IRepository<FeedItem> feedItemsRepository;
        private readonly IRepository<Comment> commentsRepository;
        private readonly IRepository<Like> likesRespository;
        private readonly IRepository<Identity> identitiesRepository;
        #endregion
        #region Methods
        #endregion
    }
}