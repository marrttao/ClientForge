using Xunit;
using Microsoft.EntityFrameworkCore;
using ClientForge.Data;
using ClientForge.Features.Project.Models;
using ClientForge.Features.User.Models;

namespace ClientForge.Tests;

public class Tests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new AppDbContext(options);
    }

    [Fact]
    public void CreateProject_Should_AddProjectToDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var clientId = Guid.NewGuid();
        var client = new User
        {
            Id = clientId,
            Login = "testclient",
            Name = "Test",
            Surname = "Client",
            Email = "client@test.com",
            Role = Role.user,
            HashedPassword = "hashed_password"
        };
        
        context.Users.Add(client);
        context.SaveChanges();
        
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.InProgress,
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            ClientId = clientId
        };

        // Act
        context.Projects.Add(project);
        var result = context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.Single(context.Projects);
        
        var savedProject = context.Projects.First();
        Assert.Equal(project.Id, savedProject.Id);
        Assert.Equal("Test Project", savedProject.Name);
        Assert.Equal("Test Description", savedProject.Description);
        Assert.Equal(ProjectStatus.InProgress, savedProject.Status);
        Assert.Equal(clientId, savedProject.ClientId);
    }

    [Fact]
    public void ReadProject_Should_RetrieveProjectFromDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        
        var client = new User
        {
            Id = clientId,
            Login = "testclient2",
            Name = "Test",
            Surname = "Client",
            Email = "client2@test.com",
            Role = Role.user,
            HashedPassword = "hashed_password"
        };
        
        context.Users.Add(client);
        
        var project = new Project
        {
            Id = projectId,
            Name = "Read Test Project",
            Description = "Read Test Description",
            Status = ProjectStatus.NotStarted,
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            ClientId = clientId
        };
        
        context.Projects.Add(project);
        context.SaveChanges();

        // Act
        var retrievedProject = context.Projects
            .FirstOrDefault(p => p.Id == projectId);

        // Assert
        Assert.NotNull(retrievedProject);
        Assert.Equal(projectId, retrievedProject.Id);
        Assert.Equal("Read Test Project", retrievedProject.Name);
        Assert.Equal("Read Test Description", retrievedProject.Description);
        Assert.Equal(ProjectStatus.NotStarted, retrievedProject.Status);
    }

    [Fact]
    public void UpdateProject_Should_ModifyProjectInDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        
        var client = new User
        {
            Id = clientId,
            Login = "testclient3",
            Name = "Test",
            Surname = "Client",
            Email = "client3@test.com",
            Role = Role.user,
            HashedPassword = "hashed_password"
        };
        
        context.Users.Add(client);
        
        var project = new Project
        {
            Id = projectId,
            Name = "Original Project Name",
            Description = "Original Description",
            Status = ProjectStatus.NotStarted,
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            ClientId = clientId
        };
        
        context.Projects.Add(project);
        context.SaveChanges();

        // Act
        var projectToUpdate = context.Projects.First(p => p.Id == projectId);
        projectToUpdate.Name = "Updated Project Name";
        projectToUpdate.Description = "Updated Description";
        projectToUpdate.Status = ProjectStatus.InProgress;
        context.SaveChanges();

        // Assert
        var updatedProject = context.Projects.First(p => p.Id == projectId);
        Assert.Equal("Updated Project Name", updatedProject.Name);
        Assert.Equal("Updated Description", updatedProject.Description);
        Assert.Equal(ProjectStatus.InProgress, updatedProject.Status);
    }

    [Fact]
    public void DeleteProject_Should_RemoveProjectFromDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        
        var client = new User
        {
            Id = clientId,
            Login = "testclient4",
            Name = "Test",
            Surname = "Client",
            Email = "client4@test.com",
            Role = Role.user,
            HashedPassword = "hashed_password"
        };
        
        context.Users.Add(client);
        
        var project = new Project
        {
            Id = projectId,
            Name = "Project To Delete",
            Description = "This project will be deleted",
            Status = ProjectStatus.OnHold,
            CreatedAt = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            ClientId = clientId
        };
        
        context.Projects.Add(project);
        context.SaveChanges();
        
        Assert.Single(context.Projects);

        // Act
        var projectToDelete = context.Projects.First(p => p.Id == projectId);
        context.Projects.Remove(projectToDelete);
        var result = context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.Empty(context.Projects);
        Assert.Null(context.Projects.FirstOrDefault(p => p.Id == projectId));
    }

    [Fact]
    public void ProjectWithAllProperties_Should_BeStoredCorrectly()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var dueDate = DateTime.UtcNow.AddDays(60);
        var finishedAt = DateTime.UtcNow.AddDays(50);
        
        var client = new User
        {
            Id = clientId,
            Login = "testclient5",
            Name = "Test",
            Surname = "Client",
            Email = "client5@test.com",
            Role = Role.user,
            HashedPassword = "hashed_password"
        };
        
        context.Users.Add(client);
        
        var project = new Project
        {
            Id = projectId,
            Name = "Complete Project",
            Description = "Project with all properties",
            Status = ProjectStatus.Completed,
            CreatedAt = createdAt,
            DueDate = dueDate,
            FinishedAt = finishedAt,
            ClientId = clientId
        };

        // Act
        context.Projects.Add(project);
        context.SaveChanges();

        // Assert
        var savedProject = context.Projects.First(p => p.Id == projectId);
        Assert.Equal("Complete Project", savedProject.Name);
        Assert.Equal("Project with all properties", savedProject.Description);
        Assert.Equal(ProjectStatus.Completed, savedProject.Status);
        Assert.Equal(createdAt, savedProject.CreatedAt);
        Assert.Equal(dueDate, savedProject.DueDate);
        Assert.Equal(finishedAt, savedProject.FinishedAt);
        Assert.Equal(clientId, savedProject.ClientId);
    }
}