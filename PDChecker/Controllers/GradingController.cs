using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDChecker.Models;

namespace PDChecker.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GradingController : ControllerBase
{
    private readonly PdDbContext _context;

    public GradingController(PdDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles="judge")]
    [HttpPost("/grade")]
    public async Task<ActionResult<Project>> GradeProject(NewGrade newGrade)
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        User? user;
        if (identity != null)
        {
            user = _context.Users.FirstOrDefault(u => u.Login == identity.Name);
            if (user == null)
            {
                return NotFound("User with this login not found");
            }
        }
        else
        {
            return BadRequest("Cannot retrieve information from JWT token");
        }
        // var grade = new Grade(user.Id, newGrade.ProjectId, newGrade.Points,newGrade.Feedback);
        
        var existingUser = _context.Users                        
            .Include(p => p.Grades)                   
            .Single(p => p.Id == user.Id); 
 
        var existingProject = _context.Projects          
            .Single(p => p.Id == newGrade.ProjectId);
 
        existingUser.Grades.Add(new Grade  
        {
            Judge = existingUser,  
            Project = existingProject,  
            Points = newGrade.Points,
            Feedback = newGrade.Feedback
        });
        // user.Grades.Add(grade);
        await _context.SaveChangesAsync();

        return Ok();
    }
    
    [Authorize]
    [HttpGet("/project_grades/{id}")]
    public async Task<ActionResult<ICollection<Grade>>> GetProjectGrades(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        var result = _context.Projects
            .Include(p => p.Grades).ThenInclude(g => g.Judge)
            .FirstOrDefault(p => p.Id == id)?.Grades
            .ToList();
        return Ok(result);
    }
    
    
    
}