namespace PDChecker.Models;

public class NewGrade
{
    public string PublicProjectId { get; set; }
    public double Points { get; set; }
    public string Feedback { get; set; }

    public NewGrade(string publicProjectId, double points, string feedback)
    {
        PublicProjectId = publicProjectId;
        Points = points;
        Feedback = feedback;
    }
}