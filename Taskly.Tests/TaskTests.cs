using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Taskly.Api.Data;
using Taskly.Api.Models;
using Xunit;

public class TaskTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        // Ensure a workspace exists
        if (!db.Workspaces.AnyAsync().Result)
        {
            db.Workspaces.Add(new Workspace { Id = 1, Name = "Test WS" });
            db.SaveChanges();
        }

        return db;
    }

    [Fact]
    public async Task CreateAndQueryTask_Works()
    {
        using var db = CreateInMemoryDb();

        db.TaskItems.Add(new TaskItem { Title = "Test task", WorkspaceId = 1 });
        await db.SaveChangesAsync();

        var tasks = await db.TaskItems.Where(t => t.WorkspaceId == 1).ToListAsync();

        Assert.Single(tasks);
        Assert.Equal("Test task", tasks[0].Title);
        Assert.False(tasks[0].IsComplete);
    }
}
