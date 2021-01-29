﻿using Moralar.Data.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Moralar.Domain.ViewModels.Property
{
    public class ResidencialPropertyFeatureViewModel
    {
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Valor do imóvel")]
        public decimal PropertyValue { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tipo do imóvel")]
        public TypeProperty TypeProperty { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Metragem quadrada")]
        public decimal SquareFootage { get; set; }


        [Display(Name = "Valor do condomínio")]
        public decimal CondominiumValue { get; set; }


        [Display(Name = "Valor do IPTU")]
        public decimal IptuValue { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Bairro de localização")]
        public string Neighborhood { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Número de pavimentos")]
        [Range(0, 20, ErrorMessage = "Insira um valor de 0 até 20")]
        public int NumberFloors { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Andar de localização")]
        [Range(0, 20, ErrorMessage = "Insira um valor de 0 até 20")]
        public int FloorLocation { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem Elevador?")]
        public bool HasElavator { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Número de quartos")]
        public int NumberOfBedrooms { get; set; }

        
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Número de banheiros")]
        public int NumberOfBathrooms { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem área de serviço?")]
        public bool HasServiceArea { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem garagem de serviço?")]
        public bool HasGarage { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem quintal?")]
        public bool HasYard { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem cisterna?")]
        public bool HasCistern { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "É toda murada?")]
        public bool HasWall { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem escada de acesso?")]
        public bool HasAccessLadder { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem rampa de acesso?")]
        public bool HasAccessRamp { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "É Adaptada ou permite adaptação para PCD?")]
        public bool HasAdaptedToPcd { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Regularização do imóvel")]
        public TypePropertyRegularization PropertyRegularization { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tipo de instalação de gás")]
        public TypePropertyGasInstallation TypeGasInstallation { get; set; }
    }
}
