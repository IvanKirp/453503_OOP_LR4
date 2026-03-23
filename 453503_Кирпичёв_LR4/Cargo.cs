public class Cargo
{
    public string Name { get; set; }
    public double WeightPerUnit { get; set; }
    public double CostPerKg { get; set; }

    public Cargo(string name, double weightPerUnit, double costPerKg)
    {
        Name = name;
        WeightPerUnit = weightPerUnit;
        CostPerKg = costPerKg;
    }

    /// <summary>
    /// Рассчитывает стоимость перевозки груза по его весу
    /// </summary>
    public double CalculateCost(double weight)
    {
        return weight * CostPerKg;
    }
}