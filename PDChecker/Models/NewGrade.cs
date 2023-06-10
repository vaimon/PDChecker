namespace PDChecker.Models;

public class NewGrade
{
    public int ProjectId { get; set; }
    public double Points { get; set; }
    public string Feedback { get; set; }

    public NewGrade(int projectId, double points, string feedback)
    {
        ProjectId = projectId;
        Points = points;
        Feedback = feedback;
    }
}