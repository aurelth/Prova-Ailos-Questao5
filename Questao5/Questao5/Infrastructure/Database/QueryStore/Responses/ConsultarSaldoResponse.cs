namespace Questao5.Infrastructure.Database.QueryStore.Responses
{
    public class ConsultarSaldoResponse
    {
        public string NumeroConta { get; set; }
        public string NomeTitular { get; set; }
        public decimal SaldoAtual { get; set; }
        public DateTime DataHoraConsulta { get; set; }
    }
}
