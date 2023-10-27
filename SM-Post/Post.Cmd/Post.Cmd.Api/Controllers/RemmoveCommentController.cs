using CQRS.Core.Exceptions;
using CQRS.Core.Infrastucture;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RemmoveCommentController : ControllerBase
    {
        private readonly ILogger<RemmoveCommentController> _logger;
        private readonly ICommandDispatcher _commandDispatcher;

        public RemmoveCommentController(ILogger<RemmoveCommentController> logger, ICommandDispatcher commandDispatcher)
        {
            _logger = logger;
            _commandDispatcher = commandDispatcher;
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemmoveCommentAsync(Guid id, RemoveCommentCommand command)
        {
            try
            {
                command.Id = id;
                await _commandDispatcher.SendAsync(command);

                return Ok(new BaseResponse
                {
                    Message = "Remmove comment message completed"
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
                const string SAFE_ERROR_MESSAGE = "Eror while processing request to remmove a comment";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
