namespace Tweetbook.Contracts.V1.Requests.Queries
{
   public class PaginationQuery
   {
      public int PageNumber { get; set; }
      public int PageSize { get; set; }

      public PaginationQuery()
      {
         // Set default values
         PageNumber = 1;
         PageSize = 100;
      }

      public PaginationQuery(int pageNumber, int pageSize )
      {
         PageNumber = pageNumber;
         // For this property we can add validation to avoid setting a really big number
         PageSize = pageSize;
      }
   }
}
