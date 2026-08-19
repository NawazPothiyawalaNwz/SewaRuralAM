namespace SewaRuralAM.App.Services;

public static class PdfFileHelper
{
    // Saves the PDF to app storage and hands it to the OS's default viewer via the
    // platform share/open sheet — works the same way on Android, iOS, and Windows.
    public static async Task<string> SaveAndOpenAsync(byte[] pdfBytes, string fileNamePrefix)
    {
        var fileName = $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        await File.WriteAllBytesAsync(filePath, pdfBytes);

        try
        {
            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Could not auto-open PDF: {ex}");
        }

        return filePath;
    }
}
