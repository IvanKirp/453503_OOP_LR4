// Абстрактная фабрика для создания транспорта
public interface ITransportFactory
{
    Transport CreateTransport();
    string GetTransportType();
}