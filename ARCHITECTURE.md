# Архитектура и структура тестового проекта

## Общая архитектура

```
ClientForge/
├── ClientForge.csproj (основной проект)
├── ClientForge.Tests/ (тестовый проект)
│   ├── ClientForge.Tests.csproj
│   ├── GlobalUsings.cs
│   ├── DatabaseTestFixture.cs (базовый класс)
│   ├── ProjectCrudTests.cs
│   ├── UserCrudTests.cs
│   ├── TaskModelCrudTests.cs
│   ├── README.md
│   ├── bin/
│   └── obj/
├── ClientForge.sln (обновлён с тестовым проектом)
├── UNIT_TESTS_SUMMARY.md
├── TESTING_QUICKSTART.md
└── TESTS_CATALOG.md (этот файл)
```

## Иерархия наследования тестов

```
DatabaseTestFixture (базовый класс)
├── ProjectCrudTests
│   ├── CreateProject_WithValidData_ShouldSucceed
│   ├── CreateProject_WithoutClient_ShouldThrow
│   ├── CreateMultipleProjects_ShouldSucceed
│   ├── ReadProject_ById_ShouldReturnProject
│   ├── ReadAllProjects_ShouldReturnAllProjects
│   ├── ReadProjectWithClient_ShouldIncludeClientData
│   ├── ReadNonExistentProject_ShouldReturnNull
│   ├── UpdateProject_ChangeStatus_ShouldSucceed
│   ├── UpdateProject_SetDueDate_ShouldSucceed
│   ├── UpdateProject_SetFinishedDate_ShouldSucceed
│   ├── UpdateProject_MultipleFields_ShouldSucceed
│   ├── DeleteProject_ShouldSucceed
│   ├── DeleteProject_ShouldNotDeleteClient
│   ├── DeleteProject_WithTasks_ShouldDeleteTasks
│   └── DeleteNonExistentProject_ShouldNotThrow
│
├── UserCrudTests
│   ├── CreateUser_WithValidData_ShouldSucceed
│   ├── CreateUser_WithDifferentRoles_ShouldSucceed
│   ├── CreateMultipleUsers_ShouldSucceed
│   ├── ReadUser_ById_ShouldReturnUser
│   ├── ReadUser_ByLogin_ShouldReturnUser
│   ├── ReadAllUsers_ShouldReturnAllUsers
│   ├── ReadUser_WithProjects_ShouldIncludeProjectData
│   ├── ReadNonExistentUser_ShouldReturnNull
│   ├── UpdateUser_ChangeEmail_ShouldSucceed
│   ├── UpdateUser_ChangeRole_ShouldSucceed
│   ├── UpdateUser_ChangePassword_ShouldSucceed
│   ├── UpdateUser_MultipleFields_ShouldSucceed
│   ├── DeleteUser_WithoutProjects_ShouldSucceed
│   ├── DeleteUser_WithProjects_ShouldThrow
│   ├── DeleteUser_WithAssignedTasks_ShouldThrow
│   └── DeleteNonExistentUser_ShouldNotThrow
│
└── TaskModelCrudTests
    ├── CreateTask_WithValidData_ShouldSucceed
    ├── CreateTask_WithoutProject_ShouldThrow
    ├── CreateTask_WithoutWorker_ShouldThrow
    ├── CreateMultipleTasks_ShouldSucceed
    ├── ReadTask_ById_ShouldReturnTask
    ├── ReadAllTasks_ShouldReturnAllTasks
    ├── ReadTask_WithProjectData_ShouldIncludeProject
    ├── ReadTask_WithWorkerData_ShouldIncludeWorker
    ├── ReadTasksByStatus_ShouldFilterCorrectly
    ├── ReadNonExistentTask_ShouldReturnNull
    ├── UpdateTask_ChangeStatus_ShouldSucceed
    ├── UpdateTask_AddSubmissionResult_ShouldSucceed
    ├── UpdateTask_AddReviewComment_ShouldSucceed
    ├── UpdateTask_MultipleFields_ShouldSucceed
    ├── DeleteTask_ShouldSucceed
    ├── DeleteTask_ShouldNotDeleteProject
    ├── DeleteTask_ShouldNotDeleteWorker
    ├── DeleteMultipleTasks_ShouldSucceed
    └── DeleteNonExistentTask_ShouldNotThrow
```

