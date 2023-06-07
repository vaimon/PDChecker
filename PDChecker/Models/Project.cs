namespace PDChecker.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public int TeamleadId { get; set; }
    public User Teamlead { get; set; }
    
    public string BuildUrl { get; set; }
    
    public List<Grade> Grades { get; set; }
}