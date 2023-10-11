using System.Threading.Tasks;
using Danstagram.Common;
using Danstagram.Feed.Contracts;
using Danstagram.Interactions.Service.Entities;
using MassTransit;

namespace Danstagram.Interactions.Service.Consumers{
    public class FeedItemCreatedConsumer : RepositoryConsumer<FeedItem>, IConsumer<FeedItemCreated>
    {
        #region Constructors
        public FeedItemCreatedConsumer(IRepository<FeedItem> repository) : base(repository)
        {
        }
        #endregion

        #region Methods

        public async Task Consume(ConsumeContext<FeedItemCreated> context)
        {
            var message = context.Message;
            if((await repository.GetAsync(message.Id)) != null) return;

            await repository.CreateAsync(new FeedItem(){Id = message.Id});
        }
        
        #endregion
    }
}