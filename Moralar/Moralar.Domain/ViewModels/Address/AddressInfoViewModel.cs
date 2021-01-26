
namespace Moralar.Domain.ViewModels
{
    public class AddressInfoViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Cep { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Logradouro { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Complemento { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Bairro { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Localidade { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Uf { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Unidade { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Ibge { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Gia { get; set; }

        public bool Erro { get; set; }
    }
}