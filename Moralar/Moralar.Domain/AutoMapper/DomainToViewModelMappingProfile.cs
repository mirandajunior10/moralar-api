using Moralar.Data.Entities;
using Moralar.Data.Entities.Auxiliar;
using Moralar.Domain.ViewModels;
using Moralar.Domain.ViewModels.Admin;
using Moralar.Domain.ViewModels.Course;
using Moralar.Domain.ViewModels.Family;
using Moralar.Domain.ViewModels.Informative;
using Moralar.Domain.ViewModels.InformativeSended;
using Moralar.Domain.ViewModels.PropertiesInterest;
using Moralar.Domain.ViewModels.Property;
using Moralar.Domain.ViewModels.Question;
using Moralar.Domain.ViewModels.Quiz;
using Moralar.Domain.ViewModels.Schedule;
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
                //.ForMember(dest => dest.Photo, opt => opt.MapFrom(src => src.Photo.SetPhotoProfile(src.ProviderId, null, null, null, null, 600)))
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => src.DataBlocked != null))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<State, StateDefaultViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<City, CityDefaultViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));

            #region Family
            CreateMap<FamilyHolder, FamilyHolderViewModel>();
            CreateMap<FamilyHolder, FamilyHolderListViewModel>()
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<FamilyHolder, FamilyHolderMinViewModel>();
            CreateMap<FamilySpouse, FamilySpouseViewModel>();
            CreateMap<FamilyMember, FamilyMemberViewModel>();
            CreateMap<FamilyFinancial, FamilyFinancialViewModel>();
            CreateMap<FamilyPriorization, FamilyPriorizationViewModel>();
            CreateMap<Family, FamilyHolderListViewModel>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Created))
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Holder.Number))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Holder.Name))
                .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.Holder.Cpf))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Holder.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Holder.Phone))
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => src.DataBlocked != null ? true : false))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Family, FamilyCompleteViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Family, FamilyCompleteViewModel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Family, FamilyEditViewModel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()))
               .ForMember(dest => dest.Holder, opt => opt.MapFrom(src => src.Holder));
            #endregion
            #region ResidencialProperty
            CreateMap<ResidencialProperty, ResidencialPropertyViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()))
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => src.DataBlocked != null ? true : false));
            CreateMap<ResidencialPropertyAdress, ResidencialPropertyAdressViewModel>();
            CreateMap<ResidencialPropertyFeatures, ResidencialPropertyFeatureViewModel>();
            CreateMap<ResidencialProperty, ResidencialPropertyAdress>();
            #endregion
            #region Quiz
            CreateMap<Quiz, QuizViewModel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Question, QuestionViewModel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<QuestionDescription, QuestionDescriptionViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));

            CreateMap<Quiz, QuizExportViewModel>()
             .ForMember(dest => dest.Created, opt => opt.MapFrom(src => Utilities.TimeStampToDateTime(src.Created.Value)));
            CreateMap<QuizFamily, QuizFamilyViewModel>();
            //.ForMember(dest => dest.Created, opt => opt.MapFrom(src => Utilities.TimeStampToDateTime(src.Created.Value)))

            CreateMap<QuizFamily, QuizFamilyListViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.TypeStatus == 0 ? "Não respondido" : "Respondido"))
            ;
            CreateMap<Quiz, QuizListViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));



            #endregion
            #region Course
           
            #endregion
            #region Schedule
            CreateMap<Schedule, ScheduleRegisterViewModel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<Schedule, ScheduleListViewModel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            #endregion

            #region Informative
            CreateMap<Informative, InformativeListViewModel>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()))
              .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.DataBlocked != null ?"true":"false"));
            CreateMap<InformativeSended, InformativeSendedDetailViewModel>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            #endregion
            #region PropertiesInterest
            CreateMap<PropertiesInterest, PropertiesInterestRegisterViewModel>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));
            CreateMap<PropertiesInterest, PropertiesInterestViewModel>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));

            //.ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src.FamilyId))
            //.ForMember(dest => dest.ResidelcialPropertyId, opt => opt.MapFrom(src => src.ResidelcialPropertyId))
            //.ForMember(dest => dest.FamilyId, opt => opt.MapFrom(src => src._id))


            #endregion
            #region Course
            CreateMap<Course, CourseViewModel>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()));

            CreateMap<Course, CourseListViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src._id.ToString()))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => Utilities.TimeStampToDateTime(src.StartDate)))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => Utilities.TimeStampToDateTime(src.EndDate)));
           
            #endregion

        }
    }
}