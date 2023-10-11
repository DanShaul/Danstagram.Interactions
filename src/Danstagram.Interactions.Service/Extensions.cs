using Danstagram.Interactions.Service.Entities;

namespace Danstagram.Interactions.Service{
    public static class Extensions{
        #region Methods
        public static LikeDto AsDto(this Like like){
            return new LikeDto(like.Id,like.UserId,like.FeedItemId);
        }
        #endregion
    }
}