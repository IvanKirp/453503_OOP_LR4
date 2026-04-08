// Сохранение в разных форматах с шифрованием и сжатием
// Комбинирование вариантов сохранения
// Паттерн: Декоратор

using System.IO.Compression;
using System.Text;
using System.Text.Json;

/// <summary>
/// Базовый интерфейс для экспортера
/// </summary>
public interface IExporter
{
    void Export(DeliveryResult result, string filePath);
    void Export(List<DeliveryOption> options, string filePath);
    string GetFormat();
}

/// <summary>
/// Конкретный экспортер в JSON
/// </summary>
public class JsonExporter : IExporter
{
    public void Export(DeliveryResult result, string filePath)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);
    }

    public void Export(List<DeliveryOption> options, string filePath)
    {
        var data = new { ExportDate = DateTime.Now, Options = options };
        var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);
    }

    public string GetFormat() => "JSON";
}

/// <summary>
/// Конкретный экспортер в CSV
/// </summary>
public class CsvExporter : IExporter
{
    public void Export(DeliveryResult result, string filePath)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Транспорт,Тип,Расстояние,Вес груза,Стоимость,Время доставки (ч)");
        csv.AppendLine($"{result.Transport.Name},{result.Transport.Type},{result.Distance}," +
                      $"{result.Shipment.CalculateTotalWeight()},{result.TotalCost}," +
                      $"{result.DeliveryTimeHours}");

        File.WriteAllText(filePath, csv.ToString());
    }

    public void Export(List<DeliveryOption> options, string filePath)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Транспорт,Тип,Скорость (км/ч),Стоимость за км,Общая стоимость,Время доставки (ч),Вес груза (кг)");

        foreach (var opt in options)
        {
            csv.AppendLine($"{opt.TransportName},{opt.TransportType},{opt.SpeedKmh}," +
                          $"{opt.CostPerKm},{opt.TotalCost},{opt.DeliveryTimeHours}," +
                          $"{opt.TotalWeight}");
        }

        File.WriteAllText(filePath, csv.ToString());
    }

    public string GetFormat() => "CSV";
}

/// <summary>
/// Базовый декоратор для экспортеров
/// </summary>
public abstract class ExporterDecorator : IExporter
{
    protected readonly IExporter _inner;

    protected ExporterDecorator(IExporter inner)
    {
        _inner = inner;
    }

    public abstract void Export(DeliveryResult result, string filePath);
    public abstract void Export(List<DeliveryOption> options, string filePath);
    public abstract string GetFormat();
}

/// <summary>
/// Декоратор для шифрования
/// </summary>
public class EncryptionExporterDecorator : ExporterDecorator
{
    private readonly string _password;

    public EncryptionExporterDecorator(IExporter inner, string password) : base(inner)
    {
        _password = password;
    }

    public override void Export(DeliveryResult result, string filePath)
    {
        var tempFile = Path.GetTempFileName();
        _inner.Export(result, tempFile);

        var encryptedData = EncryptFile(tempFile, _password);
        File.WriteAllBytes(filePath, encryptedData);

        File.Delete(tempFile);
    }

    public override void Export(List<DeliveryOption> options, string filePath)
    {
        var tempFile = Path.GetTempFileName();
        _inner.Export(options, tempFile);

        var encryptedData = EncryptFile(tempFile, _password);
        File.WriteAllBytes(filePath, encryptedData);

        File.Delete(tempFile);
    }

    private byte[] EncryptFile(string filePath, string password)
    {
        var data = File.ReadAllBytes(filePath);
        var key = System.Text.Encoding.UTF8.GetBytes(password);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(data[i] ^ key[i % key.Length]);
        }

        return data;
    }

    public override string GetFormat() => $"{_inner.GetFormat()}+Encrypted";
}

/// <summary>
/// Декоратор для сжатия в ZIP
/// </summary>
public class ZipExporterDecorator : ExporterDecorator
{
    public ZipExporterDecorator(IExporter inner) : base(inner) { }

    public override void Export(DeliveryResult result, string filePath)
    {
        var zipPath = Path.ChangeExtension(filePath, ".zip");
        var tempFile = Path.GetTempFileName();

        _inner.Export(result, tempFile);

        using (var zip = System.IO.Compression.ZipFile.Open(zipPath, System.IO.Compression.ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(tempFile, Path.GetFileName(filePath));
        }

        File.Delete(tempFile);

        if (zipPath != filePath)
        {
            File.Move(zipPath, filePath, true);
        }
    }

    public override void Export(List<DeliveryOption> options, string filePath)
    {
        var zipPath = Path.ChangeExtension(filePath, ".zip");
        var tempFile = Path.GetTempFileName();

        _inner.Export(options, tempFile);

        using (var zip = System.IO.Compression.ZipFile.Open(zipPath, System.IO.Compression.ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(tempFile, Path.GetFileName(filePath));
        }

        File.Delete(tempFile);

        if (zipPath != filePath)
        {
            File.Move(zipPath, filePath, true);
        }
    }

    public override string GetFormat() => $"{_inner.GetFormat()}+Zipped";
}

/// <summary>
/// Фабрика для создания комбинированных экспортеров
/// </summary>
public class ExporterFactory
{
    public IExporter CreateExporter(string format, bool encrypt = false, bool compress = false, string password = null)
    {
        IExporter exporter = format.ToLower() switch
        {
            "json" => new JsonExporter(),
            "csv" => new CsvExporter(),
            _ => throw new NotSupportedException($"Формат {format} не поддерживается")
        };

        if (compress)
        {
            exporter = new ZipExporterDecorator(exporter);
        }

        if (encrypt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Для шифрования нужен пароль");
            exporter = new EncryptionExporterDecorator(exporter, password);
        }

        return exporter;
    }
}