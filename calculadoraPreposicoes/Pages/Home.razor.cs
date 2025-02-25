using calculadoraPreposicoes.VM;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace calculadoraPreposicoes.Pages
{
    public partial class Home
    {
        private EditContext context;
        private Expressao expressao;
        private List<string> colunasTabela = new();
        private List<List<string>> tabelaVerdade;
        private int cont;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            expressao = new Expressao();
            context = new EditContext(expressao);
            context.OnValidationRequested += (sender, e) => Console.WriteLine("Validação acionada!");
        }
        private void adicionarCaractere(string caractere)
        {
            expressao.expressao += caractere;
        }

        private void apagarCaractere()
        {
            if (!string.IsNullOrEmpty(expressao.expressao) && expressao.expressao.Length > 0)
            {
                expressao.expressao = expressao.expressao.Substring(0, expressao.expressao.Length - 1);
            }
        }

        private void apagar()
        {
            if (!string.IsNullOrEmpty(expressao.expressao) && expressao.expressao.Length > 0)
            {
                expressao.expressao = "";
            }
        }

        private void calcularTabelaVerdade()
        {
            if (context.Validate())
            {
                // Identifica as proposições únicas
                var proposicoes = new HashSet<string>();
                foreach (var c in expressao.expressao)
                {
                    if (char.IsLetter(c))
                    {
                        proposicoes.Add(c.ToString());
                    }
                }

                var proposicoesOrdenadas = proposicoes.OrderBy(p => p).ToList();
                colunasTabela = new List<string>(proposicoesOrdenadas) { expressao.expressao }; // Mantém a ordem correta

                // Gera todas as combinações de valores verdade
                int linhas = (int)Math.Pow(2, proposicoes.Count);
                tabelaVerdade = new List<List<string>>();


                for (int i = 0; i < linhas; i++)
                {
                    var valores = new Dictionary<string, bool>();
                    for (int j = 0; j < proposicoes.Count; j++)
                    {
                        valores[proposicoesOrdenadas[j]] = (i & (1 << (proposicoes.Count - j - 1))) != 0; // Ajuste na ordem das proposições
                    }

                    // Avalia a expressão para cada combinação
                    var resultado = AvaliarExpressao(expressao.expressao, valores);

                    // Adiciona a linha à tabela
                    var linha = proposicoesOrdenadas.Select(p => valores[p].ToString()).ToList();
                    linha.Add(resultado.ToString());
                    tabelaVerdade.Add(linha);
                }

                tabelaVerdade.Add(colunasTabela);

                cont = colunasTabela.Count;
                tabelaVerdade.Reverse(); // Apenas inverte as linhas, mantendo a ordem das colunas correta
                StateHasChanged();
            }
        }




        private bool AvaliarExpressao(string expr, Dictionary<string, bool> valores)
        {
            // Substitui as proposições pelos valores
            foreach (var prop in valores.Keys)
            {
                expr = expr.Replace(prop, valores[prop] ? "true" : "false");
            }

            while (expr.Contains("("))
            {
                var parentesesMatch = System.Text.RegularExpressions.Regex.Match(expr, @"\(([^()]+)\)");
                if (parentesesMatch.Success)
                {
                    string subExpressao = parentesesMatch.Groups[1].Value;
                    bool resultadoSub = AvaliarExpressao(subExpressao, valores);
                    expr = expr.Replace(parentesesMatch.Value, resultadoSub.ToString().ToLower());
                }
            }

            // Implementação manual de operadores
            while (expr.Contains("→") || expr.Contains("∧") || expr.Contains("∨") || expr.Contains("~") || expr.Contains("↔") || expr.Contains("⊻"))
            {
                // Negação
                var negacaoMatch = System.Text.RegularExpressions.Regex.Match(expr, @"~\s*(true|false)");
                if (negacaoMatch.Success)
                {
                    var valor = negacaoMatch.Groups[1].Value == "true" ? false : true;
                    expr = expr.Replace(negacaoMatch.Value, valor.ToString().ToLower());
                    continue;
                }

                // Conjunção (AND)
                var conjuncaoMatch = System.Text.RegularExpressions.Regex.Match(expr, @"(true|false)\s*\∧\s*(true|false)");
                if (conjuncaoMatch.Success)
                {
                    var valoresConjuncao = conjuncaoMatch.Groups[1].Value == "true" && conjuncaoMatch.Groups[2].Value == "true";
                    expr = expr.Replace(conjuncaoMatch.Value, valoresConjuncao.ToString().ToLower());
                    continue;
                }

                // Disjunção (OR) atualizado para '|'
                var disjuncaoMatch = System.Text.RegularExpressions.Regex.Match(expr, @"(true|false)\s*\∨\s*(true|false)");
                if (disjuncaoMatch.Success)
                {
                    var valoresDisjuncao = disjuncaoMatch.Groups[1].Value == "true" || disjuncaoMatch.Groups[2].Value == "true";
                    expr = expr.Replace(disjuncaoMatch.Value, valoresDisjuncao.ToString().ToLower());
                    continue;
                }

                // Implicação (->)
                var implicacaoMatch = System.Text.RegularExpressions.Regex.Match(expr, @"(true|false)\s*→\s*(true|false)");
                if (implicacaoMatch.Success)
                {
                    var antecedente = implicacaoMatch.Groups[1].Value == "true";
                    var consequente = implicacaoMatch.Groups[2].Value == "true";
                    var valorImplicacao = !antecedente || consequente; // Implementação de A -> B
                    expr = expr.Substring(0, implicacaoMatch.Index)
                        + valorImplicacao.ToString().ToLower()
                        + expr.Substring(implicacaoMatch.Index + implicacaoMatch.Length);
                    continue;
                }

                var bicondicionalMatch = System.Text.RegularExpressions.Regex.Match(expr, @"(true|false)\s*↔\s*(true|false)");
                if (bicondicionalMatch.Success)
                {
                    var esquerda = bicondicionalMatch.Groups[1].Value == "true";
                    var direita = bicondicionalMatch.Groups[2].Value == "true";
                    var valorBicondicional = esquerda == direita; // Implementação de A ↔ B
                    expr = expr.Substring(0, bicondicionalMatch.Index)
                        + valorBicondicional.ToString().ToLower()
                        + expr.Substring(bicondicionalMatch.Index + bicondicionalMatch.Length);
                    continue;
                }

                var disjuncaoExclusivaMatch = System.Text.RegularExpressions.Regex.Match(expr, @"(true|false)\s*⊻\s*(true|false)");
                if (disjuncaoExclusivaMatch.Success)
                {
                    var esquerda = disjuncaoExclusivaMatch.Groups[1].Value == "true";
                    var direita = disjuncaoExclusivaMatch.Groups[2].Value == "true";
                    var valorDisjuncaoExclusiva = !(esquerda == direita); // Implementação de A ↔ B
                    expr = expr.Substring(0, disjuncaoExclusivaMatch.Index)
                        + valorDisjuncaoExclusiva.ToString().ToLower()
                        + expr.Substring(disjuncaoExclusivaMatch.Index + disjuncaoExclusivaMatch.Length);
                    continue;
                }

            }

            // Retorna o resultado final
            return expr.Trim() == "true";
        }
    }
}
