# 🚀 Начало работы с Unit Tests - ClientForge

## 📋 Краткая инструкция (5 минут)

### 1️⃣ Запустить все тесты
```bash
cd /home/romantik/RiderProjects/ClientForge
dotnet test ClientForge.Tests/ClientForge.Tests.csproj
```

### 2️⃣ Запустить с подробным выводом
```bash
dotnet test ClientForge.Tests/ClientForge.Tests.csproj -v normal
```

### 3️⃣ Запустить конкретный тест класс
```bash
# Тесты Project
dotnet test ClientForge.Tests/ClientForge.Tests.csproj --filter "ClassName=ProjectCrudTests"

# Тесты User
dotnet test ClientForge.Tests/ClientForge.Tests.csproj --filter "ClassName=UserCrudTests"

# Тесты TaskModel
dotnet test ClientForge.Tests/ClientForge.Tests.csproj --filter "ClassName=TaskModelCrudTests"
```

---

## 📁 Что было создано

### Основные файлы тестов
```
ClientForge.Tests/
├── DatabaseTestFixture.cs      ← Базовый класс с настройкой БД
├── ProjectCrudTests.cs         ← 15 тестов для Project
├── UserCrudTests.cs            ← 17 тестов для User
├── TaskModelCrudTests.cs       ← 20 тестов для TaskModel
├── GlobalUsings.cs             ← Глобальные using'и
└── ClientForge.Tests.csproj    ← Конфигурация проекта
```

### Документация
```
┌─ UNIT_TESTS_SUMMARY.md    (Общее резюме - начните отсюда!)
├─ TESTING_QUICKSTART.md    (Шпаргалка с примерами)
├─ TESTS_CATALOG.md         (Каталог всех 52 тестов)
├─ ARCHITECTURE.md          (Архитектура и структура)
└─ ClientForge.Tests/README.md (Подробная документация)
```

---

## 🎯 Что покрыто тестами

| Модель | CREATE | READ | UPDATE | DELETE | Всего |
|--------|--------|------|--------|--------|-------|
| **Project** | 3 | 5 | 4 | 4 | **15** |
| **User** | 3 | 5 | 4 | 4 | **17** |
| **TaskModel** | 4 | 6 | 4 | 3 | **20** |
| **ИТОГО** | **10** | **22** | **12** | **8** | **52** |

---

## 📚 Документы для изучения

### 1. Если вы спешите 🏃‍♂️
→ Прочитайте `UNIT_TESTS_SUMMARY.md` (2 минуты)

### 2. Если вы хотите быстро начать писать тесты ⚡
→ Используйте `TESTING_QUICKSTART.md` (примеры + команды)

### 3. Если вы хотите понять архитектуру 🏗️
→ Изучите `ARCHITECTURE.md` (диаграммы + пояснения)

### 4. Если вы ищете конкретный тест 🔍
→ Смотрите `TESTS_CATALOG.md` (каталог всех 52 тестов)

### 5. Если нужна подробная инфо 📖
→ Откройте `ClientForge.Tests/README.md` (полная документация)

---

## 🔧 Типовые задачи

### Запустить конкретный тест
```bash
dotnet test --filter "Name=CreateProject_WithValidData_ShouldSucceed"
```

### Запустить только CREATE тесты
```bash
dotnet test --filter "Name~Create"
```

### Запустить только тесты с проверкой ошибок
```bash
dotnet test --filter "Name~ShouldThrow"
```

### Запустить с результатом в формате XML
```bash
dotnet test ClientForge.Tests/ClientForge.Tests.csproj \
  --logger "trx;LogFileName=test-results.trx"
```

---

## 🧪 Структура типового теста

```csharp
// 1. Наследуем базовый класс
public class ProjectCrudTests : DatabaseTestFixture
{
    // 2. Каждый метод - один тест
    [Fact]
    public async Task CreateProject_WithValidData_ShouldSucceed()
    {
        // 3. ARRANGE - подготовка
        var client = new User { Id = Guid.NewGuid(), ... };
        DbContext.Users.Add(client);
        await DbContext.SaveChangesAsync();
        
        // 4. ACT - действие
        var project = new Project { ClientId = client.Id, ... };
        DbContext.Projects.Add(project);
        var result = await DbContext.SaveChangesAsync();
        
        // 5. ASSERT - проверка
        Assert.True(result > 0);
        Assert.Single(DbContext.Projects);
    }
}
```

---

## ✅ Проверка установки

Выполните эту команду для проверки:

```bash
cd /home/romantik/RiderProjects/ClientForge && \
dotnet build ClientForge.Tests/ClientForge.Tests.csproj && \
echo "✅ Сборка успешна!"
```

Потом запустите тесты:

```bash
dotnet test ClientForge.Tests/ClientForge.Tests.csproj && \
echo "✅ Все тесты пройдены!"
```

---

## 📊 Ожидаемый результат

