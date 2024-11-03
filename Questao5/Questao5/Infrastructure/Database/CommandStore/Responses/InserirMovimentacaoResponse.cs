namespace Questao5.Infrastructure.Database.CommandStore.Responses
{
    public class InserirMovimentacaoResponse
    {
        public string IdMovimento { get; set; }
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
    }
}
