using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace PDChecker.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    public int TeamleadId { get; set; }
    [JsonIgnore]
    public User Teamlead { get; set; }
    
    public string? BuildUrl { get; set; }
    
    [JsonIgnore]
    public ICollection<Grade> Grades { get; set; }

    public Project(string name, string description, int teamleadId)
    {
        Name = name;
        Description = description;
        BuildUrl = "";
        TeamleadId = teamleadId;
    }
}