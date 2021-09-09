﻿using System.ComponentModel.DataAnnotations;

namespace Moralar.Domain.ViewModels.PropertiesInterest
{
    public class SearchMatchsViewModel
    {
        
        /// <summary>
        /// Busca
        /// </summary>
        /// <example>Nome, CPF ou nº do cadastro</example>
        [Required(ErrorMessage = nameof(DefaultMessages.FieldRequired))]
        [Display(Name = nameof(DefaultMessages.FieldSearchTerm))]
        public string Search { get; set; }
        public string ResidencialCode { get; set; }        
    }
}
