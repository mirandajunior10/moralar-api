using Moralar.Data.Entities;
using Moralar.Domain.ViewModels;
using Moralar.Domain.ViewModels.Admin;
using Moralar.Domain.ViewModels.Family;
using UtilityFramework.Application.Core;
using UtilityFramework.Application.Core.ViewModels;
using UtilityFramework.Services.Iugu.Core.Models;
using AutoMapperProfile = AutoMapper.Profile;

namespace Moralar.Domain.AutoMapper
{
    public class DomainToViewModelMappingProfile : AutoMapperProfile
    {
        public DomainToViewModelMappingProfile()
        {
            /*EXEMPLE
            CreateMap<Entity,ViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));*/
            CreateMap<IuguCreditCard, CreditCardViewModel>()
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Data.DisplayNumber))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Data.HolderName))
                .ForMember(dest => dest.ExpMonth, opt => opt.MapFrom(src => src.Data.Month))
                .ForMember(dest => dest.ExpYear, opt => opt.MapFrom(src => src.Data.Year))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Data.Brand));
            CreateMap<Bank, BankViewModel>()
                           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<AddressInfoViewModel, InfoAddressViewModel>()
                .ForMember(dest => dest.Neighborhood, opt => opt.MapFrom(src => src.Bairro))
                .ForMember(dest => dest.StateUf, opt => opt.MapFrom(src => src.Uf))
                .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.Logradouro))
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.Cep.ToString()));
            CreateMap<UserAdministrator, UserAdministratorViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => src.DataBlocked != null))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Profile, ProfileViewModel>()
                .ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo.SetPhotoProfile(src.ProviderId, null, null, null, null, 600)))
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => src.DataBlocked != null))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<State, StateDefaultViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<City, CityDefaultViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Family, FamilyCompleteViewModel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));

        }
    }
}