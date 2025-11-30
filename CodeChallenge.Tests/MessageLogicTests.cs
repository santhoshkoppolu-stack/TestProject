using Moq;
using CodeChallenge.Api.Repositories;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Logic;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;



namespace CodeChallenge.Tests
{
    public class MessageLogicTests
    {
        private readonly Mock<IMessageRepository> _repoMock;
        private readonly Guid _orgId = Guid.NewGuid();
        private readonly MessageLogic _logic;
        public MessageLogicTests() 
        {
            _repoMock = new Mock<IMessageRepository>();
            _logic = new MessageLogic(_repoMock.Object);
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnCreated_WhenValid()
        {
            var request = new CreateMessageRequest
            {
                Title = "Hello World",
                Content = "This is valid content"
            };

            _repoMock.Setup(r => r.GetByTitleAsync(It.IsAny<Guid>(), request.Title))
                     .ReturnsAsync((Message)null);

            var result = await _logic.CreateMessageAsync(Guid.NewGuid(), request);

            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnConflict_WhenTitleExists()
        {
            var request = new CreateMessageRequest
            {
                Title = "Santhosh Existing",
                Content = "Test content"
            };

            _repoMock.Setup(r => r.GetByTitleAsync(It.IsAny<Guid>(), request.Title))
                     .ReturnsAsync(new Message());

            var result = await _logic.CreateMessageAsync(Guid.NewGuid(), request);

            result.Should().BeOfType<Conflict>();
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnValidationError_WhenContentInvalid()
        {
            var request = new CreateMessageRequest
            {
                Title = "Valid Title",
                Content = "short" // <10 chars
            };

            _repoMock.Setup(r => r.GetByTitleAsync(It.IsAny<Guid>(), request.Title))
                     .ReturnsAsync((Message)null);

            var result = await _logic.CreateMessageAsync(Guid.NewGuid(), request);

            result.Should().BeOfType<ValidationError>();
        }
        
        [Fact]
        public async Task UpdateMessage_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                     .ReturnsAsync((Message)null);

            var request = new UpdateMessageRequest
            {
                Title = "Eqiscript",
                Content = "Test Content"
            };

            var result = await _logic.UpdateMessageAsync(Guid.NewGuid(), Guid.NewGuid(), request);

            result.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task UpdateMessage_ShouldReturnConflict_WhenMessageInactive()
        {
            var msg = new Message
            {
                Id = Guid.NewGuid(),
                IsActive = false
            };

            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                     .ReturnsAsync(msg);

            var request = new UpdateMessageRequest
            {
                Title = "Value Labs",
                Content = "Value Labs content"
            };

            var result = await _logic.UpdateMessageAsync(Guid.NewGuid(), msg.Id, request);

            result.Should().BeOfType<Conflict>();
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturnNotFound_WhenMessageMissing()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                     .ReturnsAsync((Message)null);

            var result = await _logic.DeleteMessageAsync(Guid.NewGuid(), Guid.NewGuid());

            result.Should().BeOfType<NotFound>();
        }


    }
}