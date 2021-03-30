using MongoDB.Bson;
using Moralar.Data.Entities;
using Moralar.Data.Entities.Auxiliar;
using Moralar.Domain.ViewModels;
using Moralar.Domain.ViewModels.Admin;
using Moralar.Domain.ViewModels.Course;
using Moralar.Domain.ViewModels.Family;
using Moralar.Domain.ViewModels.Informative;
using Moralar.Domain.ViewModels.Notification;
using Moralar.Domain.ViewModels.Profile;
using Moralar.Domain.ViewModels.PropertiesInterest;
using Moralar.Domain.ViewModels.Property;
using Moralar.Domain.ViewModels.Question;
using Moralar.Domain.ViewModels.QuestionAnswer;
using Moralar.Domain.ViewModels.Quiz;
using Moralar.Domain.ViewModels.Schedule;
using Moralar.Domain.ViewModels.Video;
using System.Collections.Generic;
using UtilityFramework.Application.Core;
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
            CreateMap<ProfileUpdateViewModel, Profile>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));

            CreateMap<ProfileViewModel, Profile>()
              .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            #region Family
            CreateMap<FamilyHolderViewModel, FamilyHolder>();
            CreateMap<FamilyHolderMinViewModel, FamilyHolder>();
            CreateMap<FamilySpouseViewModel, FamilySpouse>();
            CreateMap<FamilyMemberViewModel, FamilyMember>();
            CreateMap<FamilyFinancialViewModel, FamilyFinancial>();
            CreateMap<PriorityRateViewModel, PriorityRate>();
            CreateMap<FamilyPriorizationViewModel, FamilyPriorization>();
            CreateMap<FamilyAddressViewModel, FamilyAddress>();

            CreateMap<FamilyCompleteViewModel, Family>()
                 .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<FamilyCompleteWebViewModel, Family>()
                 .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)))
                 .ForMember(dest => dest.Priorization, opt => opt.MapFrom(src => src.Priorization))
                 ;
            //            Error mapping types.

            //Mapping types:
            //FamilyCompleteWebViewModel->Family
            //Moralar.Domain.ViewModels.Family.FamilyCompleteWebViewModel->Moralar.Data.Entities.Family

            //Type Map configuration:
            //            FamilyCompleteWebViewModel->Family
            //Moralar.Domain.ViewModels.Family.FamilyCompleteWebViewModel->Moralar.Data.Entities.Family

            //Property:
            //            Priorization
            //            Priorization
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
            #region Course

            CreateMap<CourseFamilyViewModel, CourseFamily>()
              .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            #endregion
            #region QuizFamily
            CreateMap<QuizFamilyViewModel, QuizFamily>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            #endregion
            #region QuizAnswer
            CreateMap<QuestionAnswerRegisterViewModel, QuestionAnswer>()
             .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)))
             .ForMember(dest => dest.FamilyHolderName, opt => opt.MapFrom(src => src.FamilyHolderName))
             .ForMember(dest => dest.FamilyHolderCpf, opt => opt.MapFrom(src => src.FamilyHolderCpf))
             .ForMember(dest => dest.ResponsibleForResponsesName, opt => opt.MapFrom(src => src.ResponsibleForResponsesName))
             .ForMember(dest => dest.ResponsibleForResponsesCpf, opt => opt.MapFrom(src => src.ResponsibleForResponsesCpf));

            #endregion
            #region Schedule
            CreateMap<ScheduleRegisterViewModel, Schedule>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<ScheduleChangeSubjectViewModel, Schedule>()
               .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<ScheduleChangeStatusViewModel, Schedule>()
               .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<ScheduleRegisterViewModel, ScheduleHistory>()
               .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<ScheduleChangeStatusViewModel, ScheduleHistory>()
         .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<ScheduleChangeSubjectViewModel, ScheduleHistory>()
        .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));



            #endregion
            #region Informative
            CreateMap<InformativeViewModel, Informative>()
                 .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<InformativeListViewModel, Informative>()
                 .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            #endregion
            #region Notification
            CreateMap<NotificationViewModel, Notification>()
                 .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            #endregion
            #region PropertiesInterest
            CreateMap<PropertiesInterestRegisterViewModel, PropertiesInterest>()
             .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            #endregion

            #region Course
            CreateMap<CourseViewModel, Course>()
        .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            #endregion
            #region Video
            CreateMap<VideoViewModel, Video>()
       .ForMember(dest => dest._id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            #endregion



        }
    }

}
