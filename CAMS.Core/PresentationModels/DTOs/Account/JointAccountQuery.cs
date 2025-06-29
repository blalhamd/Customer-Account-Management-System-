namespace CAMS.Core.PresentationModels.DTOs.Account
{
    public class JointAccountQuery
    {
        private const int MaxPageSize = 10;

        private int pageNumber = 1;

        private int pageSize = MaxPageSize;
        public string? SearchBy { get; set; }

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
