using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PDChecker.Models;

namespace PDChecker.Controllers
{
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly PdDbContext _context;
        private readonly IDataProtector _protector;

        public ProjectController(PdDbContext context, IDataProtectionProvider provider)
        {
            _context = context;
            _protector = provider.CreateProtector("PDChecker.IdHiding");
        }
        
        [Authorize]
        [HttpGet("/projects/{publicId}")]
        public ActionResult<Project> GetProject(string publicId)
        {
            var id = int.Parse(_protector.Unprotect(publicId));
            var project = _context.Projects.Include(p => p.Grades).FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                publicId = _protector.Protect(project.Id.ToString()),
                name = project.Name,
                description = project.Description,
                grades = project.Grades,
                buildUrl = project.BuildUrl,
            });
        }
        
        [Authorize]
        [HttpGet("/projects/all")]
        public async Task<ActionResult<ICollection>> GetAllProjects()
        {
            var projects = await _context.Projects.Include(p => p.Teamlead).ToListAsync();
            return Ok(projects.Select(p => new
            {
                publicId = _protector.Protect(p.Id.ToString()),
                name = p.Name,
                description = p.Description,
                buildUrl = p.BuildUrl,
                teamlead = p.Teamlead
            }
                )
            );
        }
        
        [Authorize(Roles="student")]
        [HttpPost("/projects/add")]
        public async Task<ActionResult<Project>> CreateProject(NewProject newProject)
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
                return BadRequest("Cannot retreive information from JWT token");
            }
            var project = new Project(newProject.Name, newProject.Description, user.Id);

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Ok();
        }
        
        [Authorize(Roles="student")]
        [HttpGet("/projects/get")]
        public async Task<ActionResult<Project>> GetProject()
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
                return BadRequest("Cannot retreive information from JWT token");
            }

            var project = _context.Projects.Include(p => p.Grades).FirstOrDefault(p => p.TeamleadId == user.Id);
            if (project == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                publicId = _protector.Protect(project.Id.ToString()),
                name = project.Name,
                description = project.Description,
                grades = project.Grades,
                buildUrl = project.BuildUrl,
            });
        }
        
        [Authorize(Roles="student")]
        [HttpPost("/projects/buildurl/{projectPublicId}")]
        public async Task<ActionResult<Project>> AttachBuildUrl(string projectPublicId, string url)
        {
            var projectId = int.Parse(_protector.Unprotect(projectPublicId));
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFound("Project with specified id not found");
            }

            if (!Regex.IsMatch(url,
                    @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)"))
            {
                return BadRequest("Provided URL is not valid");
            }

            project.BuildUrl = url;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
