using Microsoft.EntityFrameworkCore;
using SkillTrackingApp.Data;
using SkillTrackingApp.DTOs;
using SkillTrackingApp.Models;

namespace SkillTrackingApp.Services;

public interface ISkillService
{
    Task<List<SkillDto>> GetAllSkillsAsync(int userId);
    Task<SkillDto?> GetSkillByIdAsync(int skillId, int userId);
    Task<SkillDto?> CreateSkillAsync(CreateSkillDto createSkillDto, int userId);
    Task<SkillDto?> UpdateSkillAsync(int skillId, UpdateSkillDto updateSkillDto, int userId);
    Task<bool> DeleteSkillAsync(int skillId, int userId);
    Task<SkillSummaryDto> GetSkillSummaryAsync(int userId);
    Task<List<SkillDto>> GetSkillsByCategoryAsync(string category, int userId);
    Task<List<SkillDto>> GetSkillsByLevelAsync(SkillLevel level, int userId);
}

public class SkillService : ISkillService
{
    private readonly ApplicationDbContext _context;

    public SkillService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SkillDto>> GetAllSkillsAsync(int userId)
    {
        var skills = await _context.Skills
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastUpdated)
            .ToListAsync();

        return skills.Select(MapToSkillDto).ToList();
    }

    public async Task<SkillDto?> GetSkillByIdAsync(int skillId, int userId)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == skillId && s.UserId == userId);

        return skill != null ? MapToSkillDto(skill) : null;
    }

    public async Task<SkillDto?> CreateSkillAsync(CreateSkillDto createSkillDto, int userId)
    {
        var skill = new Skill
        {
            UserId = userId,
            Name = createSkillDto.Name,
            Category = createSkillDto.Category,
            Description = createSkillDto.Description,
            Level = createSkillDto.Level,
            CreatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        return MapToSkillDto(skill);
    }

    public async Task<SkillDto?> UpdateSkillAsync(int skillId, UpdateSkillDto updateSkillDto, int userId)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == skillId && s.UserId == userId);

        if (skill == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(updateSkillDto.Name))
            skill.Name = updateSkillDto.Name;

        if (!string.IsNullOrWhiteSpace(updateSkillDto.Category))
            skill.Category = updateSkillDto.Category;

        if (updateSkillDto.Description != null)
            skill.Description = updateSkillDto.Description;

        if (updateSkillDto.Level.HasValue)
            skill.Level = updateSkillDto.Level.Value;

        skill.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToSkillDto(skill);
    }

    public async Task<bool> DeleteSkillAsync(int skillId, int userId)
    {
        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == skillId && s.UserId == userId);

        if (skill == null)
        {
            return false;
        }

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<SkillSummaryDto> GetSkillSummaryAsync(int userId)
    {
        var skills = await _context.Skills
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var summary = new SkillSummaryDto
        {
            TotalSkills = skills.Count,
            ByCategory = skills
                .GroupBy(s => s.Category)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByLevel = skills
                .GroupBy(s => s.Level)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentlyUpdated = skills
                .OrderByDescending(s => s.LastUpdated)
                .Take(5)
                .Select(MapToSkillDto)
                .ToList()
        };

        return summary;
    }

    public async Task<List<SkillDto>> GetSkillsByCategoryAsync(string category, int userId)
    {
        var skills = await _context.Skills
            .Where(s => s.UserId == userId && s.Category == category)
            .OrderByDescending(s => s.LastUpdated)
            .ToListAsync();

        return skills.Select(MapToSkillDto).ToList();
    }

    public async Task<List<SkillDto>> GetSkillsByLevelAsync(SkillLevel level, int userId)
    {
        var skills = await _context.Skills
            .Where(s => s.UserId == userId && s.Level == level)
            .OrderByDescending(s => s.LastUpdated)
            .ToListAsync();

        return skills.Select(MapToSkillDto).ToList();
    }

    private static SkillDto MapToSkillDto(Skill skill)
    {
        return new SkillDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Category = skill.Category,
            Description = skill.Description,
            Level = skill.Level,
            LevelName = skill.Level.ToString(),
            CreatedAt = skill.CreatedAt,
            LastUpdated = skill.LastUpdated
        };
    }
}
