using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PDChecker.Models;

public class Grade
{
    public int Id { get; set; }
    public int JudgeId { get; set; }
    [JsonIgnore]
    public User Judge { get; set; }
    public int ProjectId { get; set; }
    [JsonIgnore]
    public Project Project { get; set; }
    public double Points { get; set; }
    public string Feedback { get; set; }
    
}