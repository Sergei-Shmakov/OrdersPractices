Отчёт по unit-тестам

- Добавлены тесты по таблицам README для Order, QuantityValidator, DiscountCalculator, OrderService, OrderCreatedHandler и InMemoryOrderRepository.
- Для граничных значений применены Theory и InlineData; для асинхронных методов используются Task и await.
- Зависимости OrderService и OrderCreatedHandler изолированы через Moq; вызовы проверяются с Verify, Times.Once и Times.Never.
- Тест Calculate_ReturnsExpected обнаруживает ошибку на границе: при сумме ровно 1000 скидка должна быть 100, но исходный код проверял total > Threshold.
- Исправлено условие в DiscountCalculator: теперь скидка применяется начиная с Threshold включительно (total >= Threshold).
- Добавлен GitHub Actions workflow .github/workflows/ci.yml для восстановления пакетов, сборки и запуска тестов на .NET 8.
- В этой среде нет установленного .NET SDK, поэтому фактический запуск dotnet test выполнить не удалось; после распаковки проекта его следует запустить локально.
