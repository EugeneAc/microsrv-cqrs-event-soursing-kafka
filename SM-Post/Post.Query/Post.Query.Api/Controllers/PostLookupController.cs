using CQRS.Core.Infrastucture;
using Microsoft.AspNetCore.Mvc;
using Post.Common.DTOs;
using Post.Query.Api.DTOs;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;
using System.Security.Cryptography;

namespace Post.Query.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PostLookupController : ControllerBase
    {
        private readonly ILogger<PostLookupController> _logger;
        private readonly IQueryDispatcher<PostEntity> _queryDispatcher;

        public PostLookupController(ILogger<PostLookupController> logger, IQueryDispatcher<PostEntity> queryDispatcher)
        {
            _logger = logger;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPostAsync()
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindAllPostQuery());

                return NormallResponse(posts);

            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error processing request to retrieve all posts";
                return ErorResponse(ex, SAFE_ERROR_MESSAGE);
            }
        }

        [HttpGet("byId/{postId}")]
        public async Task<ActionResult> GetPostByIdAsync(Guid postId)
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindPostByIdQuery { Id = postId });

                if (posts == null || !posts.Any())
                    return NoContent();

                return Ok(new PostLookupResponse
                {
                    Posts = posts,
                    Message = $"Returned post"
                });

            }
            catch(Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error processing request to find the post by id";
                return ErorResponse(ex, SAFE_ERROR_MESSAGE);
            }
        }

        [HttpGet("byAuthor/{author}")]
        public async Task<ActionResult> GetPostsByAuthorAsync(string author)
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindPostByAuthorQuery { Author  = author });

                return NormallResponse(posts);

            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error processing request to find the posts by author";
                return ErorResponse(ex, SAFE_ERROR_MESSAGE);
            }
        }

        private ActionResult ErorResponse(Exception ex, string SAFE_ERROR_MESSAGE)
        {
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);

            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }

        [HttpGet("withComments")]
        public async Task<ActionResult> GetPostsWithCommentsAsync()
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindPostWithCommentsQuery());
                return NormallResponse(posts);
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error processing request to find the posts with comments";
                return ErorResponse(ex, SAFE_ERROR_MESSAGE);
            }
        }

        [HttpGet("withLikes/{numberOfLikes}")]
        public async Task<ActionResult> GetPostsWithLikesAsync(int numberOfLikes)
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindPostsWithLikesQuery() { NumberOfLikes = numberOfLikes});
                return NormallResponse(posts);
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error processing request to find the posts with likes";
                return ErorResponse(ex, SAFE_ERROR_MESSAGE);
            }
        }

        private ActionResult NormallResponse(List<PostEntity> posts)
        {
            if (posts == null || !posts.Any()) 
                return NoContent();

            var count = posts.Count;

            return Ok(new PostLookupResponse
            {
                Posts = posts,
                Message = $"Returned {count} post{(count > 1 ? "s" : string.Empty)}"
            });
        }
    }
}
