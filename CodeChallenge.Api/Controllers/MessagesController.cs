using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CodeChallenge.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationId}/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MessagesController> _logger;
    private readonly IMessageLogic _messageLogic;

    public MessagesController(IMessageRepository repository, ILogger<MessagesController> logger,
        IMessageLogic messageLogic)
    {
        _repository = repository;
        _logger = logger;
        _messageLogic = messageLogic;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> GetAll(Guid organizationId)
    {
        var result = await _messageLogic.GetAllMessagesAsync(organizationId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        var result = await _messageLogic.GetMessageAsync(organizationId, id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        var result = await _messageLogic.CreateMessageAsync(organizationId, request);
        return result.ToActionResult(this);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        var result = await _messageLogic.UpdateMessageAsync(id, organizationId, request);
        return result.ToActionResult(this);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid organizationId, Guid id)
    {
        var result = await _messageLogic.DeleteMessageAsync(organizationId, id);
        return result.ToActionResult(this);

    }
}
