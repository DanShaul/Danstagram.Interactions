using System.Threading.Tasks;
using Danstagram.Common;
using Danstagram.Feed.Contracts;
using Danstagram.Interactions.Service.Entities;
using MassTransit;
using MassTransit.Transports.Fabric;

namespace Danstagram.Interactions.Service.Consumers{
    public class FeedItemDeletedConsumer : RepositoryConsumer<FeedItem>, IConsumer<FeedItemDeleted>
    {
        #region Constructors
        public FeedItemDeletedConsumer(IRepository<FeedItem> repository,IRepository<Like> likesRepository,IRepository<Comment> commentsRepository, IPublishEndpoint publishEndpoint) : base(repository)
        {
            this.likesRepository = likesRepository;
            this.commentsRepository = commentsRepository;
            this.publishEndpoint = publishEndpoint;
        }
        #endregion

        #region Properties
        private readonly IRepository<Like> likesRepository;
        private readonly IRepository<Comment> commentsRepository;
        private readonly IPublishEndpoint publishEndpoint;
        #endregion

        #region Methods

        public async Task Consume(ConsumeContext<FeedItemDeleted> context)
        {
            var message = context.Message;
            if((await repository.GetAsync(message.Id)) == null) return;

            await repository.RemoveAsync(message.Id);
            
            var likes = await likesRepository.GetAllAsync(like => like.FeedItemId == message.Id);
            var comments = await commentsRepository.GetAllAsync(comment => comment.FeedItemId == message.Id);


            foreach(var like in likes) {
                await likesRepository.RemoveAsync(like.Id);
                publishEndpoint.Publish(new LikeDeleted(like.Id));
                }

                
            foreach(var comment in comments) {
                await commentsRepository.RemoveAsync(comment.Id);
                publishEndpoint.Publish(new CommentDeleted(comment.Id));
                }
        }
        
        #endregion
    }
}