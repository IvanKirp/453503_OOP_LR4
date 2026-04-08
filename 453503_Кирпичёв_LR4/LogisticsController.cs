using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Главный контроллер - паттерн Controller
/// </summary>
public class LogisticsController
{
    private readonly DataLoader _dataLoader;
    private readonly DeliveryCalculator _calculator;
    private List<Cargo> _cargos;
    private List<Transport> _transports;

    public LogisticsController()
    {
        _dataLoader = new DataLoader();
        _calculator = new DeliveryCalculator();
    }

    /// <summary>
    /// Инициализирует систему загрузкой данных из файла (CSV формат)
    /// </summary>
    public bool Initialize(string dataFilePath)
    {
        (_cargos, _transports) = _dataLoader.LoadData(dataFilePath);
        return _cargos.Count > 0 && _transports.Count > 0;
    }

    /// <summary>
    /// Инициализирует систему через фасад (поддержка JSON, XML, CSV)
    /// </summary>
    public bool InitializeWithFacade(DataLoaderFacade facade, string dataFilePath)
    {
        (_cargos, _transports) = facade.LoadData(dataFilePath);
        return _cargos.Count > 0 && _transports.Count > 0;
    }

    /// <summary>
    /// Обрабатывает запрос на доставку
    /// </summary>
    public DeliveryResult ProcessDelivery(
        Dictionary<string, int> cargoQuantities,
        string transportType,
        double distance)
    {
        // Проверка входных данных
        if (_cargos == null || _transports == null)
            throw new InvalidOperationException("Система не инициализирована. Вызовите Initialize().");

        if (_cargos.Count == 0)
            throw new InvalidOperationException("Нет данных о грузах. Проверьте файл данных.");

        if (_transports.Count == 0)
            throw new InvalidOperationException("Нет данных о транспорте. Проверьте файл данных.");

        // Создание партии груза
        var shipment = CreateShipment(cargoQuantities);

        // Создание транспорта через фабрику
        var factoryCreator = new TransportFactoryCreator(_transports);
        var factory = factoryCreator.CreateFactory(transportType);
        var transport = factory.CreateTransport();

        if (transport == null)
            throw new ArgumentException($"Нет доступного транспорта для типа: {transportType}");

        // Расчет стоимости и времени
        double totalCost = _calculator.CalculateTotalCost(shipment, transport, distance);
        double deliveryTime = _calculator.CalculateDeliveryTime(transport, distance);

        return new DeliveryResult
        {
            Shipment = shipment,
            Transport = transport,
            TotalCost = totalCost,
            DeliveryTimeHours = deliveryTime,
            Distance = distance
        };
    }


    /// <summary>
    /// Получение всех возможных вариантов доставки, когда тип транспорта не указан
    /// </summary>
    public List<DeliveryOption> GetAllPossibleDeliveryOptions(Dictionary<string, int> cargoQuantities, double distance)
    {
        var options = new List<DeliveryOption>();
        var shipment = CreateShipment(cargoQuantities);
        var totalWeight = shipment.CalculateTotalWeight();

        foreach (var transport in _transports)
        {
            var totalCost = _calculator.CalculateTotalCost(shipment, transport, distance);
            var deliveryTime = _calculator.CalculateDeliveryTime(transport, distance);

            options.Add(new DeliveryOption
            {
                TransportName = transport.Name,
                TransportType = transport.Type,
                SpeedKmh = transport.SpeedKmh,
                CostPerKm = transport.CostPerKm,
                TotalCost = totalCost,
                DeliveryTimeHours = deliveryTime,
                TotalWeight = totalWeight
            });
        }

        return options;
    }

    /// <summary>
    /// Получение вариантов доставки с фильтрацией и сортировкой
    /// </summary>
    public List<DeliveryOption> GetDeliveryOptionsWithFilterAndSort(
        Dictionary<string, int> cargoQuantities,
        double distance,
        List<IFilterStrategy> filters = null,
        List<ISortStrategy> sortStrategies = null)
    {
        var options = GetAllPossibleDeliveryOptions(cargoQuantities, distance);

        // Применяем фильтры
        if (filters != null && filters.Any())
        {
            options = options.Where(opt => filters.All(f => f.IsMatch(opt))).ToList();
        }

        // Применяем сортировки (комбинирование)
        if (sortStrategies != null && sortStrategies.Any())
        {
            IOrderedEnumerable<DeliveryOption> sorted = null;

            foreach (var strategy in sortStrategies)
            {
                if (sorted == null)
                    sorted = strategy.Sort(options);
                else
                    sorted = strategy.ThenSort(sorted);
            }

            options = sorted.ToList();
        }

        return options;
    }

    /// <summary>
    /// Экспорт результата доставки в файл
    /// Поддерживаются форматы JSON и CSV, шифрование и сжатие
    /// </summary>
    public void ExportDeliveryResultToFile(DeliveryResult result, string filePath, string format,
                                                bool encrypt = false, bool compress = false, string password = null)
    {
        var exporterFactory = new ExporterFactory();
        var exporter = exporterFactory.CreateExporter(format, encrypt, compress, password);
        exporter.Export(result, filePath);
        Console.WriteLine($"Результат экспортирован в {filePath} (формат: {exporter.GetFormat()})");
    }

    /// <summary>
    /// Экспорт списка вариантов доставки в файл
    /// Поддерживаются форматы JSON и CSV, шифрование и сжатие
    /// </summary>
    public void ExportDeliveryOptionsToFile(List<DeliveryOption> options, string filePath,
                                                string format, bool encrypt = false, bool compress = false, string password = null)
    {
        var exporterFactory = new ExporterFactory();
        var exporter = exporterFactory.CreateExporter(format, encrypt, compress, password);
        exporter.Export(options, filePath);
        Console.WriteLine($"Список вариантов экспортирован в {filePath} (формат: {exporter.GetFormat()})");
    }

    /// <summary>
    /// Получение списка доступных грузов
    /// </summary>
    public List<Cargo> GetAvailableCargos() => _cargos ?? new List<Cargo>();

    /// <summary>
    /// Получение списка доступного транспорта
    /// </summary>
    public List<Transport> GetAvailableTransports() => _transports ?? new List<Transport>();

    /// <summary>
    /// Создает партию груза на основе словаря с количествами
    /// </summary>
    private Shipment CreateShipment(Dictionary<string, int> cargoQuantities)
    {
        var shipment = new Shipment();
        foreach (var cargoReq in cargoQuantities)
        {
            var cargo = _cargos.FirstOrDefault(c =>
                string.Equals(c.Name, cargoReq.Key, StringComparison.OrdinalIgnoreCase));

            if (cargo == null)
                throw new ArgumentException($"Груз '{cargoReq.Key}' не найден. Доступные грузы: " +
                    string.Join(", ", _cargos.Select(c => c.Name)));

            shipment.AddCargoItem(cargo, cargoReq.Value);
        }
        return shipment;
    }
}
