using System;

namespace Danstagram.Interactions.Service{
    #region Properties

    public record LikeDto(Guid Id, Guid UserId,Guid FeedItemId);

    public record CommentDto(Guid Id, Guid UserId,Guid FeedItemId,string Message, DateTimeOffset CreatedDate);

    public record CreateCommentDto(Guid UserId,Guid FeedItemId,string Message);

    public record CreateLikeDto(Guid UserId,Guid FeedItemId);

    #endregion
}