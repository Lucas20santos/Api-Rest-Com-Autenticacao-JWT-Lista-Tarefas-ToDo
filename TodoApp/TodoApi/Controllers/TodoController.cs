using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models.Domain;
using TodoApi.Models.DTOs;
using TodoApi.Repositories;

[ApiController]
[Route("api/todo")]
[Authorize]
public class TodoController : ControllerBase
{
    private readonly ITodoRepository _todoRepo;
    public TodoController(ITodoRepository todoRepo)
    {
        _todoRepo = todoRepo;
    }
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        return int.Parse(userIdClaim!);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        int userId = GetUserId();
        var todos = await _todoRepo.GetAllForUserAsync(userId);

        var result = todos.Select(t => new TodoReadDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            IsCompleted = t.IsCompleted,
            CreatedAt = t.CreatedAt
        });

        return Ok(result);
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TodoDto dto)
    {
        int userId = GetUserId();

        var todo = new TodoItem
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = userId
        };

        await _todoRepo.AddAsync(todo);
        await _todoRepo.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { todo.Id }, todo);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TodoDto dto)
    {
        int userId = GetUserId();
        var todo = await _todoRepo.GetByIdAsync(id);

        if (todo == null || todo.UserId != userId) return NotFound();

        todo.Title = dto.Title;
        todo.Description = dto.Description;

        await _todoRepo.UpdateAsync(todo);
        await _todoRepo.SaveChangesAsync();


        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = GetUserId();
        var todo = await _todoRepo.GetByIdAsync(id);

        if (todo == null || todo.UserId != userId) return NotFound();

        await _todoRepo.DeleteAsync(todo);
        await _todoRepo.SaveChangesAsync();

        return NoContent();
    }
    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        int userId = GetUserId();
        var todo = await _todoRepo.GetByIdAsync(id);

        if (todo == null || todo.UserId != userId) return NotFound();

        todo.IsCompleted = true;

        await _todoRepo.UpdateAsync(todo);
        await _todoRepo.SaveChangesAsync();

        return NoContent();
    }
}


