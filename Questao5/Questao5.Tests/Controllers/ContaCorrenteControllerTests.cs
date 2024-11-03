using Moq;
using Xunit;
using Questao5.Infrastructure.Services.Controllers;
using Questao5.Application.Commands.Requests;
using Questao5.Application.Queries.Requests;
using Questao5.Application.Commands.Responses;
using Questao5.Application.Queries.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Questao5.Tests.Controllers
{
    public class ContaCorrenteControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ContaCorrenteController _controller;

        public ContaCorrenteControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ContaCorrenteController(_mediatorMock.Object);
        }

        [Fact]
        public async Task MovimentarConta_DeveRetornarOkResult_QuandoMovimentacaoSucesso()
        {
            // Arrange
            var request = new MovimentacaoRequest
            {
                IdRequisicao = "unique-request-id",
                IdContaCorrente = "B6BAFC09-6967-ED11-A567-055DFA4A16C9",
                Valor = 100.0M,
                TipoMovimento = Questao5.Domain.Enumerators.TipoMovimentoEnum.Credito
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<MovimentacaoRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MovimentacaoResponse { Mensagem = "Movimentação realizada com sucesso." });

            // Act
            var result = await _controller.MovimentarConta(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Movimentação realizada com sucesso.", okResult.Value);
        }

        [Fact]
        public async Task MovimentarConta_DeveRetornarBadRequest_QuandoContaInvalida()
        {
            // Arrange
            var request = new MovimentacaoRequest
            {
                IdRequisicao = "invalid-request-id",
                IdContaCorrente = "invalid-id",
                Valor = 100.0M,
                TipoMovimento = Questao5.Domain.Enumerators.TipoMovimentoEnum.Credito
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<MovimentacaoRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MovimentacaoResponse { Mensagem = "Conta inválida ou inativa." });

            // Act
            var result = await _controller.MovimentarConta(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Conta inválida ou inativa.", badRequestResult.Value);
        }

        [Fact]
        public async Task ConsultarSaldo_DeveRetornarOkResult_QuandoContaValida()
        {
            // Arrange
            var request = new SaldoRequest
            {
                IdContaCorrente = "B6BAFC09-6967-ED11-A567-055DFA4A16C9"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<SaldoRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SaldoResponse
                {
                    NumeroConta = "123",
                    NomeTitular = "Katherine Sanchez",
                    SaldoAtual = 500.0M,
                    DataHoraConsulta = System.DateTime.Now
                });

            // Act
            var result = await _controller.ConsultarSaldo(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var saldoResponse = Assert.IsType<SaldoResponse>(okResult.Value);
            Assert.Equal("123", saldoResponse.NumeroConta);
            Assert.Equal("Katherine Sanchez", saldoResponse.NomeTitular);
        }

        [Fact]
        public async Task ConsultarSaldo_DeveRetornarBadRequest_QuandoContaInvalida()
        {
            // Arrange
            var request = new SaldoRequest
            {
                IdContaCorrente = "invalid-id"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<SaldoRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SaldoResponse)null);

            // Act
            var result = await _controller.ConsultarSaldo(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Conta inválida ou inativa.", badRequestResult.Value);
        }
    }
}