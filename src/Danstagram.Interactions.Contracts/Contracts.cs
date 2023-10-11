using System;

namespace Danstagram.Interactions.Contracts{
    #region Properties

    public record LikeCreated(Guid Id,Guid UserId,Guid FeedItemId);

    public record LikeDeleted(Guid Id);

    public record CommentCreated(Guid Id,Guid UserId,Guid FeedItemId,string Comment,DateTimeOffset CreatedDate);

    public record CommentDeleted(Guid Id);
    
    #endregion
}