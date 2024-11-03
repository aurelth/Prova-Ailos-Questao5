using MediatR;
using Questao5.Application.Commands.Responses;
using Questao5.Domain.Enumerators;

namespace Questao5.Application.Commands.Requests
{
    public class MovimentacaoRequest : IRequest<MovimentacaoResponse>
    {
        public string IdRequisicao { get; set; }
        public string IdContaCorrente { get; set; }
        public decimal Valor { get; set; }
        public TipoMovimentoEnum TipoMovimento { get; set; }
    }
}
