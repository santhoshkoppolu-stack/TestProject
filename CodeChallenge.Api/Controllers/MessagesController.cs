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
        try
        {
            var result = await _messageLogic.GetAllMessagesAsync(organizationId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error Occured While Calling this GetAllMessagesAsync ",ex);
            return StatusCode(500, ex.Message);
        }

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetById(Guid organizationId, Guid id)
    {
        try
        {
            var result=await _messageLogic.GetMessageAsync(organizationId, id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error Occured While Calling this GetById ", ex);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<Message>> Create(Guid organizationId, [FromBody] CreateMessageRequest request)
    {
        try
        {
            var result = await _messageLogic.CreateMessageAsync(organizationId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error Occured While Calling this Create Method ", ex);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid organizationId, Guid id, [FromBody] UpdateMessageRequest request)
    {
        try
        {
            var result = await _messageLogic.UpdateMessageAsync(id,organizationId, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error Occured While Calling this Update Method ", ex);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid organizationId, Guid id)
    {
        try
        {
            var result = await _messageLogic.DeleteMessageAsync(organizationId,id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error Occured While Calling this Update Method ", ex);
            return StatusCode(500, ex.Message);
        }
    }
}
