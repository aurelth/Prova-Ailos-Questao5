using System.Text.Json.Serialization;

namespace Questao5.Domain.Enumerators
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TipoMovimentoEnum
    {
        Credito,
        Debito
    }
}
