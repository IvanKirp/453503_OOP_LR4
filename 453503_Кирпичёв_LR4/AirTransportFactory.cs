public class AirTransportFactory : ITransportFactory
{
    private readonly List<Transport> _availableAirTransports;

    public AirTransportFactory(List<Transport> transports)
    {
        _availableAirTransports = transports.Where(t => t.Type == "Воздух").ToList();
    }

    public Transport CreateTransport()
    {
        return _availableAirTransports.FirstOrDefault();
    }

    public string GetTransportType() => "Воздух";
}