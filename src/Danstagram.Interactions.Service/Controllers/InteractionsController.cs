using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Danstagram.Common;
using Danstagram.Interactions.Service.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Danstagram.Interactions.Service.Controllers{
    [ApiController]
    [Route("interactions")]
    public class InteractionsController : ControllerBase{
        #region Cosntructors
        public InteractionsController(IRepository<FeedItem> feedItemsRepository,IRepository<Comment> commentsRepository,IRepository<Like> likesRepository,IRepository<Identity> identitiesRepository){

            this.feedItemsRepository = feedItemsRepository;
            this.commentsRepository = commentsRepository;
            this.likesRespository = likesRepository;
            this.identitiesRepository = identitiesRepository;

        }
        #endregion
        #region Properties
        private readonly IRepository<FeedItem> feedItemsRepository;
        private readonly IRepository<Comment> commentsRepository;
        private readonly IRepository<Like> likesRespository;
        private readonly IRepository<Identity> identitiesRepository;

        #endregion
        #region Methods

        [Route("likes/feeditems/{id}")]
        [HttpGet]
        public async Task<IEnumerable<LikeDto>> GetLikesByFeedItemsIdAsyc(Guid feedItemId){
            
            return await likesRespository.GetAllAsync(like => like.FeedItemId == feedItemId);
        }

        [Route("comments/feeditems/{id}")]
        [HttpGet]
        public async Task<IEnumerable<CommentDto>> GetCommentsByFeedItemsIdAsyc(Guid feedItemId){
            return (await commentsRepository.GetAllAsync(comment => comment.FeedItemId == feedItemId)).AsDto();
        }

        [Route("comments")]
        [HttpPost]
        public 

        #endregion
    }
}