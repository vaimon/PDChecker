using System.Collections;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDChecker.Models;

namespace PDChecker.Controllers;

[ApiController]
public class GradingController : ControllerBase
{
    private readonly PdDbContext _context;
    private readonly IDataProtector _protector;

    public GradingController(PdDbContext context, IDataProtectionProvider provider)
    {
        _context = context;
        _protector = provider.CreateProtector("PDChecker.IdHiding");
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
            .Single(p => p.Id == int.Parse(_protector.Unprotect(newGrade.PublicProjectId)));
 
        existingUser.Grades.Add(new Grade  
        {
            Judge = existingUser,  
            Project = existingProject,  
            Points = newGrade.Points,
            Feedback = newGrade.Feedback,
        });
        await _context.SaveChangesAsync();

        return Ok();
    }
    
    [Authorize]
    [HttpGet("/project_grades/{publicId}")]
    public async Task<ActionResult<ICollection>> GetProjectGrades(string publicId)
    {
        var id = int.Parse(_protector.Unprotect(publicId));
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        var result = _context.Projects
            .Include(p => p.Grades).ThenInclude(g => g.Judge)
            .FirstOrDefault(p => p.Id == id)?.Grades
            .Select(g => new
            {
                points = g.Points,
                feedback = g.Feedback,
                judgePublicId = _protector.Protect(g.JudgeId.ToString())
            })
            .ToList();
        return Ok(result);
    }
    
    
    
}