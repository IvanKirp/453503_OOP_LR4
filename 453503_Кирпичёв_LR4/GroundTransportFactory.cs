public class GroundTransportFactory : ITransportFactory
{
    private readonly List<Transport> _availableGroundTransports;

    public GroundTransportFactory(List<Transport> transports)
    {
        _availableGroundTransports = transports.Where(t => t.Type == "Земля").ToList();
    }

    public Transport CreateTransport()
    {
        return _availableGroundTransports.FirstOrDefault();
    }

    public string GetTransportType() => "Земля";
}