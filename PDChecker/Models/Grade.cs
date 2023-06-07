namespace PDChecker.Models;

public class Grade
{
    public int Id { get; set; }
    public int JudgeId { get; set; }
    public User Judge { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; }
    public double Points { get; set; }
    public string Feedback { get; set; }
    
}