using System.ComponentModel.DataAnnotations;
using UtilityFramework.Application.Core;

namespace Moralar.Domain.ViewModels.ResidencialProperty
{
    public class ResidencialPropertyImportViewModel
    {

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Código do imóvel*")]
        [Column(1)]
        public string Code { get; set; }

        /*ENDEREÇO*/
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "CEP*")]
        [Column(2)]
        public string CEP { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Endereço*")]
        [Column(3)]
        public string StreetAddress { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Número*")]
        [Column(4)]
        public string Number { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Nome da Cidade*")]
        [Column(5)]
        public string CityName { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "UF*")]
        [Column(6)]
        public string StateUf { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Bairro*")]
        [Column(7)]
        public string Neighborhood { get; set; }

        [Display(Name = "Complemento")]
        [Column(8)]
        public string Complement { get; set; }

        /*CARACTERÍSTICAS*/
        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Valor do imóvel*")]
        [Column(9)]
        public string PropertyValue { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tipo do imóvel*")]
        [Column(10)]
        public string TypeProperty { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Metragem quadrada*")]
        [Column(11)]
        public string SquareFootage { get; set; }

        [Display(Name = "Valor do condomínio")]
        [Column(12)]
        public string CondominiumValue { get; set; }

        [Display(Name = "Valor do IPTU")]
        [Column(13)]
        public string IptuValue { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Bairro de localização*")]
        [Column(14)]
        public string NeighborhoodLocalization { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Número de pavimentos*")]
        [Column(15)]
        public string NumberFloors { get; set; }


        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Andar de localização*")]
        [Column(16)]
        public string FloorLocation { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem Elevador?")]
        [Column(17)]
        public string HasElavator { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Número de quartos*")]
        [Column(18)]
        public string NumberOfBedrooms { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Número de banheiros*")]
        [Column(19)]
        public string NumberOfBathrooms { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem área de serviço?*")]
        [Column(20)]
        public string HasServiceArea { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem garagem de serviço?")]
        [Column(21)]
        public string HasGarage { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem quintal?")]
        [Column(22)]
        public string HasYard { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem cisterna?")]
        [Column(23)]
        public string HasCistern { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "É toda murada?")]
        [Column(24)]
        public string HasWall { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem escada de acesso?")]
        [Column(25)]
        public string HasAccessLadder { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tem rampa de acesso?")]
        [Column(26)]
        public string HasAccessRamp { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "É Adaptada ou permite adaptação para PCD?")]
        [Column(27)]
        public string HasAdaptedToPcd { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Regularização do imóvel")]
        [Column(28)]
        public string PropertyRegularization { get; set; }

        [Required(ErrorMessage = DefaultMessages.FieldRequired)]
        [Display(Name = "Tipo de instalação de gás")]
        [Column(29)]
        public string TypeGasInstallation { get; set; }

        [Display(Name = "Beira de rua?")]
        [Column(30)]
        public string StreetEdge { get; set; }

    }
}