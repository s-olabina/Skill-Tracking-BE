using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillTrackingApp.DTOs;
using SkillTrackingApp.Models;
using SkillTrackingApp.Services;
using System.Security.Claims;

namespace SkillTrackingApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillsController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSkills()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skills = await _skillService.GetAllSkillsAsync(userId.Value);
        return Ok(skills);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSkillById(int id)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skill = await _skillService.GetSkillByIdAsync(id, userId.Value);
        if (skill == null)
        {
            return NotFound();
        }

        return Ok(skill);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillDto createSkillDto)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skill = await _skillService.CreateSkillAsync(createSkillDto, userId.Value);
        return CreatedAtAction(nameof(GetSkillById), new { id = skill!.Id }, skill);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSkill(int id, [FromBody] UpdateSkillDto updateSkillDto)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skill = await _skillService.UpdateSkillAsync(id, updateSkillDto, userId.Value);
        if (skill == null)
        {
            return NotFound();
        }

        return Ok(skill);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _skillService.DeleteSkillAsync(id, userId.Value);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSkillSummary()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var summary = await _skillService.GetSkillSummaryAsync(userId.Value);
        return Ok(summary);
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetSkillsByCategory(string category)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skills = await _skillService.GetSkillsByCategoryAsync(category, userId.Value);
        return Ok(skills);
    }

    [HttpGet("level/{level}")]
    public async Task<IActionResult> GetSkillsByLevel(SkillLevel level)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skills = await _skillService.GetSkillsByLevelAsync(level, userId.Value);
        return Ok(skills);
    }

    private int? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
