﻿using Moralar.Data.Enum;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using UtilityFramework.Application.Core;

namespace Moralar.Domain.ViewModels
{
    public class LoginFamilyViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string HolderCpf { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        public string Password { get; set; }
        public string RefreshToken { get; set; }

    }
}