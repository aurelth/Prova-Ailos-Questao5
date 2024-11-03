using Microsoft.AspNetCore.Mvc;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Queries.Requests;
using Questao5.Domain.Language;
using MediatR;

namespace Questao5.Infrastructure.Services.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContaCorrenteController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("movimentacao")]
        public async Task<IActionResult> MovimentarConta([FromBody] MovimentacaoRequest request)
        {
            var response = await _mediator.Send(request);
            if (response.Mensagem == Mensagens.MovimentacaoSucesso)
            {
                return Ok(response);
            }
            return BadRequest(response.Mensagem);
        }

        [HttpGet("saldo")]
        public async Task<IActionResult> ConsultarSaldo([FromQuery] SaldoRequest request)
        {
            var response = await _mediator.Send(request);
            return Ok(response);
        }
    }
}
