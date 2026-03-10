using AS.Domain.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace AS.Application.Utils;

public static class PdfListaGenerator
{
    public static byte[] GenerateListasPdf(
        List<Lista> listas)
    {
        using var document = new PdfDocument();

        const double margin = 10.0;
        var titleFont = new XFont("Arial", 14, XFontStyle.Bold);

        foreach (var lista in listas)
        {
            // Una página por cada lista
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // Nombre de la lista justo encima de la imagen
            var title = $"Lista de {lista?.IdInscripcionNavigation?.IdUsuarioNavigation?.Nick}";
            var titleSize = gfx.MeasureString(title, titleFont);
            double yTitle = margin;
            gfx.DrawString(title, titleFont, XBrushes.Black, new XRect(margin, yTitle, page.Width - 2 * margin, titleSize.Height), XStringFormats.TopCenter);

            // Punto de inicio de la imagen (debajo del título)
            double y = yTitle + titleSize.Height + 6.0; // pequeño espacio entre título e imagen

            // Si no hay imagen, dejamos la página con solo el título
            if (string.IsNullOrWhiteSpace(lista?.ListaData))
                continue;

            try
            {
                var base64 = lista.ListaData;
                var commaIndex = base64.IndexOf(',');
                if (commaIndex >= 0)
                    base64 = base64[(commaIndex + 1)..];

                var imageBytes = Convert.FromBase64String(base64);

                using var xImage = XImage.FromStream(() => new MemoryStream(imageBytes));

                // Convertir tamaño en píxeles a puntos (1 punto = 1/72 inch)
                double horizRes = xImage.HorizontalResolution > 0 ? xImage.HorizontalResolution : 96.0;
                double vertRes = xImage.VerticalResolution > 0 ? xImage.VerticalResolution : 96.0;
                double imgWidthPts = xImage.PixelWidth * 72.0 / horizRes;
                double imgHeightPts = xImage.PixelHeight * 72.0 / vertRes;

                double maxWidth = page.Width - 2 * margin;
                double maxHeight = page.Height - y - margin;

                // Escalar para ocupar la mayor área posible manteniendo la relación de aspecto.
                // No forzamos upscaling mayor que 1 para preservar calidad native; si prefiere escalar hasta rellenar, elimine el Math.Min(1.0,...)
                double ratio = Math.Min(maxWidth / imgWidthPts, maxHeight / imgHeightPts);
                ratio = double.IsInfinity(ratio) || ratio <= 0 ? 1.0 : Math.Min(1.0, ratio);

                double drawWidth = imgWidthPts * ratio;
                double drawHeight = imgHeightPts * ratio;

                // Centrar la imagen horizontalmente y colocarla bajo el título
                double x = (page.Width - drawWidth) / 2.0;
                double yImage = y + ((maxHeight - drawHeight) / 2.0); // centrar verticalmente en el espacio restante

                gfx.DrawImage(xImage, x, yImage, drawWidth, drawHeight);
            }
            catch
            {
                // Si falla la imagen, dejamos la página con el título (solicitud: solo título + imagen por página)
                continue;
            }
        }

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }
}
