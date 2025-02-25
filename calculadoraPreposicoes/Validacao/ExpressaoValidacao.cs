using System.ComponentModel.DataAnnotations;

namespace calculadoraPreposicoes.Validacao
{
    public class ExpressaoValidacao : ValidationAttribute
    {
        private static readonly HashSet<char> caracteresPermitidos = new()
        {
            'A', 'B', 'C', 'D', 'E', '(', ')', '~', '∧', '∨', '→', '↔', '⊻'
        };

        private static readonly HashSet<char> operadores = new()
        {
            '~', '∧', '∨', '→', '↔', '⊻'
        };

        private static readonly HashSet<char> preposicoes = new()
        {
            'A', 'B', 'C', 'D', 'E'
        };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not string expressao || string.IsNullOrWhiteSpace(expressao))
            {
                return new ValidationResult("A expressão não pode ser vazia.");
            }

            // Verificar caracteres inválidos
            if (expressao.Any(c => !caracteresPermitidos.Contains(c)))
            {
                return new ValidationResult("A expressão contém caracteres inválidos.");
            }

            // Verificar se os parênteses estão balanceados
            if (!ParentesesBalanceados(expressao))
            {
                return new ValidationResult("A expressão possui parênteses desbalanceados.");
            }

            // Verificar se não há preposições juntas sem operador
            if (TemPreposicoesJuntas(expressao))
            {
                return new ValidationResult("A expressão contém preposições juntas sem operador.");
            }

            // Verificar se não há operadores juntos sem preposição entre eles
            if (TemOperadoresJuntos(expressao))
            {
                return new ValidationResult("A expressão contém operadores juntos sem preposição entre eles.");
            }

            return ValidationResult.Success;
        }

        private static bool ParentesesBalanceados(string expressao)
        {
            int contador = 0;
            foreach (char c in expressao)
            {
                if (c == '(') contador++;
                if (c == ')') contador--;

                if (contador < 0) return false; // Fecha parênteses antes de abrir
            }
            return contador == 0;
        }

        private static bool TemPreposicoesJuntas(string expressao)
        {
            for (int i = 0; i < expressao.Length - 1; i++)
            {
                if (preposicoes.Contains(expressao[i]) && preposicoes.Contains(expressao[i + 1]))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool TemOperadoresJuntos(string expressao)
        {
            for (int i = 0; i < expressao.Length - 1; i++)
            {
                if (operadores.Contains(expressao[i]) && operadores.Contains(expressao[i + 1]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
