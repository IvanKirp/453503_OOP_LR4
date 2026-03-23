// Партия груза - агрегирует позиции грузов
public class Shipment
{
    public List<CargoItem> Items { get; set; } = new List<CargoItem>();

    /// <summary>
    /// Добавляет позицию груза в партию
    /// </summary>
    public void AddCargoItem(Cargo cargo, int quantity)
    {
        Items.Add(new CargoItem(cargo, quantity));
    }

    /// <summary>
    /// Рассчитывает общую стоимость всех грузов в партии
    /// </summary>
    public double CalculateTotalCargoCost()
    {
        return Items.Sum(item => item.CalculateCost());
    }

    /// <summary>
    /// Рассчитывает общий вес всех грузов в партии
    /// </summary>
    public double CalculateTotalWeight()
    {
        return Items.Sum(item => item.TotalWeight);
    }

    public List<Cargo> GetCargoTypes()
    {
        return Items.Select(item => item.CargoType).ToList();
    }
}