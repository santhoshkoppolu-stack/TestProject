using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic
{
    public class MessageLogic : IMessageLogic
    {
        private readonly Message _messages = new();
        private readonly IMessageRepository _messageRepository;
        public MessageLogic(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
        {
            return await _messageRepository.GetAllByOrganizationAsync(organizationId);
        }

        public async Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
        {
            return await _messageRepository.GetByIdAsync(organizationId, id);
        }

        public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
        {
            var validation = ValidateMessage(request.Title, request.Content);
            if (validation is ValidationError)
                return validation;

            var existing = await _messageRepository.GetByTitleAsync(organizationId, request.Title);
            if (existing != null)
                return new Conflict("A message with this title already exists.");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                OrganizationId = organizationId,
                IsActive = true
            };

            await _messageRepository.CreateAsync(message);

            return new Created<Message>(message);

        }
        public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
        {
            var message = await _messageRepository.GetByIdAsync(organizationId, id);

            if (message == null)
                return new NotFound("Message not found.");

            if (!message.IsActive)
                return new Conflict("Cannot update an inactive message.");

            var validation = ValidateMessage(request.Title, request.Content);
            if (validation is ValidationError)
                return validation;

            message.Title = request.Title;
            message.Content = request.Content;
            message.UpdatedAt = DateTime.UtcNow;

            await _messageRepository.UpdateAsync(message);

            return new Updated();
        }

        public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
        {
            var message = await _messageRepository.GetByIdAsync(organizationId, id);

            if (message == null)
                return new NotFound("Message not found.");

            await _messageRepository.DeleteAsync(organizationId, id);
            return new Deleted();
        }

        private Result ValidateMessage(string title, string content)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(title))
                errors["Title"] = new[] { "Title is required." };
            else if (title.Length < 3 || title.Length > 200)
                errors["Title"] = new[] { "Title must be between 3 and 200 characters." };

            if (string.IsNullOrWhiteSpace(content) || content.Length < 10 || content.Length > 1000)
            {
                errors["Content"] = new[] { "Content must be between 10 and 1000 characters." };
            }

            return errors.Any() ? new ValidationError(errors) : new Success();
        }
    }
}
