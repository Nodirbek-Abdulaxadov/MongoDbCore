using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TodoAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly TodoService _todoService;

    public TodosController(TodoService todoService)
    {
        _todoService = todoService;
    }

    #region Create

    [HttpPost]
    public IActionResult Create(string task)
    {
        var todo = _todoService.Create(task);
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    [HttpPost("async")]
    public async Task<IActionResult> CreateAsync(string task)
    {
        var todo = await _todoService.CreateAsync(task);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = todo.Id }, todo);
    }

    #endregion

    #region Read

    [HttpGet]
    public ActionResult<IEnumerable<Todo>> GetAll()
    {
        return Ok(_todoService.GetAll());
    }

    [HttpGet("async")]
    public async Task<ActionResult<IEnumerable<Todo>>> GetAllAsync()
    {
        var todos = await _todoService.GetAllAsync();
        return Ok(todos);
    }

    #endregion

    #region Update

    [HttpPut]
    public IActionResult Update(Todo todo)
    {
        var updatedTodo = _todoService.Update(todo);
        return Ok(updatedTodo);
    }

    [HttpPut("async")]
    public async Task<IActionResult> UpdateAsync(Todo todo)
    {
        var updatedTodo = await _todoService.UpdateAsync(todo);
        return Ok(updatedTodo);
    }

    #endregion

    #region Delete

    [HttpDelete]
    public IActionResult Delete(Todo todo)
    {
        _todoService.Delete(todo);
        return NoContent();
    }

    [HttpDelete("async")]
    public async Task<IActionResult> DeleteAsync(Todo todo)
    {
        await _todoService.DeleteAsync(todo);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteById(string id)
    {
        _todoService.DeleteById(id);
        return NoContent();
    }

    [HttpDelete("{id}/async")]
    public async Task<IActionResult> DeleteByIdAsync(string id)
    {
        await _todoService.DeleteByIdAsync(id);
        return NoContent();
    }

    #endregion

    #region FirstOrDefault

    [HttpGet("{id}")]
    public ActionResult<Todo?> GetById(string id)
    {
        var todo = _todoService.GetById(id);
        return todo is not null ? Ok(todo) : NotFound();
    }

    [HttpGet("{id}/async")]
    public async Task<ActionResult<Todo?>> GetByIdAsync(string id)
    {
        var todo = await _todoService.GetByIdAsync(id);
        return todo is not null ? Ok(todo) : NotFound();
    }

    #endregion

    #region Where

    [HttpGet("filter")]
    public ActionResult<IEnumerable<Todo>> Filter(DateTime start, DateTime end)
    {
        var todos = _todoService.Filter(start, end);
        return Ok(todos);
    }

    #endregion

    #region BulkWrite

    [HttpPost("bulk")]
    public ActionResult<IEnumerable<Todo>> CreateRange(IEnumerable<string> tasks)
    {
        var todos = _todoService.CreateRange(tasks);
        return Ok(todos);
    }

    [HttpPost("bulk/async")]
    public async Task<ActionResult<IEnumerable<Todo>>> CreateRangeAsync(IEnumerable<string> tasks)
    {
        var todos = await _todoService.CreateRangeAsync(tasks);
        return Ok(todos);
    }

    #endregion

    #region BulkUpdate

    [HttpPut("bulk")]
    public IActionResult UpdateMany()
    {
        var updatedCount = _todoService.UpdateMany();
        return Ok(new { UpdatedCount = updatedCount });
    }

    [HttpPut("bulk/async")]
    public async Task<IActionResult> UpdateManyAsync()
    {
        var updatedCount = await _todoService.UpdateManyAsync();
        return Ok(new { UpdatedCount = updatedCount });
    }

    #endregion

    #region BulkDelete

    [HttpDelete("bulk")]
    public IActionResult DeleteRange(IEnumerable<Todo> todos)
    {
        _todoService.DeleteRange(todos);
        return NoContent();
    }

    [HttpDelete("bulk/async")]
    public async Task<IActionResult> DeleteRangeAsync(IEnumerable<Todo> todos)
    {
        await _todoService.DeleteRangeAsync(todos);
        return NoContent();
    }

    #endregion
}