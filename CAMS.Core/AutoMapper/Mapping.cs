using AutoMapper;
using CAMS.Core.PresentationModels.DTOs.Account;
using CAMS.Core.PresentationModels.DTOs.Client;
using CAMS.Core.PresentationModels.DTOs.Transaction;
using CAMS.Core.PresentationModels.DTOs.User;
using CAMS.Core.PresentationModels.ViewModels.Account;
using CAMS.Core.PresentationModels.ViewModels.Client;
using CAMS.Core.PresentationModels.ViewModels.Transaction;
using CAMS.Domains.Entities;

namespace CAMS.Core.AutoMapper
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<CreateClientDto, Client>()
                    .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.BirthDate)))
                    .ForPath(dest => dest.Address.Country, opt => opt.MapFrom(src => src.Address.Country))
                    .ForPath(dest => dest.Address.City, opt => opt.MapFrom(src => src.Address.City))
                    .ForPath(dest => dest.Address.Street, opt => opt.MapFrom(src => src.Address.Street))
                    .ForPath(dest => dest.Address.ZipCode, opt => opt.MapFrom(src => src.Address.Country))
                    .ForMember(dest => dest.UserId, opt => opt.Ignore())
                    .ForMember(dest => dest.ImagePath, opt => opt.Ignore());

            CreateMap<Client, ClientViewModel>();

            CreateMap<AuditEntry, AuditEntryDto>().ReverseMap();

            CreateMap<ClientDto, ClientViewViewModel>().ReverseMap();

            CreateMap<UpdateClientDto,Client>()
                    .ForPath(dest => dest.BirthDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.BirthDate)))
                    .ForPath(dest => dest.Address.Country, opt => opt.MapFrom(src => src.Address.Country))
                    .ForPath(dest => dest.Address.City, opt => opt.MapFrom(src => src.Address.City))
                    .ForPath(dest => dest.Address.Street, opt => opt.MapFrom(src => src.Address.Street))
                    .ForPath(dest => dest.Address.ZipCode, opt => opt.MapFrom(src => src.Address.Country))
                    .ForMember(dest => dest.ImagePath, opt => opt.Ignore());

            CreateMap<FixedDeposit, FixedDepositViewModel>().ReverseMap();
            CreateMap<CreateFixedDepositDto, FixedDeposit>();

            CreateMap<JointAccount, JointAccountViewModel>().ReverseMap();

            CreateMap<Account, AccountViewViewModel>()
                .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src => src.Transactions));

            CreateMap<Transaction, TransactionDto>();

            CreateMap<Saving, SavingViewModel>();
            CreateMap<Current, CurrentViewModel>();
            CreateMap<Transaction, TransactionViewModel>();

            CreateMap<Transaction, TransactionViewModel>();

            CreateMap<Account, AccountViewModel>();
            CreateMap<Loan, LoanViewModel>();
        }
    }
}
