// Создатель фабрик - демонстрирует принцип Creator
public class TransportFactoryCreator
{
    private readonly List<Transport> _transports;
    private readonly Dictionary<string, ITransportFactory> _factories;

    public TransportFactoryCreator(List<Transport> transports)
    {
        _transports = transports;

        // Базовые фабрики
        _factories = new Dictionary<string, ITransportFactory>
        {
            { "Земля", new GroundTransportFactory(_transports) },
            { "Вода", new WaterTransportFactory(_transports) },
            { "Воздух", new AirTransportFactory(_transports) }
        };
    }

    // Конструктор, который принимает дополнительные фабрики
    public TransportFactoryCreator(List<Transport> transports,
                                   Dictionary<string, ITransportFactory> additionalFactories)
        : this(transports)
    {
        // Добавляем дополнительные фабрики к существующим
        foreach (var factory in additionalFactories)
        {
            _factories[factory.Key] = factory.Value;
        }
    }

    // Метод для объединения с другим словарем
    public void MergeFactories(Dictionary<string, ITransportFactory> otherFactories)
    {
        foreach (var factory in otherFactories)
        {
            _factories[factory.Key] = factory.Value;
        }
    }

    public ITransportFactory CreateFactory(string transportType)
    {
        if (_factories.TryGetValue(transportType, out var factory))
        {
            return factory;
        }

        throw new ArgumentException($"Неизвестный тип транспорта: {transportType}");
    }
}