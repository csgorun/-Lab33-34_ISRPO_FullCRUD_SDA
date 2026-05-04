using Microsoft.AspNetCore.Mvc;
using NotesApp.Helpers;
using NotesApp.Models;
using NotesApp.Models.DTOs;
using NotesApp.Repositories;

namespace NotesApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _repo;

    public CategoriesController(ICategoryRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponseDto>>>> GetAll()
    {
        var categories = await _repo.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<CategoryResponseDto>>.Ok("Успешно", categories));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> GetById(int id)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category is null)
            return NotFound(ApiError.NotFound($"Категория с id={id} не найдена"));

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            CreatedAt = category.CreatedAt,
            NotesCount = category.Notes.Count(n => !n.IsArchived)
        };

        return Ok(ApiResponse<CategoryResponseDto>.Ok("Успешно", response));
    }

    [HttpGet("{id}/notes")]
    public async Task<ActionResult<ApiResponse<object>>> GetWithNotes(int id)
    {
        var category = await _repo.GetByIdWithNotesAsync(id);
        if (category is null)
            return NotFound(ApiError.NotFound($"Категория с id={id} не найдена"));

        var response = new
        {
            CategoryId = category.Id,
            CategoryName = category.Name,
            Notes = category.Notes.Select(n => new { n.Id, n.Title, n.IsPinned, n.Priority, n.CreatedAt }).ToList()
        };

        return Ok(ApiResponse<object>.Ok("Успешно", response));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiError.BadRequest("Ошибка валидации", errors));
        }

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(category);

        var response = new CategoryResponseDto
        {
            Id = created.Id,
            Name = created.Name,
            Description = created.Description,
            Color = created.Color,
            CreatedAt = created.CreatedAt,
            NotesCount = 0
        };

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<CategoryResponseDto>.Created("Категория создана", response));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiError.BadRequest("Ошибка валидации", errors));
        }

        var category = await _repo.GetByIdAsync(id);
        if (category is null)
            return NotFound(ApiError.NotFound($"Категория с id={id} не найдена"));

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.Color = dto.Color;

        await _repo.UpdateAsync(category);

        var response = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            CreatedAt = category.CreatedAt,
            NotesCount = category.Notes?.Count(n => !n.IsArchived) ?? 0
        };

        return Ok(ApiResponse<CategoryResponseDto>.Ok("Категория обновлена", response));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category is null)
            return NotFound(ApiError.NotFound($"Категория с id={id} не найдена"));

        if (await _repo.HasNotesAsync(id))
            return BadRequest(ApiError.BadRequest("Невозможно удалить категорию, в которой есть заметки"));

        await _repo.DeleteAsync(category);
        return NoContent();
    }
}