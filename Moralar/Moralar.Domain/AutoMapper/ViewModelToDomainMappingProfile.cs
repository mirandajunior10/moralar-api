using MongoDB.Bson;
using Moralar.Data.Entities;
using Moralar.Domain.ViewModels;
using Moralar.Domain.ViewModels.Admin;
using Moralar.Domain.ViewModels.Family;
using AutoMapperProfile = AutoMapper.Profile;

namespace Moralar.Domain.AutoMapper
{
    public class ViewModelToDomainMappingProfile : AutoMapperProfile
    {
        public ViewModelToDomainMappingProfile()
        {
            /*EXEMPLE*/
            //CreateMap<ViewModel, Entity>()
            //    .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<UserAdministratorViewModel, UserAdministrator>()
                    .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<ProfileRegisterViewModel, Profile>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<FamilyCompleteViewModel, Family>()
              .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
        }
    }
}