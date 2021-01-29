
using Moralar.Data.Entities;
using Moralar.Data.Enum;
using System;
using System.Threading.Tasks;

namespace Moralar.Domain.Services.Interface
{
    public interface IUtilService
    {
        Task RegisterLogAction(LocalAction localAction, TypeAction typeAction, TypeResposible typeResposible, string message, string responsibleId = null, string responsibleName = null, string referenceId = null, string justification = null, Exception ex = null);
        Task LogUserAdministrationAction(string userId, string message, TypeAction typeAction, LocalAction localAction, string referenceId = null);
        void UpdateCascate(Family familyEntity);
        string GetFlag(string flag);
    }
}