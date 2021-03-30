using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UtilityFramework.Application.Core.ViewModels;

namespace Moralar.Domain.ViewModels.Notification
{
    public class NotificationViewModel : BaseViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Título da notificação")]
        public string Title { get; set; }
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Descrição da notificação")]
        public string Description { get; set; }
    }
}
