using System.ComponentModel.DataAnnotations;
namespace NotesApp.Models;

public class Category
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Note> Notes { get; set; } = new(
    );
}