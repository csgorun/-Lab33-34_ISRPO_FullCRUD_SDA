using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$")]
    public string Color { get; set; } = "#3498db";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Note> Notes { get; set; } = new();
}