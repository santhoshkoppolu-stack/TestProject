using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic
{
    public class MessageLogic : IMessageLogic
    {
        private readonly Message _messages = new();
        private readonly IMessageRepository _messageRepository;
        private readonly ILogger<MessageLogic> _logger;
        private readonly object _lock = new();
        public MessageLogic(ILogger<MessageLogic> logger,
            IMessageRepository messageRepository)
        {
            _logger = logger;
            _messageRepository = messageRepository;

        }

        public Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
        {
            lock (_lock)
            {
                var messages = _messageRepository.GetAllByOrganizationAsync(organizationId);
                return Task.FromResult<IEnumerable<Message>>(messages.Result);
            }
        }

        public Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
        {
            lock (_lock)
            {
                var message = _messageRepository.GetByIdAsync(organizationId, id);
                return Task.FromResult<Message?>(message.Result);
            }
        }

        public Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
        {
            lock (_lock)
            {
                var errors = new Dictionary<string, string[]>();

                
                if (string.IsNullOrWhiteSpace(request.Title))
                    errors["Title"] = new[] { "Title is required." };

                
                if (!string.IsNullOrWhiteSpace(request.Title) &&
                    (request.Title.Length < 3 || request.Title.Length > 200))
                    errors["Title"] = new[] { "Title must be between 3 and 200 characters." };

                
                if (string.IsNullOrWhiteSpace(request.Content) ||
                    request.Content.Length < 10 || request.Content.Length > 1000)
                    errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };

                
                var result = _messageRepository.GetByTitleAsync(organizationId, request.Title).Result;
                if (result != null)
                    return Task.FromResult<Result>(new Conflict("A message with this title already exists."));

                if (errors.Any())
                    return Task.FromResult<Result>(new ValidationError(errors));
                _messages.Title = request.Title;
                _messages.Content = request.Content;
                _messages.CreatedAt = DateTime.Now;
                _messages.OrganizationId = organizationId;
                _messageRepository.CreateAsync(_messages);

                return Task.FromResult<Result>(new Created<Message>(_messages));
            }
        }
        public Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
        {
            lock (_lock)
            {
                var message = _messageRepository.GetByIdAsync(id, organizationId).Result;

                
                if (message == null)
                    return Task.FromResult<Result>(new NotFound("Message not found."));

               
                if (message != null && !message.IsActive)
                    return Task.FromResult<Result>(new Conflict("Cannot update an inactive message."));

               
                var errors = new Dictionary<string, string[]>();

                if (string.IsNullOrWhiteSpace(request.Title))
                    errors["Title"] = new[] { "Title is required." };

                if (!string.IsNullOrWhiteSpace(request.Title) &&
                    (request.Title.Length < 3 || request.Title.Length > 200))
                    errors["Title"] = new[] { "Title must be between 3 and 200 characters." };

                if (string.IsNullOrWhiteSpace(request.Content) ||
                    request.Content.Length < 10 || request.Content.Length > 1000)
                    errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };

                if (errors.Any())
                    return Task.FromResult<Result>(new ValidationError(errors));

                _messages.Title = request.Title;
                _messages.Content = request.Content;
                _messages.UpdatedAt = DateTime.Now;
                _messages.OrganizationId = organizationId;
                _messageRepository.UpdateAsync(_messages);
                return Task.FromResult<Result>(new Updated());

            }

        }

        public Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
        {
            lock (_lock)
            {
                _messageRepository.DeleteAsync(organizationId, id);
                return Task.FromResult<Result>(new Deleted());
            }
        }
    }
}
