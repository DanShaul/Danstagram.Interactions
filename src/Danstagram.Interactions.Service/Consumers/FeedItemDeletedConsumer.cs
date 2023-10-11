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
        public FeedItemDeletedConsumer(IRepository<FeedItem> repository,IRepository<Like> likesRepository,IRepository<Comment> commentsRepository) : base(repository)
        {
            this.likesRepository = likesRepository;
            this.commentsRepository = commentsRepository;
        }
        #endregion

        #region Properties
        private readonly IRepository<Like> likesRepository;
        private readonly IRepository<Comment> commentsRepository;
        #endregion

        #region Methods

        public async Task Consume(ConsumeContext<FeedItemDeleted> context)
        {
            var message = context.Message;
            if((await repository.GetAsync(message.Id)) == null) return;

            await repository.RemoveAsync(message.Id);
            
            var likes = await likesRepository.GetAllAsync(like => like.FeedItemId == message.Id);
            var comments = await commentsRepository.GetAllAsync(comment => comment.FeedItemId == message.Id);

            foreach(var like in likes) {await likesRepository.RemoveAsync(like.Id);}
            foreach(var comment in comments) {await commentsRepository.RemoveAsync(comment.Id);}
        }
        
        #endregion
    }
}