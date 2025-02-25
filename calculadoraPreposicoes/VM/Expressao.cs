using calculadoraPreposicoes.Validacao;
using System.ComponentModel.DataAnnotations;

namespace calculadoraPreposicoes.VM
{
    public class Expressao
    {
        [Required(ErrorMessage = "A expressão é obrigatória.")]
        [ExpressaoValidacao(ErrorMessage = "A expressão não é válida.")]
        public string expressao { get; set; }
    }
}
