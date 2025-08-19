namespace Taskly.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string PasswordSalt { get; set; } = "";
    public int WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }
}
