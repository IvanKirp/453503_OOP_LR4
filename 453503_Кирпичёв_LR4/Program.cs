class Program
{
    static void Main(string[] args)
    {
        try
        {
            var controller = new LogisticsController();

            // Инициализация с файлом данных
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

            /*            
            string csvFilePath = "data.csv";
            if (File.Exists(csvFilePath))
            {
                Console.WriteLine("Загрузка из CSV файла");
                controller.Initialize(csvFilePath);
                Console.WriteLine($"Загружено из CSV: {controller.GetAvailableCargos().Count} грузов, {controller.GetAvailableTransports().Count} транспортов");
            }
            */

            /*
            string jsonFilePath = "data.json";
            if (File.Exists(jsonFilePath))
            {
                Console.WriteLine("\nЗагрузка из JSON файла");
                var facade = new DataLoaderFacade();
                controller.InitializeWithFacade(facade, jsonFilePath);
                Console.WriteLine($"Загружено из JSON: {controller.GetAvailableCargos().Count} грузов, {controller.GetAvailableTransports().Count} транспортов");
            }
            */

            /*
            string xmlFilePath = "data.xml";
            if (File.Exists(xmlFilePath))
            {
                Console.WriteLine("\nЗагрузка из XML файла");
                var facade = new DataLoaderFacade();
                controller.InitializeWithFacade(facade, xmlFilePath);
                Console.WriteLine($"Загружено из XML: {controller.GetAvailableCargos().Count} грузов, {controller.GetAvailableTransports().Count} транспортов");
            }
            */

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

            var cargoQuantities = new Dictionary<string, int>
            {
                { "Электроника", 10 },
                { "Одежда", 5 }
            };
            double distance = 500;



            // Все варианты доставки (тип транспорта не указан)
            Console.WriteLine("\n\nВсе возможные варианты доставки:");
            var allOptions = controller.GetAllPossibleDeliveryOptions(cargoQuantities, distance);
            foreach (var opt in allOptions)
            {
                opt.Display();
            }

            // Фильтрация и сортировка
            Console.WriteLine("\n\nОтфильтрованные и отсортированные варианты:");
            var filters = new List<IFilterStrategy>
            {
                new MaxCostFilter(10000),
                new MaxTimeFilter(10)
            };

            var sorts = new List<ISortStrategy>
            {
                new CostSort(true),
                new SpeedSort(false)
            };

            var filteredOptions = controller.GetDeliveryOptionsWithFilterAndSort(
                cargoQuantities, distance, filters, sorts);

            foreach (var opt in filteredOptions)
            {
                opt.Display();
            }

            // Экспорт результатов
            Console.WriteLine("\n\nЭкспорт результатов:");

            // Обычная доставка с указанием типа транспорта
            var result = controller.ProcessDelivery(cargoQuantities, "Земля", distance);
            result.Display();

            // Экспорт в JSON с шифрованием
            controller.ExportDeliveryResultToFile(result, "result.json", "json", encrypt: true, password: "12345");

            // Экспорт в CSV со сжатием
            controller.ExportDeliveryResultToFile(result, "result.csv", "csv", compress: true);

            // Экспорт в JSON с шифрованием и сжатием
            controller.ExportDeliveryResultToFile(result, "result_secure.json", "json",
                encrypt: true, compress: true, password: "secret");

            // Экспорт списка вариантов в CSV
            controller.ExportDeliveryOptionsToFile(allOptions, "options.csv", "csv");

            // Экспорт списка вариантов в JSON
            controller.ExportDeliveryOptionsToFile(allOptions, "options.json", "json");

            Console.WriteLine("\n\nВсе файлы успешно экспортированы:");
            Console.WriteLine("  - result.json (зашифрован)");
            Console.WriteLine("  - result.csv (сжат в ZIP)");
            Console.WriteLine("  - result_secure.json (зашифрован и сжат)");
            Console.WriteLine("  - options.csv");
            Console.WriteLine("  - options.json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nОшибка: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
        }

        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}