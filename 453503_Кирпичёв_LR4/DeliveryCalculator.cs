// Калькулятор доставки - Information Expert
public class DeliveryCalculator
{
    /// <summary>
    /// Рассчитывает общую стоимость доставки
    /// </summary>
    public double CalculateTotalCost(Shipment shipment, Transport transport, double distance)
    {
        // Information Expert: этот класс имеет всю необходимую информацию для расчета
        double cargoCost = shipment.CalculateTotalCargoCost();
        double deliveryCost = transport.CalculateDeliveryCost(distance);

        return cargoCost + deliveryCost;
    }

    /// <summary>
    /// Рассчитывает время доставки
    /// </summary>
    public double CalculateDeliveryTime(Transport transport, double distance)
    {
        return transport.CalculateDeliveryTime(distance);
    }
}