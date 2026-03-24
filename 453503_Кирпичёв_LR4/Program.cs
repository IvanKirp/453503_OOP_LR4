using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var controller = new LogisticsController();

            string dataFilePath = "data.txt";

            // Проверка существования файла
            if (!File.Exists(dataFilePath))
            {
                Console.WriteLine($"Файл данных '{dataFilePath}' не найден!");
                Console.WriteLine($"Текущая директория: {Environment.CurrentDirectory}");
                Console.WriteLine("\nУбедитесь, что файл data.txt находится в правильном месте.");
                return;
            }

            Console.WriteLine($"Загрузка данных из: {Path.GetFullPath(dataFilePath)}\n");

            if (!controller.Initialize(dataFilePath))
            {
                Console.WriteLine("Не удалось инициализировать систему. Проверьте формат файла данных.");
                return;
            }

            Console.WriteLine("\nДоступные грузы:");
            foreach (var cargo in controller.GetAvailableCargos())
            {
                Console.WriteLine($"  - {cargo.Name}: {cargo.WeightPerUnit:F2} кг/ед., " +
                                    $"{cargo.CostPerKg:F2} ед./кг");
            }

            Console.WriteLine("\nДоступные типы транспорта:");
            var transportGroups = controller.GetAvailableTransports()
                .GroupBy(t => t.Type)
                .Select(g => new { Type = g.Key, Transports = g.ToList() });

            foreach (var group in transportGroups)
            {
                Console.WriteLine($"  {group.Type}:");
                foreach (var transport in group.Transports)
                {
                    Console.WriteLine($"    - {transport.Name}: {transport.CostPerKm:F2} ед./км, {transport.SpeedKmh:F2} км/ч");
                }
            }

            // Пример 1: Наземный транспорт
            Console.WriteLine("\n\nПример 1: Наземный транспорт");
            var cargoQuantities = new Dictionary<string, int>
            {
                { "Электроника", 10 },
                { "Одежда", 5 }
            };

            string transportType = "Земля";
            double distance = 500;

            var result = controller.ProcessDelivery(cargoQuantities, transportType, distance);
            result.Display();

            // Пример 2: Воздушный транспорт
            Console.WriteLine("\n\nПример 2: Воздушный транспорт");
            var cargoQuantities2 = new Dictionary<string, int>
            {
                { "Скоропортящиеся продукты", 3 },
                { "Оборудование", 1 }
            };

            var result2 = controller.ProcessDelivery(cargoQuantities2, "Воздух", 1500);
            result2.Display();

            // Пример 3: Водный транспорт
            Console.WriteLine("\n\nПример 3: Водный транспорт");
            var cargoQuantities3 = new Dictionary<string, int>
            {
                { "Оборудование", 2 }
            };

            var result3 = controller.ProcessDelivery(cargoQuantities3, "Вода", 800);
            result3.Display();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nОшибка: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
        }
    }
}
