using CAMS.Core.IRepositories.Non_Generic;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Core.PresentationModels.DTOs.Client;
using CAMS.Core.PresentationModels.DTOs.User;
using CAMS.Domains.Entities;
using CAMS.Infrastructure.Data.context;
using CAMS.Infrastructure.Repositories.Generic;
using Microsoft.EntityFrameworkCore;

namespace CAMS.Infrastructure.Repositories
{
    public class ClientRepositoryAsync : GenericRepositoryAsync<Client, string> ,IClientRepositoryAsync
    {
        private readonly AppDbContext _context;
        public ClientRepositoryAsync(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ClientDto?> GetClientByIdAsync(string clientId)
        {
            var query = await _context.Clients
                                              .AsNoTracking()
                                              .Select(x => new ClientDto
                                              {
                                                  Id = x.Id,
                                                  FullName = x.FullName,
                                                  BirthDate = x.BirthDate,
                                                  Gender = x.Gender,
                                                  JobTitle = x.JobTitle,
                                                  SSN = x.SSN,
                                                  MonthlyIncome = x.MonthlyIncome,
                                                  Nationality = x.Nationality,
                                                  ImagePath = x.ImagePath,
                                                  FinancialSource = x.FinancialSource,
                                                  Address = x.Address,
                                                  User = new AppUserDto
                                                  {
                                                      Email = x.User.Email ?? string.Empty,
                                                      PhoneNumber = x.User.PhoneNumber ?? string.Empty
                                                  },
                                                  Accounts = x.Accounts.Select(a => new AccountDto
                                                  {
                                                      AccountNumber = a.AccountNumber,
                                                      AccountStatus = a.AccountStatus,
                                                      Balance = a.Balance,
                                                      Branch = a.Branch,
                                                      CurrencyType = a.CurrencyType,
                                                      IsSigned = a.IsSigned,
                                                  }).ToList()
                                              })
                                              .FirstOrDefaultAsync(x => x.Id == clientId);

            return query;
        }

        public async Task<Client> GetSoftDeleteClientAsync(string clientId)
        {
            var client = await _context.Clients.IgnoreQueryFilters()
                                               .FirstOrDefaultAsync(x => x.Id == clientId && x.IsDeleted);

            return client!;
        }

        public Task<Client?> GetSoftDeleteClientAsyncV2(string clientId)
        {
            return _getSoftDeleteClient(_context, clientId);
        }

        private static readonly Func<AppDbContext, string, Task<Client?>> _getSoftDeleteClient =
            EF.CompileAsyncQuery((AppDbContext context, string clientId)
                => context.Set<Client>().IgnoreQueryFilters()
                                               .FirstOrDefault(x => x.Id == clientId && x.IsDeleted));

    }
}
