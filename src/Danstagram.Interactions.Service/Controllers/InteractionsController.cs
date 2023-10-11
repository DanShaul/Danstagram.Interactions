using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danstagram.Common;
using Danstagram.Interactions.Contracts;
using Danstagram.Interactions.Service.Entities;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;

namespace Danstagram.Interactions.Service.Controllers{
    [ApiController]
    [Route("interactions")]
    public class InteractionsController : ControllerBase{
        #region Cosntructors
        public InteractionsController(IRepository<FeedItem> feedItemsRepository,IRepository<Comment> commentsRepository,IRepository<Like> likesRepository,IRepository<Identity> identitiesRepository, IPublishEndpoint publishEndpoint){

            this.feedItemsRepository = feedItemsRepository;
            this.commentsRepository = commentsRepository;
            this.likesRepository = likesRepository;
            this.identitiesRepository = identitiesRepository;
            this.publishEndpoint = publishEndpoint;

        }
        #endregion

        #region Properties
        private readonly IRepository<FeedItem> feedItemsRepository;
        private readonly IRepository<Comment> commentsRepository;
        private readonly IRepository<Like> likesRepository;
        private readonly IRepository<Identity> identitiesRepository;
        private readonly IPublishEndpoint publishEndpoint;

        #endregion
        #region Methods

        [Route("likes/feeditems/{id}")]
        [HttpGet]
        public async Task<IEnumerable<LikeDto>> GetLikesByFeedItemsIdAsync(Guid feedItemId){
            
            return (await likesRepository.GetAllAsync(like => like.FeedItemId == feedItemId)).Select(like => like.AsDto());
        }

        [Route("likes/{id}")]
        [HttpGet]
        public async Task<ActionResult<LikeDto>> GetLikeByIdAsync(Guid likeId){
            var like = await likesRepository.GetAsync(likeId);

            if (like == null) return NotFound();

            return like.AsDto();

        }


        [Route("comments/{id}")]
        [HttpGet]
        public async Task<ActionResult<CommentDto>> GetCommentByIdAsync(Guid commentId){
            var comment = await commentsRepository.GetAsync(commentId);

            if (comment == null) return NotFound();

            var userName = (await identitiesRepository.GetAsync(comment.UserId)).UserName;

            var commentDto = new CommentDto(
                comment.Id,
                userName,
                comment.FeedItemId,
                comment.Message,
                comment.CreatedDate);

            return commentDto;

        }

        [Route("comments/feeditems/{id}")]
        [HttpGet]
        public async Task<IEnumerable<CommentDto>> GetCommentsByFeedItemsIdAsync(Guid feedItemId){
            var comments = await commentsRepository.GetAllAsync(comment => comment.FeedItemId == feedItemId);

            var commentUserIds = comments.Select(x => x.UserId);
            var identities = await identitiesRepository.GetAllAsync(identity => commentUserIds.Contains(identity.Id));


            var feedComments = comments.Select(comment => new CommentDto(
                comment.Id,
                identities.SingleOrDefault(Identity => Identity.Id == comment.UserId).UserName,
                comment.FeedItemId,
                comment.Message,
                comment.CreatedDate));

            return feedComments;

        }

        [Route("comments")]
        [HttpPost]
        public async Task<ActionResult<CommentDto>> PostCommentAsync(CreateCommentDto createComment){
            Comment comment = new(){
                Id = Guid.NewGuid(),
                CreatedDate = DateTimeOffset.UtcNow,
                FeedItemId = createComment.FeedItemId,
                Message = createComment.Message,
                UserId = createComment.UserId
            };
            var validationResponse = await ValidateInteractionAsync<Comment>(comment);
            if (validationResponse != null) return validationResponse;

            await commentsRepository.CreateAsync(comment);
            await publishEndpoint.Publish(new CommentCreated(comment.Id,comment.UserId,comment.FeedItemId,comment.Message,comment.CreatedDate));

            return CreatedAtAction(nameof(GetCommentByIdAsync),new{id = comment.Id},comment);
        }

        [Route("likes")]
        [HttpPost]
        public async Task<ActionResult<LikeDto>> PostLikeAsync(CreateLikeDto createLike){
            Like like = new(){
                Id = Guid.NewGuid(),
                FeedItemId = createLike.FeedItemId,
                UserId = createLike.UserId
            };

            var validationResponse = await ValidateInteractionAsync<Like>(like);
            if (validationResponse != null) return validationResponse;

            await likesRepository.CreateAsync(like);
            await publishEndpoint.Publish(new LikeCreated(like.Id,like.UserId,like.FeedItemId));

            return CreatedAtAction(nameof(GetLikeByIdAsync),new{Id = like.Id},like);
        }

        [Route("likes/{id}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteLikeAsync(Guid id) {
            if(likesRepository.GetAsync(id) == null) return NoContent();

            await likesRepository.RemoveAsync(id);
            await publishEndpoint.Publish(new LikeDeleted(id));

            return Ok();
        }

        [Route("comments/{id}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteCommentsAsync(Guid id) {
            if(commentsRepository.GetAsync(id) == null) return NoContent();

            await commentsRepository.RemoveAsync(id);
            await publishEndpoint.Publish(new CommentDeleted(id));
            
            return Ok();
        }
        private async Task<ActionResult> ValidateInteractionAsync<T>(T t) where T : Interaction{
            if((await feedItemsRepository.GetAsync(t.FeedItemId)) == null) return NotFound("Feed item not found");

            if((await identitiesRepository.GetAsync(t.UserId)) == null) return NotFound("User not found");

            return null;
        }
        #endregion
    }
}