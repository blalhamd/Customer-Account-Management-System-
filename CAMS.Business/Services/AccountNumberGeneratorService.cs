using CAMS.Core.IRepositories.Generic;
using CAMS.Core.IServices;
using CAMS.Domains.Entities;

namespace CAMS.Business.Services
{
    public class AccountNumberGeneratorService : IAccountNumberGeneratorService
    {
        private readonly IGenericRepositoryAsync<Account, string> _accountRepository;
        private static readonly Random _random = new();

        public AccountNumberGeneratorService(IGenericRepositoryAsync<Account, string> accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<string> GenerateUniqueAccountNumberAsync(CancellationToken ct = default)
        {
            string accountNumber;
            bool exists;

            do
            {
                accountNumber = GenerateAccountNumber();
                exists = await _accountRepository
                    .FirstOrDefaultAsync(x => x.AccountNumber == accountNumber, cancellationToken: ct) != null;
            }
            while (exists);

            return accountNumber;
        }

        private static string GenerateAccountNumber()
        {
            var digits = new char[12];
            for (int i = 0; i < digits.Length; i++)
            {
                digits[i] = (char)('0' + _random.Next(0, 10));
            }
            return new string(digits);
        }
    }
}
