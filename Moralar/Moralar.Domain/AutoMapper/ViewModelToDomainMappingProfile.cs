using MongoDB.Bson;
using Moralar.Data.Entities;
using Moralar.Data.Entities.Auxiliar;
using Moralar.Domain.ViewModels;
using Moralar.Domain.ViewModels.Admin;
using Moralar.Domain.ViewModels.Family;
using Moralar.Domain.ViewModels.Property;
using Moralar.Domain.ViewModels.Question;
using Moralar.Domain.ViewModels.Quiz;
using System.Collections.Generic;
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
            #region Family
                CreateMap<FamilyHolderViewModel, FamilyHolder>();
                CreateMap<FamilyHolderMinViewModel, FamilyHolder>();
                CreateMap<FamilySpouseViewModel, FamilySpouse>();
                CreateMap<FamilyMemberViewModel, FamilyMember>();
                CreateMap<FamilyFinancialViewModel, FamilyFinancial>();
                CreateMap<FamilyPriorizationViewModel, FamilyPriorization>();
                CreateMap<FamilyCompleteViewModel, Family>()
                     .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<FamilyEditViewModel, Family>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)))
                .ForMember(dest => dest.Holder, opt => opt.MapFrom(src => src.Holder));
            #endregion
            #region ResidencialProperty
               CreateMap<ResidencialPropertyViewModel, ResidencialProperty>()
                 .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
               CreateMap<ResidencialPropertyFeatureViewModel, ResidencialPropertyFeatures>();
               CreateMap<ResidencialPropertyAdressViewModel, ResidencialPropertyAdress>();

            #endregion
            #region Question
            CreateMap<QuestionViewModel, Question>()
              .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<QuestionRegisterViewModel, Question>();
            CreateMap<QuestionDescriptionViewModel, QuestionDescription>();

            CreateMap<QuestionDescriptionViewModel, QuestionDescription>()
                 .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<QuizViewModel, Quiz>();
            CreateMap<QuizUpdateViewModel, Quiz>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));

            #endregion





        }
    }
}