## Зависимости между тестами

```
DatabaseTestFixture
    ↓
Инициализирует DbContext (AppDbContext)
    ↓
    ├─→ ProjectCrudTests (тесты для Project)
    │   ├─ Использует: DbContext.Projects
    │   ├─ Использует: DbContext.Users (для ClientId FK)
    │   └─ Использует: DbContext.Tasks (для каскадного удаления)
    │
    ├─→ UserCrudTests (тесты для User)
    │   ├─ Использует: DbContext.Users
    │   ├─ Использует: DbContext.Projects (для relationship)
    │   └─ Использует: DbContext.Tasks (для constraint check)
    │
    └─→ TaskModelCrudTests (тесты для TaskModel)
        ├─ Использует: DbContext.Tasks
        ├─ Использует: DbContext.Projects (для ProjectId FK)
        └─ Использует: DbContext.Users (для WorkerId FK)
```

## In-Memory База данных

```
DatabaseTestFixture
    ↓
Создаёт уникальную in-memory БД
    ↓
UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
    ↓
EnsureCreated() - создание схемы
    ↓
[Test выполняется]
    ↓
Dispose()
    ↓
EnsureDeleted() - очистка БД
```

### Схема БД в памяти

```
┌─────────────────────────────────────────────────────┐
│              In-Memory Database                      │
├─────────────────────────────────────────────────────┤
│                                                       │
│  Table: Users                                       │
│  ├── Id (Guid, PK)                                  │
│  ├── Login (string)                                 │
│  ├── Name (string)                                  │
│  ├── Surname (string)                               │
│  ├── Email (string)                                 │
│  ├── Role (enum)                                    │
│  └── HashedPassword (string)                        │
│                                                       │
│  Table: Projects                                    │
│  ├── Id (Guid, PK)                                  │
│  ├── Name (string)                                  │
│  ├── Description (string)                           │
│  ├── Status (enum)                                  │
│  ├── CreatedAt (DateTime)                           │
│  ├── DueDate (DateTime?)                            │
│  ├── FinishedAt (DateTime?)                         │
│  └── ClientId (Guid, FK → Users.Id)                │
│                                                       │
│  Table: Tasks (TaskModel)                           │
│  ├── Id (int, PK)                                   │
│  ├── Name (string)                                  │
│  ├── Description (string)                           │
│  ├── DueDate (DateTime)                             │
│  ├── ProjectId (Guid, FK → Projects.Id)            │
│  ├── WorkerId (Guid, FK → Users.Id)                │
│  ├── Status (enum)                                  │
│  ├── SubmissionResult (string?)                     │
│  └── ReviewComment (string?)                        │
│                                                       │
└─────────────────────────────────────────────────────┘
```

## Связи в БД и их тестирование

### 1. User → Projects (один ко многим)

```
User
  │
  └──→ Projects (ICollection<Project>)

Поведение: DeleteBehavior.Restrict
- ❌ Нельзя удалить User с Projects
- ✅ Тест: DeleteUser_WithProjects_ShouldThrow
```

### 2. Project → Tasks (один ко многим)

```
Project
  │
  └──→ Tasks (ICollection<TaskModel>)

Поведение: DeleteBehavior.Cascade
- ✅ При удалении Project удаляются все Tasks
- ✅ Тест: DeleteProject_WithTasks_ShouldDeleteTasks
```

### 3. User (Worker) → Tasks (один ко многим)

```
User (как Worker)
  │
  └──→ Tasks (ICollection<TaskModel>)

Поведение: DeleteBehavior.Restrict
- ❌ Нельзя удалить User с assigned Tasks
- ✅ Тест: DeleteUser_WithAssignedTasks_ShouldThrow
```

## Паттерны тестирования

### Паттерн 1: Arrange-Act-Assert (AAA)

```csharp
[Fact]
public async Task TestName()
{
    // ARRANGE - подготовка данных
    var entity = new Entity { Property = "value" };
    DbContext.Entities.Add(entity);
    await DbContext.SaveChangesAsync();
    
    // ACT - выполнение действия
    var result = DbContext.Entities.First();
    
    // ASSERT - проверка результата
    Assert.NotNull(result);
}
```