При запуске `dotnet test` вы должны увидеть:

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed: 0, Passed: 52, Skipped: 0, Time: XXXms
```

---

## 🐛 Если тесты не работают

### Проблема: "Cannot find namespace ClientForge"
**Решение**: Убедитесь, что в `ClientForge.Tests.csproj` есть:
```xml
<ProjectReference Include="..\ClientForge.csproj" />
```

### Проблема: "Database connection error"
**Решение**: Это нормально! Тесты используют in-memory БД, не требует подключения.

### Проблема: "xunit not found"
**Решение**: Выполните:
```bash
cd ClientForge.Tests && dotnet restore
```

---

## 🎓 Обучающие примеры

### Пример 1: Простой CREATE тест
```csharp
[Fact]
public async Task CreateUser_WithValidData_ShouldSucceed()
{
    // ARRANGE
    var user = new User 
    { 
        Login = "testuser",
        Name = "Test",
        Surname = "User",
        Email = "test@example.com",
        HashedPassword = "hashed"
    };
    
    // ACT
    DbContext.Users.Add(user);
    await DbContext.SaveChangesAsync();
    
    // ASSERT
    Assert.NotEqual(Guid.Empty, user.Id);
    Assert.Single(DbContext.Users);
}
```

### Пример 2: Тест с проверкой ошибки
```csharp
[Fact]
public async Task DeleteUser_WithProjects_ShouldThrow()
{
    // ARRANGE
    var user = new User { Id = Guid.NewGuid(), ... };
    var project = new Project { ClientId = user.Id, ... };
    DbContext.Users.Add(user);
    DbContext.Projects.Add(project);
    await DbContext.SaveChangesAsync();
    
    // ACT & ASSERT
    var userToDelete = DbContext.Users.First();
    DbContext.Users.Remove(userToDelete);
    await Assert.ThrowsAsync<Exception>(
        async () => await DbContext.SaveChangesAsync()
    );
}
```

### Пример 3: Тест с проверкой отношений
```csharp
[Fact]
public async Task ReadProjectWithClient_ShouldIncludeClientData()
{
    // ARRANGE
    var client = new User { Id = Guid.NewGuid(), Name = "Client", ... };
    var project = new Project { ClientId = client.Id, ... };
    DbContext.Users.Add(client);
    DbContext.Projects.Add(project);
    await DbContext.SaveChangesAsync();
    
    // ACT
    var result = DbContext.Projects
        .Select(p => new { Project = p, ClientName = p.Client.Name })
        .First();
    
    // ASSERT
    Assert.Equal("Client", result.ClientName);
}
```

---

## 🚀 Интеграция с CI/CD

### GitHub Actions пример
```yaml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '10.0'
      - run: dotnet test ClientForge.Tests/ClientForge.Tests.csproj
```

---

## 📝 Добавление новых тестов

Структура для новой модели:

```csharp
// NewModelCrudTests.cs
namespace ClientForge.Tests;

public class NewModelCrudTests : DatabaseTestFixture
{
    // CREATE
    [Fact]
    public async Task CreateNewModel_WithValidData_ShouldSucceed()
    {
        // Implement test
    }
    
    // READ
    [Fact]
    public async Task ReadNewModel_ById_ShouldReturnNewModel()
    {
        // Implement test
    }
    
    // UPDATE
    [Fact]
    public async Task UpdateNewModel_ChangeProperty_ShouldSucceed()
    {
        // Implement test
    }
    
    // DELETE
    [Fact]
    public async Task DeleteNewModel_ShouldSucceed()
    {
        // Implement test
    }
}
```

---

## 💡 Полезные советы

1. **Одна концепция на тест** - каждый тест проверяет одно
2. **Описательные имена** - методы должны быть самодокументирующимися
3. **Независимость** - тесты не должны зависеть друг от друга
4. **Скорость** - тесты должны выполняться быстро
5. **Читаемость** - код должен быть понятен другим разработчикам

---

## 📞 Справка по Assert'ам

```csharp
// Значения
Assert.Equal(expected, actual);           // Проверка равенства
Assert.NotEqual(val1, val2);              // Проверка неравенства
Assert.Null(value);                       // Проверка на null
Assert.NotNull(value);                    // Проверка не null

// Булевы значения
Assert.True(condition);                   // Проверка true
Assert.False(condition);                  // Проверка false

// Коллекции
Assert.Empty(collection);                 // Пусто
Assert.Single(collection);                // Ровно 1 элемент
Assert.Equal(count, collection.Count());  // Количество элементов
Assert.Contains(item, collection);        // Содержит элемент

// Исключения
Assert.Throws<Exception>(() => { ... });  // Синхронное исключение
await Assert.ThrowsAsync<Exception>(async () => { ... }); // Асинхронное
```

---

## 📖 Дополнительные ресурсы

- [xUnit Документация](https://xunit.net/)
- [Entity Framework Testing](https://docs.microsoft.com/en-us/ef/core/testing/)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [AAA Pattern](https://www.freecodecamp.org/news/unit-tests-explained/)

---

## ✨ Что дальше?

1. ✅ Запустите тесты: `dotnet test`
2. 📚 Прочитайте UNIT_TESTS_SUMMARY.md
3. 🧪 Напишите свой первый тест
4. 🔍 Изучите примеры в TESTING_QUICKSTART.md
5. 🚀 Интегрируйте в CI/CD

---

**Всё готово к запуску! 🎉**

Вопросы? Смотрите в документации или примерах кода в `ClientForge.Tests/`

