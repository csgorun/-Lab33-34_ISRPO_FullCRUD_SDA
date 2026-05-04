using Microsoft.AspNetCore.Mvc;
using NotesApp.Helpers;
using NotesApp.Models;
using NotesApp.Models.DTOs;
using NotesApp.Repositories;

namespace NotesApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly INoteRepository _noteRepo;
    private readonly ICategoryRepository _categoryRepo;

    public NotesController(INoteRepository noteRepo, ICategoryRepository categoryRepo)
    {
        _noteRepo = noteRepo;
        _categoryRepo = categoryRepo;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NoteResponseDto>>>> GetAll(
        [FromQuery] bool? archived = false,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool? isPinned = null,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool descending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var filter = new NoteFilterDto
        {
            Archived = archived,
            CategoryId = categoryId,
            IsPinned = isPinned,
            Search = search,
            SortBy = sortBy,
            Descending = descending,
            Page = page,
            PageSize = pageSize
        };

        var notes = await _noteRepo.GetAllAsync(filter);
        return Ok(ApiResponse<IEnumerable<NoteResponseDto>>.Ok("Успешно", notes));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<NoteResponseDto>>> GetById(int id)
    {
        var note = await _noteRepo.GetByIdAsync(id);
        if (note is null)
            return NotFound(ApiError.NotFound($"Заметка с id={id} не найдена"));

        return Ok(ApiResponse<NoteResponseDto>.Ok("Успешно", note));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<NoteResponseDto>>> Create([FromBody] CreateNoteDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiError.BadRequest("Ошибка валидации", errors));
        }

        if (!await _categoryRepo.ExistsAsync(dto.CategoryId))
            return BadRequest(ApiError.BadRequest($"Категория с id={dto.CategoryId} не существует"));

        var note = new Note
        {
            Title = dto.Title,
            Content = dto.Content,
            CategoryId = dto.CategoryId,
            Priority = dto.Priority,
            IsPinned = false,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _noteRepo.CreateAsync(note);
        var response = await _noteRepo.GetByIdAsync(note.Id);
        return CreatedAtAction(nameof(GetById), new { id = note.Id }, ApiResponse<NoteResponseDto>.Created("Заметка создана", response!));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<NoteResponseDto>>> Update(int id, [FromBody] UpdateNoteDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiError.BadRequest("Ошибка валидации", errors));
        }

        var note = await _noteRepo.FindAsync(id);
        if (note is null)
            return NotFound(ApiError.NotFound($"Заметка с id={id} не найдена"));

        if (!await _categoryRepo.ExistsAsync(dto.CategoryId))
            return BadRequest(ApiError.BadRequest($"Категория с id={dto.CategoryId} не существует"));

        note.Title = dto.Title;
        note.Content = dto.Content;
        note.CategoryId = dto.CategoryId;
        note.Priority = dto.Priority;

        await _noteRepo.UpdateAsync(note);
        var response = await _noteRepo.GetByIdAsync(note.Id);
        return Ok(ApiResponse<NoteResponseDto>.Ok("Заметка обновлена", response!));
    }

    [HttpPatch("{id}/pin")]
    public async Task<ActionResult<ApiResponse<NoteResponseDto>>> TogglePin(int id)
    {
        var note = await _noteRepo.FindAsync(id);
        if (note is null)
            return NotFound(ApiError.NotFound($"Заметка с id={id} не найдена"));

        note.IsPinned = !note.IsPinned;
        await _noteRepo.UpdateAsync(note);

        var response = await _noteRepo.GetByIdAsync(note.Id);
        var message = note.IsPinned ? "Заметка закреплена" : "Заметка откреплена";
        return Ok(ApiResponse<NoteResponseDto>.Ok(message, response!));
    }

    [HttpPatch("{id}/archive")]
    public async Task<ActionResult<ApiResponse<NoteResponseDto>>> ToggleArchive(int id)
    {
        var note = await _noteRepo.FindAsync(id);
        if (note is null)
            return NotFound(ApiError.NotFound($"Заметка с id={id} не найдена"));

        note.IsArchived = !note.IsArchived;
        if (note.IsArchived)
            note.IsPinned = false;

        await _noteRepo.UpdateAsync(note);
        var response = await _noteRepo.GetByIdAsync(note.Id);
        var message = note.IsArchived ? "Заметка архивирована" : "Заметка восстановлена";
        return Ok(ApiResponse<NoteResponseDto>.Ok(message, response!));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var note = await _noteRepo.FindAsync(id);
        if (note is null)
            return NotFound(ApiError.NotFound($"Заметка с id={id} не найдена"));

        await _noteRepo.DeleteAsync(note);
        return NoContent();
    }
}