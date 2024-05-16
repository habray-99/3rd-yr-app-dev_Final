using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication6.Areas.Identity.Data;

namespace WebApplication6.Models
{
    public class Reaction
    {
        [Key]
        public int? ReactionID { get; set; }
        [Required]
        public int BlogID { get; set; }
        [Required]
        public string? UserID { get; set; }
        [ForeignKey("ReactionTypeID")]
        [Required]
        public int ReactionTypeID { get; set; }
        //[ForeignKey("BlogID")]
        public virtual Blog? Blog { get; set; }
        public virtual CustomUser? User { get; set; }
        //[ForeignKey("ReactionTypeID")]
        public virtual ReactionType? ReactionType { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public void CreateReactionNotification(IdentityDBContext context, string blogOwnerUserId, string byWhome)
        {
            // Get the reaction type name
            var reactionType = context.ReactionTypes.Find(ReactionTypeID);
            var reactionName = reactionType?.ReactionName;

            var notification = new Notification
            {
                UserID = blogOwnerUserId,
                NotificationType = $"New reaction ({reactionName}) on your blog post by: {byWhome}",
                EntityID = this.BlogID
            };

            context.Notifications.Add(notification);
            context.SaveChanges();
        }
    }
}
