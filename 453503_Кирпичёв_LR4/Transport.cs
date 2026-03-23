public class Transport
{
    public string Name { get; set; }
    public string Type { get; set; }
    public double CostPerKm { get; set; }
    public double SpeedKmh { get; set; }

    public Transport(string name, string type, double costPerKm, double speedKmh)
    {
        Name = name;
        Type = type;
        CostPerKm = costPerKm;
        SpeedKmh = speedKmh;
    }

    /// <summary>
    /// Рассчитывает стоимость доставки на заданное расстояние
    /// </summary>
    public double CalculateDeliveryCost(double distance)
    {
        return distance * CostPerKm;
    }

    /// <summary>
    /// Рассчитывает время доставки на заданное расстояние
    /// </summary>
    public double CalculateDeliveryTime(double distance)
    {
        return distance / SpeedKmh;
    }
}