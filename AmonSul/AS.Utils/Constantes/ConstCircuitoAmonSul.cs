using System.Globalization;
using System.Text;

namespace AS.Utils.Constantes;

/// <summary>
/// El circuito Amon Sûl de cada año: todos los eventos suman en el del año en que empiezan, sin poder quitarlos.
/// Lo crea la web a nombre de la administración y su nombre queda reservado
/// </summary>
public static class ConstCircuitoAmonSul
{
    public const int IdUsuarioCreador = 168;

    public static string Nombre(int anio) => $"Amon Sûl {anio}";

    // Sin distinguir mayúsculas ni tildes, como la columna: «Amon Sul 2027» también lo es
    public static bool EsAmonSul(string? nombre) =>
        string.Concat((nombre ?? "").Trim().Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark))
            .StartsWith("amon sul", StringComparison.OrdinalIgnoreCase);
}
