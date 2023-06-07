namespace PDChecker.Models;

public class Grade
{
    public int JudgeId { get; set; }
    public User Judge { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; }
}