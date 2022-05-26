using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hangfire;
using Hangfire.Server;

using MongoDB.Bson;

using Moralar.Data.Entities;
using Moralar.Data.Enum;
using Moralar.Domain.Services.Interface;
using Moralar.Repository.Interface;
using Moralar.WebApi.HangFire.Interface;

namespace Moralar.WebApi.HangFire
{
    public class HangFireService : IHangFireService
    {

        private readonly IQuizRepository _quizRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IUtilService _utilService;

        public HangFireService(IQuizRepository quizRepository, IFamilyRepository familyRepository, IScheduleRepository scheduleRepository)
        {
            _quizRepository = quizRepository;
            _familyRepository = familyRepository;
            _scheduleRepository = scheduleRepository;
        }

        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task MakeQuestionAvailable(PerformContext context = null)
        {
            try
            {

                //var limitDate = new DateTimeOffset(DateTime.Today.AddDays(-30)).ToUnixTimeSeconds();
                var limitDate = new DateTimeOffset(DateTime.Now.AddMinutes(-2)).ToUnixTimeSeconds();

                var listQuestionaryToSend = await _scheduleRepository.FindByAsync(x => x.TypeScheduleStatus == TypeScheduleStatus.Finalizado && x.DatePostChangeQuestionnaire == null && x.Quiz != null && x.DateFinished <= limitDate) as List<Schedule>;

                var totalQuestionary = listQuestionaryToSend.Count();

                if (totalQuestionary > 0)
                {
                    var listFamily = await _familyRepository.FindIn("_id", listQuestionaryToSend.Select(x => ObjectId.Parse(x.FamilyId)).Distinct().ToList()) as List<Family>;

                    var listQuiz = await _quizRepository.FindIn("_id", listQuestionaryToSend.Select(x => ObjectId.Parse(x.Quiz.Id)).Distinct().ToList()) as List<Family>;

                    var now = DateTimeOffset.Now.ToUnixTimeSeconds();

                    for (int i = 0; i < listQuestionaryToSend.Count(); i++)
                    {
                        try
                        {
                            var scheduleEntity = listQuestionaryToSend[i];

                            if (listQuiz == null)
                                continue;

                            var quizEntity = listQuiz.Find(x => x._id.ToString() == scheduleEntity.Quiz.Id);

                            var familyEntity = listFamily.Find(x => x._id.ToString() == scheduleEntity.FamilyId);

                            if (familyEntity == null || quizEntity == null)
                            {
                                scheduleEntity.DatePostChangeQuestionnaire = now;

                                await _scheduleRepository.UpdateAsync(scheduleEntity);

                                continue;
                            }

                            var title = "Novo questionário disponibilizado";

                            var message = new StringBuilder();
                            message.AppendLine($"<p>Olá {familyEntity.Holder.Name}<br/>");
                            message.AppendLine($"Precisamos saber sua opinião sobre sua casa nova!<br/>");
                            message.AppendLine($"Acesse a area de questionários do app Moralar para mais informações</p>");

                            await _utilService.SendNotify(title, message.ToString(), familyEntity.Holder.Email, familyEntity.DeviceId, ForType.Family, familyEntity._id.ToString());

                            scheduleEntity.DatePostChangeQuestionnaire = now;

                            await _scheduleRepository.UpdateAsync(scheduleEntity);
                        }
                        catch (Exception) {/*UNUSED*/}
                    }

                }


            }
            catch (Exception) {/*UNUSED*/}
        }
    }
}
