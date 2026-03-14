# Быстрый старт - Unit Tests

## Быстрые команды

### Запустить все тесты
```bash
cd /home/romantik/RiderProjects/ClientForge
dotnet test ClientForge.Tests/ClientForge.Tests.csproj
```

### Запустить и посмотреть детали
```bash
dotnet test ClientForge.Tests/ClientForge.Tests.csproj -v normal
```

### Запустить конкретный тест класс
```bash
# Project тесты
dotnet test ClientForge.Tests/ClientForge.Tests.csproj --filter "ClassName=ProjectCrudTests"

# User тесты  
dotnet test ClientForge.Tests/ClientForge.Tests.csproj --filter "ClassName=UserCrudTests"

# TaskModel тесты
dotnet test ClientForge.Tests/ClientForge.Tests.csproj --filter "ClassName=TaskModelCrudTests"
```

### Запустить конкретный тест
```bash
dotnet test ClientForge.Tests/ClientForge.Tests.csproj --filter "Name=CreateProject_WithValidData_ShouldSucceed"
```

### Запустить с выводом в консоль
```bash
dotnet test ClientForge.Tests/ClientForge.Tests.csproj --logger "console;verbosity=detailed"
```

## Структура тестов (AAA Pattern)

Каждый тест следует паттерну Arrange-Act-Assert:

```csharp
[Fact]
public async Task CreateProject_WithValidData_ShouldSucceed()
{
    // ARRANGE - подготовка данных
    var client = new User { Id = Guid.NewGuid(), ... };
    var project = new Project { Name = "Test", ClientId = client.Id, ... };
    DbContext.Users.Add(client);
    
    // ACT - выполнение действия
    DbContext.Projects.Add(project);
    var result = await DbContext.SaveChangesAsync();
    
    // ASSERT - проверка результата
    Assert.True(result > 0);
    Assert.NotEqual(Guid.Empty, project.Id);
}
```

## Основные операции в тестах

### Создание (CREATE)
```csharp
var user = new User { Login = "testuser", ... };
DbContext.Users.Add(user);
await DbContext.SaveChangesAsync();
```

### Чтение (READ)
```csharp
// По ID
var user = DbContext.Users.FirstOrDefault(u => u.Id == userId);

// Все записи
var users = DbContext.Users.ToList();

// С связанными данными
var project = DbContext.Projects
    .Where(p => p.Id == projectId)
    .Select(p => new { Project = p, ClientName = p.Client.Name })
    .FirstOrDefault();
```

### Обновление (UPDATE)
```csharp
var user = DbContext.Users.First(u => u.Id == userId);
user.Email = "newemail@example.com";
user.Role = Role.admin;
await DbContext.SaveChangesAsync();
```

### Удаление (DELETE)
```csharp
var user = DbContext.Users.First(u => u.Id == userId);
DbContext.Users.Remove(user);
await DbContext.SaveChangesAsync();
```

## Проверка исключений

```csharp
// Проверка, что выбросится исключение
await Assert.ThrowsAsync<Exception>(async () => 
{
    var project = new Project { Name = "Test", ClientId = Guid.NewGuid() };
    DbContext.Projects.Add(project);
    await DbContext.SaveChangesAsync();
});
```

## Использование DatabaseTestFixture

```csharp
public class MyTests : DatabaseTestFixture
{
    public MyTests() : base()
    {
        // DbContext уже инициализирован в базовом классе
    }
    
    [Fact]
    public async Task MyTest()
    {
        // Использовать DbContext для операций с БД
        DbContext.Users.Add(new User { ... });
        await DbContext.SaveChangesAsync();
    }
}
// После теста БД автоматически очищается
```

## Примеры тестов

