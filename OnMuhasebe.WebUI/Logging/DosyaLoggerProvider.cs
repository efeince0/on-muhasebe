namespace OnMuhasebe.WebUI.Logging;

/// <summary>
/// Harici bir pakete (Serilog vb.) ihtiyac duymadan "teknik hatalar loglanmalidir"
/// gereksinimini karsilayan, bagimliliksiz basit dosya loglayici.
/// Sadece Warning ve uzeri seviyeleri, gun bazli bir .log dosyasina yazar.
/// ASP.NET Core'un UseExceptionHandler middleware'i beklenmeyen her hatayi zaten
/// Error seviyesinde logluyor; bu provider'i ekleyince o kayit konsolun yaninda
/// kalici bir dosyaya da duser.
/// </summary>
public sealed class DosyaLoggerProvider : ILoggerProvider
{
    private readonly string _klasor;
    private readonly object _kilit = new();

    public DosyaLoggerProvider(string klasor)
    {
        _klasor = klasor;
        Directory.CreateDirectory(_klasor);
    }

    public ILogger CreateLogger(string kategoriAdi) => new DosyaLogger(kategoriAdi, _klasor, _kilit);

    public void Dispose() { }

    private sealed class DosyaLogger(string kategoriAdi, string klasor, object kilit) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        // Bilgi/Debug seviyesi konsolda kalsin; dosyaya sadece uyari ve hata dusun.
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var satir = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {kategoriAdi}: {formatter(state, exception)}";
            if (exception is not null)
                satir += Environment.NewLine + exception;

            var dosyaYolu = Path.Combine(klasor, $"hata-{DateTime.Now:yyyyMMdd}.log");

            try
            {
                lock (kilit)
                {
                    File.AppendAllText(dosyaYolu, satir + Environment.NewLine);
                }
            }
            catch
            {
                // Loglama basarisiz olsa bile uygulama akisini bozmamali.
            }
        }
    }
}
