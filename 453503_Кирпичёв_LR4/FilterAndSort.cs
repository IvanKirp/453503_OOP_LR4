// Фильтрация по полям
// Сортировка по полям
// Комбинирование сортировок
// Паттерн: Стратегия + Итератор

/// <summary>
/// Интерфейс стратегии фильтрации
/// </summary>
public interface IFilterStrategy
{
    bool IsMatch(DeliveryOption option);
    string Description { get; }
}

/// <summary>
/// Фильтр по типу транспорта
/// </summary>
public class TransportTypeFilter : IFilterStrategy
{
    private readonly string _transportType;

    public TransportTypeFilter(string transportType)
    {
        _transportType = transportType;
    }

    public bool IsMatch(DeliveryOption option) => option.TransportType == _transportType;
    public string Description => $"Тип транспорта = {_transportType}";
}

/// <summary>
/// Фильтр по максимальной стоимости
/// </summary>
public class MaxCostFilter : IFilterStrategy
{
    private readonly double _maxCost;

    public MaxCostFilter(double maxCost)
    {
        _maxCost = maxCost;
    }

    public bool IsMatch(DeliveryOption option) => option.TotalCost <= _maxCost;
    public string Description => $"Стоимость ≤ {_maxCost}";
}

/// <summary>
/// Фильтр по максимальному времени доставки
/// </summary>
public class MaxTimeFilter : IFilterStrategy
{
    private readonly double _maxHours;

    public MaxTimeFilter(double maxHours)
    {
        _maxHours = maxHours;
    }

    public bool IsMatch(DeliveryOption option) => option.DeliveryTimeHours <= _maxHours;
    public string Description => $"Время ≤ {_maxHours} ч";
}

/// <summary>
/// Интерфейс стратегии сортировки
/// </summary>
public interface ISortStrategy
{
    IOrderedEnumerable<DeliveryOption> Sort(IEnumerable<DeliveryOption> options);
    IOrderedEnumerable<DeliveryOption> ThenSort(IOrderedEnumerable<DeliveryOption> sortedOptions);
    string Description { get; }
}

/// <summary>
/// Сортировка по названию транспорта
/// </summary>
public class TransportNameSort : ISortStrategy
{
    private readonly bool _ascending;

    public TransportNameSort(bool ascending = true)
    {
        _ascending = ascending;
    }

    public IOrderedEnumerable<DeliveryOption> Sort(IEnumerable<DeliveryOption> options)
    {
        return _ascending
            ? options.OrderBy(o => o.TransportName)
            : options.OrderByDescending(o => o.TransportName);
    }

    public IOrderedEnumerable<DeliveryOption> ThenSort(IOrderedEnumerable<DeliveryOption> sortedOptions)
    {
        return _ascending
            ? sortedOptions.ThenBy(o => o.TransportName)
            : sortedOptions.ThenByDescending(o => o.TransportName);
    }

    public string Description => $"Название транспорта ({(_ascending ? "возрастание" : "убывание")})";
}

/// <summary>
/// Сортировка по стоимости
/// </summary>
public class CostSort : ISortStrategy
{
    private readonly bool _ascending;

    public CostSort(bool ascending = true)
    {
        _ascending = ascending;
    }

    public IOrderedEnumerable<DeliveryOption> Sort(IEnumerable<DeliveryOption> options)
    {
        return _ascending
            ? options.OrderBy(o => o.TotalCost)
            : options.OrderByDescending(o => o.TotalCost);
    }

    public IOrderedEnumerable<DeliveryOption> ThenSort(IOrderedEnumerable<DeliveryOption> sortedOptions)
    {
        return _ascending
            ? sortedOptions.ThenBy(o => o.TotalCost)
            : sortedOptions.ThenByDescending(o => o.TotalCost);
    }

    public string Description => $"Стоимость ({(_ascending ? "возрастание" : "убывание")})";
}

/// <summary>
/// Сортировка по скорости
/// </summary>
public class SpeedSort : ISortStrategy
{
    private readonly bool _ascending;

    public SpeedSort(bool ascending = true)
    {
        _ascending = ascending;
    }

    public IOrderedEnumerable<DeliveryOption> Sort(IEnumerable<DeliveryOption> options)
    {
        return _ascending
            ? options.OrderBy(o => o.SpeedKmh)
            : options.OrderByDescending(o => o.SpeedKmh);
    }

    public IOrderedEnumerable<DeliveryOption> ThenSort(IOrderedEnumerable<DeliveryOption> sortedOptions)
    {
        return _ascending
            ? sortedOptions.ThenBy(o => o.SpeedKmh)
            : sortedOptions.ThenByDescending(o => o.SpeedKmh);
    }

    public string Description => $"Скорость ({(_ascending ? "возрастание" : "убывание")})";
}

/// <summary>
/// для вариантов доставки
/// </summary>
public class DeliveryOption
{
    public string TransportName { get; set; }
    public string TransportType { get; set; }
    public double SpeedKmh { get; set; }
    public double CostPerKm { get; set; }
    public double TotalCost { get; set; }
    public double DeliveryTimeHours { get; set; }
    public double TotalWeight { get; set; }

    public void Display()
    {
        Console.WriteLine($"  - {TransportName} ({TransportType}):");
        Console.WriteLine($"    Стоимость: {TotalCost:F2} ед., Время: {DeliveryTimeHours:F2} ч, Скорость: {SpeedKmh:F2} км/ч");
    }
}

/// <summary>
/// Итератор для обхода результатов
/// </summary>
public class DeliveryOptionIterator : IEnumerator<DeliveryOption>
{
    private readonly List<DeliveryOption> _options;
    private int _position = -1;

    public DeliveryOptionIterator(List<DeliveryOption> options)
    {
        _options = options;
    }

    public DeliveryOption Current => _options[_position];
    object System.Collections.IEnumerator.Current => Current;

    public bool MoveNext()
    {
        _position++;
        return _position < _options.Count;
    }

    public void Reset() => _position = -1;
    public void Dispose() { }
}