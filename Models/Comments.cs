using System.ComponentModel.DataAnnotations;
using WebApplication6.Areas.Identity.Data;

namespace WebApplication6.Models;

public class Comment
{
    [Key] public int CommentID { get; set; }

    [Required] public int BlogID { get; set; }

    [Required] public string? UserID { get; set; }

    [Required] public string? CommentText { get; set; }

    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    //[ForeignKey("BlogID")]
    public virtual Blog? Blog { get; set; }

    //[ForeignKey("UserID")]
    public virtual CustomUser? User { get; set; }

    public ICollection<CommentReaction>? CommentReactions { get; set; }

    //public void CreateCommentNotification(IdentityDBContext context, string blogOwnerUserId. string byWhome)
    //{
    //    var notification = new Notification
    //    {
    //        UserID = blogOwnerUserId,
    //        NotificationType = $"New comment on your blog post: {byWhome}",
    //        EntityID = this.BlogID
    //    };

    //    context.Notifications.Add(notification);
    //    context.SaveChanges();
    //}
    public void CreateCommentNotification(IdentityDBContext context, string blogOwnerUserId, string byWhome)
    {
        var notification = new Notification
        {
            UserID = blogOwnerUserId,
            NotificationType = $"New comment on your blog post by: {byWhome}",
            EntityID = BlogID
        };
        context.Notifications.Add(notification);
        context.SaveChanges();
    }
}