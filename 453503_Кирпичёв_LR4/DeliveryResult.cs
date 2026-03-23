public class DeliveryResult
{
    public Shipment Shipment { get; set; }
    public Transport Transport { get; set; }
    public double TotalCost { get; set; }
    public double DeliveryTimeHours { get; set; }
    public double Distance { get; set; }

    /// <summary>
    /// Отображает результат расчета
    /// </summary>
    public void Display()
    {
        Console.WriteLine("\nРезультат расчёта доставки:");
        Console.WriteLine($"Транспорт: {Transport.Name} ({Transport.Type})");
        Console.WriteLine($"Расстояние: {Distance:F2} км");
        Console.WriteLine($"Общий вес груза: {Shipment.CalculateTotalWeight():F2} кг");
        Console.WriteLine($"Общая стоимость: {TotalCost:F2} ед.");
        Console.WriteLine($"Время доставки: {DeliveryTimeHours:F2} ч ({DeliveryTimeHours / 24:F1} дн.)");

        Console.WriteLine("\nДетали груза:");
        foreach (var item in Shipment.Items)
        {
            Console.WriteLine($"  - {item.CargoType.Name}: {item.Quantity} ед., " +
                              $"{item.TotalWeight:F2} кг, " +
                              $"Стоимость: {item.CalculateCost():F2} ед.");
        }
    }
}