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
                Title = "Test user",
                Content = "This is valid test user content"
            };

            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title))
                     .ReturnsAsync((Message)null);

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Created<Message>>();

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnConflict_WhenTitleExists()
        {
            var request = new CreateMessageRequest
            {
                Title = "Santhosh Existing",
                Content = "Test content"
            };

            _repoMock.Setup(r => r.GetByTitleAsync(_orgId, request.Title))
                     .ReturnsAsync(new Message { Title = request.Title });

            var result = await _logic.CreateMessageAsync(_orgId, request);

            result.Should().BeOfType<Conflict>();

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Never);
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnValidationError_WhenContentTooShort()
        {
            var request = new CreateMessageRequest
            {
                Title = "Resource",
                Content = "value" // <10 chars
            };

            _repoMock.Setup(r => r.GetByTitleAsync(It.IsAny<Guid>(), request.Title))
                     .ReturnsAsync((Message)null);

            var result = await _logic.CreateMessageAsync(Guid.NewGuid(), request);

            result.Should().BeOfType<ValidationError>();

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Message>()), Times.Never);
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

            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Message>()), Times.Never);
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

            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Message>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMessage_ShouldReturnValidationError_WhenInvalidTitle()
        {
            var msg = new Message { Id = Guid.NewGuid(), IsActive = true };

            _repoMock.Setup(r => r.GetByIdAsync(_orgId, msg.Id))
                     .ReturnsAsync(msg);

            var req = new UpdateMessageRequest
            {
                Title = "D",  
                Content = "content"
            };

            var result = await _logic.UpdateMessageAsync(_orgId, msg.Id, req);

            result.Should().BeOfType<ValidationError>();

            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Message>()), Times.Never);
        }

            [Fact]
        public async Task DeleteMessage_ShouldReturnNotFound_WhenMessageDoesNotExist()
        {
            _repoMock.Setup(r => r.GetByIdAsync(_orgId, It.IsAny<Guid>()))
                     .ReturnsAsync((Message)null);

            var result = await _logic.DeleteMessageAsync(_orgId, Guid.NewGuid());

            result.Should().BeOfType<NotFound>();

            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);

        }

        [Fact]
        public async Task DeleteMessage_ShouldReturnDeleted_WhenMessageExists()
        {
            var msg = new Message { Id = Guid.NewGuid(), IsActive = true };

            _repoMock.Setup(r => r.GetByIdAsync(_orgId, msg.Id))
                     .ReturnsAsync(msg);

            var result = await _logic.DeleteMessageAsync(_orgId, msg.Id);

            result.Should().BeOfType<Deleted>();

            _repoMock.Verify(r => r.DeleteAsync(_orgId, msg.Id), Times.Once);
        }
    }
}