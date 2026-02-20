namespace CAMS.Core.Constants
{
    public class PaginedResponse<T>
    {
        public required T Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
