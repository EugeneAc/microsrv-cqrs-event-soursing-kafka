using CQRS.Core.Exceptions;
using CQRS.Core.Infrastucture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EditMessageController : ControllerBase
    {
        private readonly ILogger<NewPostController> _logger;
        private readonly ICommandDispatcher _commandDispatcher;

        public EditMessageController(ILogger<NewPostController> logger, ICommandDispatcher commandDispatcher)
        {
            _logger = logger;
            _commandDispatcher = commandDispatcher;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> EditMessageAsync(Guid id, EditMessageCommand command)
        {
            try
            {
                command.Id = id;
                await _commandDispatcher.SendAsync(command);

                return Ok(new BaseResponse
                {
                    Message = "Edit message request completed"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.Log(LogLevel.Warning, "Client made a bad request");
                return BadRequest(new BaseResponse { Message = ex.Message });
            }
            catch (AggregateNotFoundException ex)
            {
                _logger.Log(LogLevel.Warning, "Could not retrieve aggregate, client passed an incorrect post id");
                return BadRequest(new BaseResponse { Message = ex.Message });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Eror while processing request to edit a post";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
