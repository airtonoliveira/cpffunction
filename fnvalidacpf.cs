using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;

namespace ValidacaoCPF
{
    public static class CPFValidator
    {
        [FunctionName("ValidarCPF")]
        public static async Task<IActionResult> Executar(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest requisicao,
            ILogger logger)
        {
            logger.LogInformation("Processo de validação de CPF iniciado.");

            string corpoRequisicao = await new StreamReader(requisicao.Body).ReadToEndAsync();
            dynamic entrada = JsonConvert.DeserializeObject(corpoRequisicao);
            if (entrada == null || entrada.cpf == null)
            {
                return new BadRequestObjectResult("É necessário fornecer um CPF válido.");
            }

            string documento = entrada.cpf;
            
            if (!VerificarCPF(documento))
                return new BadRequestObjectResult("O CPF informado é inválido.");
            
            return new OkObjectResult("O CPF é válido.");
        }

        private static bool VerificarCPF(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                return false;

            documento = new string(documento.Where(char.IsDigit).ToArray());

            if (documento.Length != 11 || documento.Distinct().Count() == 1)
                return false;

            int[] pesosPrimeiroDigito = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma1 = documento.Take(9).Select((digito, indice) => (digito - '0') * pesosPrimeiroDigito[indice]).Sum();
            int primeiroDigito = soma1 % 11 < 2 ? 0 : 11 - (soma1 % 11);

            int[] pesosSegundoDigito = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma2 = documento.Take(10).Select((digito, indice) => (digito - '0') * pesosSegundoDigito[indice]).Sum();
            int segundoDigito = soma2 % 11 < 2 ? 0 : 11 - (soma2 % 11);

            return documento[9] - '0' == primeiroDigito && documento[10] - '0' == segundoDigito;
        }
    }
}
