// Загрузчик данных - Pure Fabrication класс
using System.Globalization;
using System.Xml.Linq;

public class DataLoader
{
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    public (List<Cargo> cargos, List<Transport> transports) LoadData(string filePath)
    {
        var cargos = new List<Cargo>();
        var transports = new List<Transport>();

        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Файл не найден: {filePath}");
                Console.WriteLine($"Текущая директория: {Environment.CurrentDirectory}");
                return (cargos, transports);
            }

            var lines = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);

            if (lines.Length == 0)
            {
                Console.WriteLine("Файл пуст");
                return (cargos, transports);
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(';');
                if (parts.Length < 6)
                {
                    Console.WriteLine($"Пропускаем некорректную строку {i}: {line}");
                    continue;
                }

                var type = parts[0].Trim();
                var name = parts[1].Trim();

                if (type == "cargo")
                {
                    if (parts.Length >= 4)
                    {
                        double weightPerUnit = ParseDouble(parts[2]);
                        double costPerKg = ParseDouble(parts[3]);
                        cargos.Add(new Cargo(name, weightPerUnit, costPerKg));
                        Console.WriteLine($"Загружен груз: {name}, вес ед.: {weightPerUnit:F2} кг, стоимость за кг: {costPerKg:F2}");
                    }
                }
                else if (type == "transport")
                {
                    string transportType = DetermineTransportType(name);
                    if (parts.Length >= 6)
                    {
                        double costPerKm = ParseDouble(parts[4]);
                        double speedKmh = ParseDouble(parts[5]);
                        transports.Add(new Transport(name, transportType, costPerKm, speedKmh));
                        Console.WriteLine($"Загружен транспорт: {name}, тип: {transportType}, расход на км: {costPerKm:F2}, скорость: {speedKmh:F2} км/ч");
                    }
                }
                else
                {
                    Console.WriteLine($"Неизвестный тип: {type} в строке {i}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            Console.WriteLine($"Стек вызовов: {ex.StackTrace}");
        }

        Console.WriteLine($"\nВсего загружено: {cargos.Count} грузов, {transports.Count} транспортных средств");
        return (cargos, transports);
    }

    /// <summary>
    /// Определяет тип транспорта
    /// </summary>
    private string DetermineTransportType(string name)
    {
        int startIndex = name.IndexOf('(');
        int endIndex = name.IndexOf(')');

        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            string type = name.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();

            return type;
        }

        // Если не удалось извлечь из скобок, запасной вариант:
        if (name.Contains("Земля") || name.Contains("Грузовик") || name.Contains("Поезд"))
            return "Земля";
        if (name.Contains("Вода") || name.Contains("Танкер") || name.Contains("Корабль"))
            return "Вода";
        if (name.Contains("Воздух") || name.Contains("Самолет") || name.Contains("Вертолет"))
            return "Воздух";

        return "";
    }

    private double ParseDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;

        value = value.Trim();

        if (double.TryParse(value, NumberStyles.Any, InvariantCulture, out double result))
            return result;

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            return result;

        value = value.Replace('.', ',');
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            return result;

        Console.WriteLine($"Предупреждение: не удалось преобразовать '{value}' в число, возвращаем 0");
        return 0;
    }
}





// Прием файлов в разных форматах (JSON, XML, CSV)
// Паттерн: Адаптер + Фасад

/// <summary>
/// Целевой интерфейс, который ожидает наша система
/// </summary>
public interface IDataLoader
{
    (List<Cargo> cargos, List<Transport> transports) LoadData(string filePath);
}

/// <summary>
/// Адаптер для CSV (уже существующий DataLoader, но приведем к общему интерфейсу)
/// </summary>
public class CsvDataLoaderAdapter : IDataLoader
{
    private readonly DataLoader _csvLoader = new DataLoader();

    public (List<Cargo> cargos, List<Transport> transports) LoadData(string filePath)
    {
        return _csvLoader.LoadData(filePath);
    }
}

