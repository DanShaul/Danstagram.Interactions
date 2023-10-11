using System.Threading.Tasks;
using Danstagram.Common;
using Danstagram.Identities.Contracts;
using Danstagram.Interactions.Service.Entities;
using MassTransit;

namespace Danstagram.Interactions.Service.Consumers{
    public class IdentityCreatedConsumer : RepositoryConsumer<Identity>,IConsumer<IdentityCreated>
    {
        #region Constructors
        public IdentityCreatedConsumer(IRepository<Identity> repository) : base(repository)
        {
        }
        #endregion


        #region Methods
        public async Task Consume(ConsumeContext<IdentityCreated> context)
        {
            
            var message = context.Message;
            if ((await repository.GetAsync(message.Id)) != null) return;

            await this.repository.CreateAsync(new Identity{Id = message.Id,UserName = message.UserName});
        }
        #endregion
    }
}