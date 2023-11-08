using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tweetbook.Domain
{
   public class PostTag
   {
      [ForeignKey(nameof(TagName))]
      public virtual Tag Tag { get; set; }

      public string TagName { get; set; }

      public Guid PostId { get; set; }

      public virtual Post Post { get; set; }
   }
}


