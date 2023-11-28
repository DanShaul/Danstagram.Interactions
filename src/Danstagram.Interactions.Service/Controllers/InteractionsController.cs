using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Danstagram.Common;
using Danstagram.Interactions.Contracts;
using Danstagram.Interactions.Service.Entities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Danstagram.Interactions.Service.Controllers
{
    [ApiController]
    [Route("interactions")]
    public class InteractionsController : ControllerBase
    {
        #region Cosntructors
        public InteractionsController(IRepository<FeedItem> feedItemsRepository, IRepository<Comment> commentsRepository, IRepository<Like> likesRepository, IRepository<Identity> identitiesRepository, IPublishEndpoint publishEndpoint)
        {

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
        [Route("~/health")]
        [HttpGet]
        public ActionResult GetHealth()
        {
            return Ok();
        }

        [Route("likes/feeditems/{id}")]
        [HttpGet]
        public async Task<IEnumerable<LikeDto>> GetLikesByFeedItemsIdAsync(Guid feedItemId)
        {

            return (await likesRepository.GetAllAsync(like => like.FeedItemId == feedItemId)).Select(like => like.AsDto());
        }

        [Route("likes/{id}")]
        [HttpGet]
        public async Task<ActionResult<LikeDto>> GetLikeByIdAsync(Guid likeId)
        {
            Like like = await likesRepository.GetAsync(likeId);

            return like == null ? (ActionResult<LikeDto>)NotFound() : (ActionResult<LikeDto>)like.AsDto();
        }


        [Route("comments/{id}")]
        [HttpGet]
        public async Task<ActionResult<CommentDto>> GetCommentByIdAsync(Guid commentId)
        {
            Comment comment = await commentsRepository.GetAsync(commentId);

            if (comment == null)
            {
                return NotFound();
            }

            string userName = (await identitiesRepository.GetAsync(comment.UserId)).UserName;

            CommentDto commentDto = new(
                comment.Id,
                userName,
                comment.FeedItemId,
                comment.Message,
                comment.CreatedDate);

            return commentDto;

        }

        [Route("comments/feeditems/{id}")]
        [HttpGet]
        public async Task<IEnumerable<CommentDto>> GetCommentsByFeedItemsIdAsync(Guid feedItemId)
        {
            IReadOnlyCollection<Comment> comments = await commentsRepository.GetAllAsync(comment => comment.FeedItemId == feedItemId);

            IEnumerable<Guid> commentUserIds = comments.Select(x => x.UserId);
            IReadOnlyCollection<Identity> identities = await identitiesRepository.GetAllAsync(identity => commentUserIds.Contains(identity.Id));


            IEnumerable<CommentDto> feedComments = comments.Select(comment => new CommentDto(
                comment.Id,
                identities.SingleOrDefault(Identity => Identity.Id == comment.UserId).UserName,
                comment.FeedItemId,
                comment.Message,
                comment.CreatedDate));

            return feedComments;

        }

        [Route("comments")]
        [HttpPost]
        public async Task<ActionResult<CommentDto>> PostCommentAsync(CreateCommentDto createComment)
        {
            Comment comment = new()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTimeOffset.UtcNow,
                FeedItemId = createComment.FeedItemId,
                Message = createComment.Message,
                UserId = createComment.UserId
            };
            ActionResult validationResponse = await ValidateInteractionAsync(comment);
            if (validationResponse != null)
            {
                return validationResponse;
            }

            await commentsRepository.CreateAsync(comment);
            await publishEndpoint.Publish(new CommentCreated(comment.Id, comment.UserId, comment.FeedItemId, comment.Message, comment.CreatedDate));

            return CreatedAtAction(nameof(GetCommentByIdAsync), new { id = comment.Id }, comment);
        }

        [Route("likes")]
        [HttpPost]
        public async Task<ActionResult<LikeDto>> PostLikeAsync(CreateLikeDto createLike)
        {
            Like like = new()
            {
                Id = Guid.NewGuid(),
                FeedItemId = createLike.FeedItemId,
                UserId = createLike.UserId
            };

            ActionResult validationResponse = await ValidateInteractionAsync(like);
            if (validationResponse != null)
            {
                return validationResponse;
            }

            await likesRepository.CreateAsync(like);
            await publishEndpoint.Publish(new LikeCreated(like.Id, like.UserId, like.FeedItemId));

            return CreatedAtAction(nameof(GetLikeByIdAsync), new { like.Id }, like);
        }

        [Route("likes/{id}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteLikeAsync(Guid id)
        {
            if (likesRepository.GetAsync(id) == null)
            {
                return NoContent();
            }

            await likesRepository.RemoveAsync(id);
            await publishEndpoint.Publish(new LikeDeleted(id));

            return Ok();
        }

        [Route("comments/{id}")]
        [HttpDelete]
        public async Task<ActionResult> DeleteCommentsAsync(Guid id)
        {
            if (commentsRepository.GetAsync(id) == null)
            {
                return NoContent();
            }

            await commentsRepository.RemoveAsync(id);
            await publishEndpoint.Publish(new CommentDeleted(id));

            return Ok();
        }
        private async Task<ActionResult> ValidateInteractionAsync<T>(T t) where T : Interaction
        {
            return (await feedItemsRepository.GetAsync(t.FeedItemId)) == null
            ? NotFound("Feed item not found")
            : (await identitiesRepository.GetAsync(t.UserId)) == null ? NotFound("User not found")
            : (t is Like && (await likesRepository.GetAsync((entity) => entity.FeedItemId == t.FeedItemId && entity.UserId == t.UserId)) != null) ? Ok("Like is already created") : (ActionResult)null;
        }
        #endregion
    }
}