using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.UnitTests;

/// <summary>
/// Builds a real <see cref="AppDbContext"/> backed by a private SQLite in-memory database so
/// service logic (EF queries + writes) can be exercised without SQL Server.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext Db { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;
        Db = new AppDbContext(options);
        Db.Database.EnsureCreated();
    }

    public EvaluationPeriod SeedActivePeriod()
    {
        var p = new EvaluationPeriod
        {
            Name = "Active",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Status = PeriodStatus.Active
        };
        Db.EvaluationPeriods.Add(p);
        Db.SaveChanges();
        return p;
    }

    public Department SeedDepartment(string name = "IT")
    {
        var d = new Department { Name = name };
        Db.Departments.Add(d);
        Db.SaveChanges();
        return d;
    }

    public Designation SeedDesignation(string name)
    {
        var d = new Designation { Name = name };
        Db.Designations.Add(d);
        Db.SaveChanges();
        return d;
    }

    public Employee SeedEmployee(string name, int? departmentId = null, int? designationId = null, int? managerId = null)
    {
        var e = new Employee
        {
            HrCode = "HR-" + Guid.NewGuid().ToString("N")[..6],
            AccountsCode = "ACC",
            Name = name,
            DepartmentId = departmentId ?? SeedDepartment().Id,
            DesignationId = designationId ?? SeedDesignation("Officer-" + Guid.NewGuid().ToString("N")[..4]).Id,
            JobTitle = "Officer",
            IsActive = true,
            ReportingManagerId = managerId
        };
        Db.Employees.Add(e);
        Db.SaveChanges();
        return e;
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
