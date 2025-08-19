namespace Taskly.Api.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsComplete { get; set; }
    public int WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }
}
