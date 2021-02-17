using Microsoft.AspNetCore.Hosting;
using Moralar.Data.Entities;
using Moralar.Repository.Interface;
using UtilityFramework.Infra.Core.MongoDb.Business;

namespace Moralar.Repository
{
    public class CourseRegistrationRepository : BusinessBaseAsync<CourseRegistration>, ICourseRegistrationRepository
    {
        public CourseRegistrationRepository(IHostingEnvironment env) : base(env)
        {
        }
    }
}