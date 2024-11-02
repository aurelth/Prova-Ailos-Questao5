using Questao5.Application.Commands.Requests;
using Questao5.Application.Commands.Responses;
using Questao5.Infrastructure.Sqlite;
using MediatR;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Questao5.Application.Handlers
{
    public class MovimentacaoHandler : IRequestHandler<MovimentacaoRequest, MovimentacaoResponse>
    {
        private readonly DatabaseConfig _dbConfig;

        public MovimentacaoHandler(DatabaseConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<MovimentacaoResponse> Handle(MovimentacaoRequest request, CancellationToken cancellationToken)
        {
            using var connection = new SqliteConnection(_dbConfig.Name);

            // Verificação de Idempotência
            var requisicaoExistente = await connection.QueryFirstOrDefaultAsync<string>("SELECT resultado FROM idempotencia WHERE chave_idempotencia = @IdRequisicao",
                                                                                        new { request.IdRequisicao });
            if (requisicaoExistente != null)
            {
                return new MovimentacaoResponse { Mensagem = requisicaoExistente };
            }

            // Validação da conta
            var conta = await connection.QueryFirstOrDefaultAsync("SELECT * FROM contacorrente WHERE idcontacorrente = @IdContaCorrente AND ativo = 1",
                                                                  new { request.IdContaCorrente });
            if (conta == null)
            {
                return new MovimentacaoResponse { Mensagem = "INVALID_ACCOUNT" };
            }

            // Validação do valor e tipo de movimento
            if (request.Valor <= 0) return new MovimentacaoResponse { Mensagem = "INVALID_VALUE" };
            if (request.TipoMovimento != "C" && request.TipoMovimento != "D")
                return new MovimentacaoResponse { Mensagem = "INVALID_TYPE" };

            // Persistência do movimento
            var idMovimento = Guid.NewGuid().ToString();
            await connection.ExecuteAsync(
                "INSERT INTO movimento (idmovimento, idcontacorrente, datamovimento, tipomovimento, valor) VALUES (@IdMovimento, @IdContaCorrente, @DataMovimento, @TipoMovimento, @Valor)",
                new { IdMovimento = idMovimento, request.IdContaCorrente, DataMovimento = DateTime.Now.ToString("dd/MM/yyyy"), request.TipoMovimento, request.Valor });

            // Salvando a operação na tabela de idempotência
            await connection.ExecuteAsync("INSERT INTO idempotencia (chave_idempotencia, resultado) VALUES (@IdRequisicao, @Resultado)",
                                          new { request.IdRequisicao, Resultado = $"Movimentação {idMovimento} realizada com sucesso" });

            return new MovimentacaoResponse { IdMovimento = idMovimento, Mensagem = "Movimentação realizada com sucesso" };
        }
    }
}