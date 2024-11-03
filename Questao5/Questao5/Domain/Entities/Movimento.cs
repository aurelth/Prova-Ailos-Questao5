using Questao5.Domain.Enumerators;

namespace Questao5.Domain.Entities
{
    public class Movimento
    {
        public string IdMovimento { get; set; }
        public string IdContaCorrente { get; set; }
        public DateTime DataMovimento { get; set; }
        public TipoMovimentoEnum TipoMovimento { get; set; }
        public decimal Valor { get; set; }
    }
}