### Пример 1: Простое создание и чтение
```csharp
[Fact]
public async Task ReadUser_ById_ShouldReturnUser()
{
    // Arrange
    var userId = Guid.NewGuid();
    var user = new User { Id = userId, Login = "testuser", ... };
    DbContext.Users.Add(user);
    await DbContext.SaveChangesAsync();
    
    // Act
    var result = DbContext.Users.FirstOrDefault(u => u.Id == userId);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("testuser", result.Login);
}
```

### Пример 2: Тестирование связей
```csharp
[Fact]
public async Task DeleteProject_WithTasks_ShouldDeleteTasks()
{
    // Arrange - создать проект с задачами
    var client = new User { Id = Guid.NewGuid(), ... };
    var project = new Project { ClientId = client.Id, ... };
    DbContext.Users.Add(client);
    DbContext.Projects.Add(project);
    await DbContext.SaveChangesAsync();
    
    var task = new TaskModel { ProjectId = project.Id, ... };
    DbContext.Tasks.Add(task);
    await DbContext.SaveChangesAsync();
    
    // Act - удалить проект (задачи должны удалиться каскадно)
    var projectToDelete = DbContext.Projects.First();
    DbContext.Projects.Remove(projectToDelete);
    await DbContext.SaveChangesAsync();
    
    // Assert - проверить, что задачи тоже удалены
    Assert.Empty(DbContext.Projects);
    Assert.Empty(DbContext.Tasks);
}
```

### Пример 3: Тестирование ошибок
```csharp
[Fact]
public async Task CreateProject_WithoutClient_ShouldThrow()
{
    // Arrange
    var project = new Project 
    { 
        Name = "Orphan Project",
        ClientId = Guid.NewGuid() // Несуществующий клиент
    };
    DbContext.Projects.Add(project);
    
    // Act & Assert
    await Assert.ThrowsAsync<Exception>(
        async () => await DbContext.SaveChangesAsync()
    );
}
```

## Полезные Assert'ы

```csharp
// Проверка значений
Assert.Equal(expected, actual);
Assert.NotEqual(value1, value2);
Assert.True(condition);
Assert.False(condition);
Assert.Null(value);
Assert.NotNull(value);

// Проверка коллекций
Assert.Single(collection);      // Ровно 1 элемент
Assert.Empty(collection);        // Пусто
Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);
Assert.Equal(count, collection.Count());

// Проверка исключений
await Assert.ThrowsAsync<Exception>(async () => { ... });
Assert.Throws<ArgumentNullException>(() => { ... });
```

## Отладка тестов

### Вывод информации
```csharp
// Используйте Output для вывода в тесте
[Fact]
public void MyTest(ITestOutputHelper output)
{
    output.WriteLine("Debug message: " + value);
}
```

### Запуск одного теста в отладчике
```bash
# Запустить с отладкой (если используется IDE)
dotnet test --filter "Name=YourTestName" --logger "console;verbosity=detailed"
```

## Проверка покрытия тестами

```bash
# Установить инструмент для измерения покрытия
dotnet tool install -g coverlet.console

# Получить отчёт о покрытии
dotnet test ClientForge.Tests/ClientForge.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Интеграция с Git Pre-commit

Добавьте в `.git/hooks/pre-commit`:
```bash
#!/bin/bash
dotnet test ClientForge.Tests/ClientForge.Tests.csproj
if [ $? -ne 0 ]; then
    echo "Tests failed, aborting commit"
    exit 1
fi
```

## Мониторинг тестов

Для CI/CD платформ:
```bash
# GitHub Actions примерно так:
- name: Run Tests
  run: dotnet test ClientForge.Tests/ClientForge.Tests.csproj --logger "trx;LogFileName=test-results.trx"
  
- name: Upload Test Results
  uses: actions/upload-artifact@v2
  with:
    name: test-results
    path: test-results.trx
```

## Полезные ссылки

- [xUnit документация](https://xunit.net/)
- [Entity Framework Core тестирование](https://docs.microsoft.com/en-us/ef/core/testing/)
- [Best Practices для Unit Tests](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

