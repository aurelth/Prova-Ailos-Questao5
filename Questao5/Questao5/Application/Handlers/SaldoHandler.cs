using Questao5.Application.Queries.Requests;
using Questao5.Application.Queries.Responses;
using Questao5.Infrastructure.Sqlite;
using MediatR;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Questao5.Application.Handlers
{
    public class SaldoHandler : IRequestHandler<SaldoRequest, SaldoResponse>
    {
        private readonly DatabaseConfig _dbConfig;

        public SaldoHandler(DatabaseConfig dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<SaldoResponse> Handle(SaldoRequest request, CancellationToken cancellationToken)
        {
            using var connection = new SqliteConnection(_dbConfig.Name);

            // Consulta da conta
            var conta = await connection.QueryFirstOrDefaultAsync("SELECT numero, nome FROM contacorrente WHERE idcontacorrente = @IdContaCorrente AND ativo = 1",
                                                                  new { request.IdContaCorrente });
            if (conta == null)
            {
                return new SaldoResponse { SaldoAtual = 0, NumeroConta = "INVALID_ACCOUNT" };
            }

            // Cálculo do saldo
            var creditos = await connection.QuerySingleAsync<decimal>("SELECT IFNULL(SUM(valor), 0) FROM movimento WHERE idcontacorrente = @IdContaCorrente AND tipomovimento = 'C'",
                                                                      new { request.IdContaCorrente });
            var debitos = await connection.QuerySingleAsync<decimal>("SELECT IFNULL(SUM(valor), 0) FROM movimento WHERE idcontacorrente = @IdContaCorrente AND tipomovimento = 'D'",
                                                                     new { request.IdContaCorrente });

            var saldoAtual = creditos - debitos;

            return new SaldoResponse
            {
                NumeroConta = conta.numero.ToString(),
                NomeTitular = conta.nome,
                SaldoAtual = saldoAtual,
                DataHoraConsulta = DateTime.Now
            };
        }
    }
}
