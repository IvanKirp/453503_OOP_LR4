public class WaterTransportFactory : ITransportFactory
{
    private readonly List<Transport> _availableWaterTransports;

    public WaterTransportFactory(List<Transport> transports)
    {
        _availableWaterTransports = transports.Where(t => t.Type == "Вода").ToList();
    }

    public Transport CreateTransport()
    {
        return _availableWaterTransports.FirstOrDefault();
    }

    public string GetTransportType() => "Вода";
}