public class JsonDataLoaderAdapter : IDataLoader
{
    public (List<Cargo> cargos, List<Transport> transports) LoadData(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var data = System.Text.Json.JsonSerializer.Deserialize<JsonDataFormat>(json);
        return ConvertFromJson(data);
    }

    private (List<Cargo>, List<Transport>) ConvertFromJson(JsonDataFormat data)
    {
        var cargos = data.Cargos.Select(c => new Cargo(c.Name, c.WeightPerUnit, c.CostPerKg)).ToList();
        var transports = data.Transports.Select(t => new Transport(t.Name, t.Type, t.CostPerKm, t.SpeedKmh)).ToList();
        return (cargos, transports);
    }
}

/// <summary>
/// DTO для десериализации JSON
/// </summary>
public class JsonDataFormat
{
    public List<JsonCargo> Cargos { get; set; }
    public List<JsonTransport> Transports { get; set; }
}

public class JsonCargo
{
    public string Name { get; set; }
    public double WeightPerUnit { get; set; }
    public double CostPerKg { get; set; }
}

public class JsonTransport
{
    public string Name { get; set; }
    public string Type { get; set; }
    public double CostPerKm { get; set; }
    public double SpeedKmh { get; set; }
}

/// <summary>
/// Адаптер для XML
/// </summary>
public class XmlDataLoaderAdapter : IDataLoader
{
    public (List<Cargo> cargos, List<Transport> transports) LoadData(string filePath)
    {
        var xml = File.ReadAllText(filePath);
        return ParseXml(xml);
    }

    private (List<Cargo> cargos, List<Transport> transports) ParseXml(string xml)
    {
        var cargos = new List<Cargo>();
        var transports = new List<Transport>();

        var doc = XDocument.Parse(xml);

        var cargoElements = doc.Descendants("Cargo");
        foreach (var cargoElem in cargoElements)
        {
            var name = cargoElem.Element("Name")?.Value ?? "";
            var weightPerUnit = double.Parse(cargoElem.Element("WeightPerUnit")?.Value ?? "0",
                System.Globalization.CultureInfo.InvariantCulture);
            var costPerKg = double.Parse(cargoElem.Element("CostPerKg")?.Value ?? "0",
                System.Globalization.CultureInfo.InvariantCulture);

            cargos.Add(new Cargo(name, weightPerUnit, costPerKg));
        }

        var transportElements = doc.Descendants("Transport");
        foreach (var transportElem in transportElements)
        {
            var name = transportElem.Element("Name")?.Value ?? "";
            var type = transportElem.Element("Type")?.Value ?? "";
            var costPerKm = double.Parse(transportElem.Element("CostPerKm")?.Value ?? "0",
                System.Globalization.CultureInfo.InvariantCulture);
            var speedKmh = double.Parse(transportElem.Element("SpeedKmh")?.Value ?? "0",
                System.Globalization.CultureInfo.InvariantCulture);

            transports.Add(new Transport(name, type, costPerKm, speedKmh));
        }

        return (cargos, transports);
    }
}

/// <summary>
/// Фасад для упрощения загрузки данных из разных форматов
/// </summary>
public class DataLoaderFacade
{
    private readonly Dictionary<string, IDataLoader> _loaders;

    public DataLoaderFacade()
    {
        _loaders = new Dictionary<string, IDataLoader>
        {
            { ".csv", new CsvDataLoaderAdapter() },
            { ".json", new JsonDataLoaderAdapter() },
            { ".xml", new XmlDataLoaderAdapter() }
        };
    }

    public (List<Cargo> cargos, List<Transport> transports) LoadData(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();

        if (_loaders.TryGetValue(extension, out var loader))
        {
            return loader.LoadData(filePath);
        }

        throw new NotSupportedException($"Формат {extension} не поддерживается");
    }
}