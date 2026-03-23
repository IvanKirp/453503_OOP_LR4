// Главный контроллер - Controller

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
    /// Инициализирует систему загрузкой данных из файла
    /// </summary>
    public bool Initialize(string dataFilePath)
    {
        (_cargos, _transports) = _dataLoader.LoadData(dataFilePath);
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
        var shipment = new Shipment();
        foreach (var cargoReq in cargoQuantities)
        {
            // Поиск груза по имени
            var cargo = _cargos.FirstOrDefault(c =>
                string.Equals(c.Name, cargoReq.Key, StringComparison.OrdinalIgnoreCase));

            if (cargo == null)
            {
                throw new ArgumentException($"Груз '{cargoReq.Key}' не найден. Доступные грузы: " +
                    string.Join(", ", _cargos.Select(c => c.Name)));
            }

            shipment.AddCargoItem(cargo, cargoReq.Value);
        }

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

    public List<Cargo> GetAvailableCargos() => _cargos ?? new List<Cargo>();
    public List<Transport> GetAvailableTransports() => _transports ?? new List<Transport>();
}