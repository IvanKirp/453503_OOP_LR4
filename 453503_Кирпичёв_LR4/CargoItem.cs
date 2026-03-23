// Позиция груза в партии
public class CargoItem
{
    public Cargo CargoType { get; set; }
    public int Quantity { get; set; }
    public double TotalWeight => CargoType.WeightPerUnit * Quantity;

    public CargoItem(Cargo cargoType, int quantity)
    {
        CargoType = cargoType;
        Quantity = quantity;
    }

    /// <summary>
    /// Рассчитывает стоимость данной позиции груза
    /// </summary>
    public double CalculateCost()
    {
        return CargoType.CalculateCost(TotalWeight);
    }
}