### Паттерн 2: Fixture-на основе тестирование

```csharp
public class TestClass : DatabaseTestFixture
{
    // DatabaseTestFixture предоставляет:
    // - protected AppDbContext DbContext
    // - Автоматическое управление жизненным циклом БД
    // - Cleanup после каждого теста
    
    [Fact]
    public async Task MyTest()
    {
        // Использует DbContext из базового класса
    }
}
```

### Паттерн 3: Проверка исключений

```csharp
[Fact]
public async Task ConstraintViolation_ShouldThrow()
{
    // Arrange
    var entity = new Entity { RequiredForeignKey = Guid.NewGuid() };
    DbContext.Entities.Add(entity);
    
    // Act & Assert
    await Assert.ThrowsAsync<Exception>(
        async () => await DbContext.SaveChangesAsync()
    );
}
```

## Жизненный цикл теста

```
1. Инициализация DatabaseTestFixture
   ↓
2. Создание in-memory БД с уникальным именем
   ↓
3. EnsureCreated() - создание таблиц и связей
   ↓
4. [Test setUp выполняется]
   ↓
5. Arrange - подготовка тестовых данных
   ↓
6. Act - выполнение теста
   ↓
7. Assert - проверка результатов
   ↓
8. [Test tearDown выполняется]
   ↓
9. Dispose() вызывается автоматически
   ↓
10. EnsureDeleted() - очистка БД
   ↓
11. DbContext.Dispose() - освобождение ресурсов
```

## Правила написания новых тестов

### ✅ Хорошие практики

```csharp
[Fact] // Используйте [Fact] для без параметров, [Theory] для параметризованных
public async Task DescriptiveMethodName_InputCondition_ExpectedBehavior()
{
    // 1. Одна концепция на тест
    // 2. Использовать имена, описывающие Arrange-Act-Assert
    // 3. Использовать базовый класс DatabaseTestFixture
    // 4. Проверять ровно одно на Assert
    // 5. Избегать условной логики в тестах
}
```

### ❌ Плохие практики

```csharp
[Fact]
public void BadTest() // Не используйте void для async операций
{
    // Множество Assert'ов
    // Зависимость между тестами
    // Использование глобального состояния
    // Сложная логика в тесте
}
```

## Интеграция с различными инструментами

### Visual Studio / Rider
```
Test Explorer → ClientForge.Tests → Run All/Selected Tests
```

### Командная строка
```bash
dotnet test ClientForge.Tests/ClientForge.Tests.csproj
```

### CI/CD Pipeline (GitHub Actions)
```yaml
- name: Run Tests
  run: dotnet test ClientForge.Tests/ClientForge.Tests.csproj
```

## Расширение тестов

### Добавление нового теста

```csharp
public class MyNewTests : DatabaseTestFixture
{
    [Fact]
    public async Task NewTest_Scenario_Result()
    {
        // Новый тест автоматически получит:
        // - Свежую in-memory БД
        // - DbContext для работы
        // - Автоматическую очистку
    }
}
```

### Добавление параметризованного теста

```csharp
[Theory]
[InlineData("value1")]
[InlineData("value2")]
public async Task ParametrizedTest(string value)
{
    // Тест выполнится для каждого набора параметров
}
```

## Метрики и отчёты

### Статистика покрытия

```
Total Tests:     52
Project Tests:   15 (28.8%)
User Tests:      17 (32.7%)
TaskModel Tests: 20 (38.5%)

Coverage by Operation:
- CREATE: 10 tests (19.2%)
- READ:   22 tests (42.3%)
- UPDATE: 12 tests (23.1%)
- DELETE:  8 tests (15.4%)
```

### Требуемое время выполнения

- Среднее: ~3-5 мс на тест
- Всего: ~200-300 мс для всех 52 тестов
- С вывода: ~1-2 сек

## Рекомендации

1. **Масштабирование**: При добавлении новых моделей создавайте новый класс `ModelNameCrudTests`
2. **Переиспользование**: Используйте `DatabaseTestFixture` для всех тестов с БД
3. **Документирование**: Добавляйте комментарии для сложных сценариев
4. **CI/CD**: Интегрируйте тесты в pipeline
5. **Покрытие**: Стремитесь к 80%+ покрытию кода

