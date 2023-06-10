using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PDChecker.Models;

namespace PDChecker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly PdDbContext _context;

        public ProjectController(PdDbContext context)
        {
            _context = context;
        }
        
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }
        
        [Authorize(Roles="student")]
        [HttpPost]
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

            return CreatedAtAction("GetProject", new {id = project.Id}, project);
        }
        
        [Authorize(Roles="student")]
        [HttpPost("/buildurl/{projectId}")]
        public async Task<ActionResult<Project>> AttachBuildUrl(int projectId, string url)
        {
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
