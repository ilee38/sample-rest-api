using System.ComponentModel.DataAnnotations;

namespace Tweetbook.Domain
{
    public class Post
    {
        /// <summary>
        /// The Key attribute is used to mark the primary key of the entity (for Entity Framework).
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
