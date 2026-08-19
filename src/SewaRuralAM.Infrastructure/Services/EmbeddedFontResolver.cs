using PdfSharpCore.Fonts;

namespace SewaRuralAM.Infrastructure.Services;

// PdfSharpCore can't reliably read system fonts on Android/iOS, so PDF text is rendered
// with a font embedded directly in this assembly instead.
public class EmbeddedFontResolver : IFontResolver
{
    public string DefaultFontName => "Poppins";

    public byte[] GetFont(string faceName)
    {
        var resourceName = faceName switch
        {
            "Poppins#Bold" => "SewaRuralAM.Infrastructure.Fonts.Poppins-Bold.ttf",
            "Poppins#SemiBold" => "SewaRuralAM.Infrastructure.Fonts.Poppins-SemiBold.ttf",
            _ => "SewaRuralAM.Infrastructure.Fonts.Poppins-Regular.ttf"
        };

        var assembly = typeof(EmbeddedFontResolver).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded font resource not found: {resourceName}");

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        var faceName = isBold ? "Poppins#Bold" : "Poppins#Regular";
        return new FontResolverInfo(faceName);
    }
}
