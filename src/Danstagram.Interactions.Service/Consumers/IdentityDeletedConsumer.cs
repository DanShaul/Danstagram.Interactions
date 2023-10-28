using System.Threading.Tasks;
using Danstagram.Common;
using Danstagram.Identities.Contracts;
using Danstagram.Interactions.Contracts;
using Danstagram.Interactions.Service.Entities;
using MassTransit;

namespace Danstagram.Interactions.Service.Consumers
{
  public class IdentityDeletedConsumer : RepositoryConsumer<Identity>, IConsumer<IdentityDeleted>
  {

    #region Constructors
    public IdentityDeletedConsumer(IRepository<Identity> repository, IRepository<Like> likesRepository, IRepository<Comment> commentsRepository, IPublishEndpoint publishEndpoint) : base(repository)
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
    public async Task Consume(ConsumeContext<IdentityDeleted> context)
    {
      IdentityDeleted message = context.Message;
      if ((await repository.GetAsync(message.Id)) == null)
      {
        return;
      }

      await repository.RemoveAsync(message.Id);

      System.Collections.Generic.IReadOnlyCollection<Like> likes = await likesRepository.GetAllAsync(like => like.UserId == message.Id);
      System.Collections.Generic.IReadOnlyCollection<Comment> comments = await commentsRepository.GetAllAsync(comment => comment.UserId == message.Id);

      foreach (Like like in likes)
      {
        await likesRepository.RemoveAsync(like.Id);
        await publishEndpoint.Publish(new LikeDeleted(like.Id));
      }


      foreach (Comment comment in comments)
      {
        await commentsRepository.RemoveAsync(comment.Id);
        await publishEndpoint.Publish(new CommentDeleted(comment.Id));
      }
    }
    #endregion
  }
}