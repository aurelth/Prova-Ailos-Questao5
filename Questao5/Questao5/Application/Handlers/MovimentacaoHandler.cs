using Questao5.Application.Commands.Requests;
using Questao5.Application.Commands.Responses;
using Questao5.Infrastructure.Database.CommandStore.Requests;
using Questao5.Infrastructure.Sqlite;
using Questao5.Domain.Enumerators;
using Questao5.Domain.Language;
using MediatR;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Questao5.Application.Handlers
{
    public class MovimentacaoHandler : IRequestHandler<MovimentacaoRequest, MovimentacaoResponse>
    {
        private readonly DatabaseConfig _dbConfig;
        private readonly ILogger<MovimentacaoHandler> _logger;

        public MovimentacaoHandler(IOptions<DatabaseConfig> dbConfig, ILogger<MovimentacaoHandler> logger)
        {
            _dbConfig = dbConfig.Value;
            _logger = logger;
        }

        public async Task<MovimentacaoResponse> Handle(MovimentacaoRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Verificando conta com ID: {IdContaCorrente}", request.IdContaCorrente);

            using var connection = new SqliteConnection(_dbConfig.Name);

            // Verificação de Idempotência
            var requisicaoExistente = await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT resultado FROM idempotencia WHERE chave_idempotencia = @IdRequisicao",
                new { request.IdRequisicao });
            if (requisicaoExistente != null)
            {
                return new MovimentacaoResponse { Mensagem = requisicaoExistente };
            }

            // Validação da conta
            var conta = await connection.QueryFirstOrDefaultAsync(
                "SELECT * FROM contacorrente WHERE idcontacorrente = @IdContaCorrente AND ativo = 1",
                new { request.IdContaCorrente });
            if (conta == null)
            {
                _logger.LogWarning("Conta não encontrada ou inativa para ID: {IdContaCorrente}", request.IdContaCorrente);
                return new MovimentacaoResponse { Mensagem = Mensagens.ContaInvalida };
            }

            _logger.LogInformation("Conta encontrada e ativa para ID: {IdContaCorrente}", request.IdContaCorrente);

            // Validação do valor e tipo de movimento
            if (request.Valor <= 0)
                return new MovimentacaoResponse { Mensagem = Mensagens.ValorInvalido };
            if (request.TipoMovimento != TipoMovimentoEnum.Credito && request.TipoMovimento != TipoMovimentoEnum.Debito)
            {
                return new MovimentacaoResponse { Mensagem = Mensagens.TipoInvalido };
            }

            // Preparação da requisição para inserir a movimentação
            var inserirMovimentacaoRequest = new InserirMovimentacaoRequest
            {
                IdMovimento = Guid.NewGuid().ToString(),
                IdContaCorrente = request.IdContaCorrente,
                DataMovimento = DateTime.Now,
                TipoMovimento = request.TipoMovimento,
                Valor = request.Valor
            };

            // Persistência do movimento no banco
            await connection.ExecuteAsync(
                "INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor) VALUES (@IdMovimento, @IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor)",
                new
                {
                    inserirMovimentacaoRequest.IdMovimento,
                    inserirMovimentacaoRequest.IdContaCorrente,
                    DataMovimento = inserirMovimentacaoRequest.DataMovimento.ToString("dd/MM/yyyy"),
                    TipoMovimento = inserirMovimentacaoRequest.TipoMovimento == TipoMovimentoEnum.Credito ? "C" : "D",
                    inserirMovimentacaoRequest.Valor
                });

            // Salvando a operação na tabela de idempotência
            await connection.ExecuteAsync(
                "INSERT INTO idempotencia (chave_idempotencia, resultado) VALUES (@IdRequisicao, @Resultado)",
                new { request.IdRequisicao, Resultado = Mensagens.MovimentacaoSucesso });

            return new MovimentacaoResponse
            {
                IdMovimento = inserirMovimentacaoRequest.IdMovimento,
                Mensagem = Mensagens.MovimentacaoSucesso
            };
        }
    }
}
