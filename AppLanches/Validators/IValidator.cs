using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLanches.Validators
{
    public interface IValidator
    {
        string NomeErro { get; set; }
        string EmailErro { get; set; }
        string TelefoneErro { get; set; }
        string SenhaErro { get; set; }
        Task<bool> Validar(string nome, string email,
                           string telefone, string senha);
    }
}
