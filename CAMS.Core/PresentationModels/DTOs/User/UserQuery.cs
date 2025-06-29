using CAMS.Domains.Enums;

namespace CAMS.Core.PresentationModels.DTOs.User
{
    public class UserQuery
    {
        private const int MaxPageSize = 10;

        public int pageNumber = 1;

        private int pageSize = MaxPageSize;
        public string? SearchText { get; init; }      // name, email, phone substrings
        public bool? IsEnabled { get; init; }         // true = active, false = disabled
        public string? SortBy { get; init; } = "CreatedAt";      // "CreatedAt", "Email"…
        public SortDirection SortDir { get; init; } = SortDirection.ASC;


        public int PageNumber
        {
            get => pageNumber;
            set => pageNumber = (value <= 0) ? 1 : value;
        }
        public int PageSize
        {
            get => pageSize;
            set => pageSize = (value <= 0 || value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}
