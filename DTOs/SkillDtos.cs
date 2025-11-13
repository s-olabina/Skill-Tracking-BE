using System.ComponentModel.DataAnnotations;
using SkillTrackingApp.Models;

namespace SkillTrackingApp.DTOs;

public class CreateSkillDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public SkillLevel Level { get; set; }
}

public class UpdateSkillDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public SkillLevel? Level { get; set; }
}

public class SkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SkillLevel Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class SkillSummaryDto
{
    public int TotalSkills { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public Dictionary<SkillLevel, int> ByLevel { get; set; } = new();
    public List<SkillDto> RecentlyUpdated { get; set; } = new();
}
