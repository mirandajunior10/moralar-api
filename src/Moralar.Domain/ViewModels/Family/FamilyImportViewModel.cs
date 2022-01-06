

using UtilityFramework.Application.Core;

namespace Moralar.Domain.ViewModels.Family
{
    public class FamilyImportViewModel
    {

        [Column(1)]
        public string Numero_do_cadastro { get; set; }
        [Column(2)]
        public string Nome_do_titular { get; set; }
        [Column(3)]
        public string Cpf_do_titular { get; set; }
        [Column(4)]
        public string Data_de_nascimento { get; set; }
        [Column(5)]
        public string Genero { get; set; }
        [Column(6)]
        public string E_mail { get; set; }
        [Column(7)]
        public string Telefone { get; set; }
        [Column(8)]
        public string Escolaridade { get; set; }
        [Column(9)]
        public string Nome_do_Conjuge { get; set; }
        [Column(10)]
        public string Data_de_Nascimento { get; set; }
        [Column(11)]
        public string Genero_Conjuge { get; set; }
        [Column(12)]
        public string Escolaridade_Conjuge { get; set; }
        [Column(13)]
        public string Renda_Familiar { get; set; }
        [Column(14)]
        public string Valor_imovel_demolido { get; set; }
        [Column(15)]
        public string Valor_para_compra_de_imovel { get; set; }
        [Column(16)]
        public string Valor_incremento { get; set; }
        [Column(17)]
        public string Frente_de_Obras { get; set; }
        [Column(18)]
        public string Deficiencia_que_demande_imovel_acessivel { get; set; }
        [Column(19)]
        public string Idoso_acima_de_80_anos { get; set; }
        [Column(20)]
        public string Idoso_60_ate_79_anos { get; set; }
        [Column(21)]
        public string Mulher_atendida_por_medida_protetiva { get; set; }
        [Column(22)]
        public string Monoparental { get; set; }
        [Column(23)]
        public string Familia_com_mais_5_pessoas { get; set; }
        [Column(24)]
        public string Filhos_menores_de_18_anos { get; set; }
        [Column(25)]
        public string Chefe_de_familia_sem_renda { get; set; }
        [Column(26)]
        public string Beneficio_de_prestacao_continuada { get; set; }
        [Column(27)]
        public string Bolsa_Familia { get; set; }
        [Column(28)]
        public string Coabitacao_involuntaria { get; set; }
        [Column(29)]
        public string Renda_familiar_de_ate_dois_salarios_minimos { get; set; }
    }
}