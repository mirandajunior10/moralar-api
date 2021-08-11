using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Moralar.Domain.ViewModels.Family
{
    public class FamilyForgotPasswordViewModel
    {
        public string MotherName { get; set; }
        public string MotherCityBorned { get; set; }
        public string Cpf { get; set; }

    }
}
