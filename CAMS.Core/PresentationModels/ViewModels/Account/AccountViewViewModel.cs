using CAMS.Core.PresentationModels.DTOs.Transaction;

namespace CAMS.Core.PresentationModels.ViewModels.Account
{
    public class AccountViewViewModel : AccountViewModel
    {
        public ICollection<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    }
}
