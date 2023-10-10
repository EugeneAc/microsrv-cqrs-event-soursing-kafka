using CQRS.Core.Domain;
using CQRS.Core.Messages;
using Post.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Post.Cmd.Domain.Aggregates
{
    public class PostAggregate: AggregateRoot   
    {
        private bool _active;
        private string _author;
        private readonly Dictionary<Guid, Tuple<string, string>> _comments = new();
        public bool Active
        {
            get { return _active; } set { _active = value; }
        }
        public PostAggregate()
        {

        }
        public PostAggregate(Guid id, string author, string message)
        {
            RaiseEvent(new PostCreatedEvent
            {
                Id = id,
                Author = author,
                Message = message,
                DatePosted = DateTime.Now,
            });
        }
        public void Apply(PostCreatedEvent @event)
        {
            _id = @event.Id;
            _active = true;
            _author = @event.Author;
        }

        public void EditMessage(string message)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Can not edit inactive post");
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new InvalidOperationException($"The value of {nameof(message)} can not be null or empty");
            }

            RaiseEvent(new MessageUpdatedEvent
            {
                Id = _id,
                Message = message
            });
        }
        public void Apply(MessageUpdatedEvent @event) 
        {
            _id = @event.Id;
        }

        public void LikePost()
        {
            if (!_active)
            {
                throw new InvalidOperationException("Can not like inactive post");
            }

            RaiseEvent(new PostLikedEvent { Id = _id });
        }

        public void Apply(PostLikedEvent @event)
        {
            _id = @event.Id;
        }

        public void AddComment(string comment, string userName)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Can not comment inactive post");
            }
            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new InvalidOperationException($"The value of {nameof(comment)} can not be null or empty");
            }

            RaiseEvent(new CommentAddedEvent
            {
                Id = _id,
                CommentId = Guid.NewGuid(),
                Comment = comment,
                UserName = userName,
                CommentDate = DateTime.UtcNow
            });
        }

        public void Apply(CommentAddedEvent @event)
        {
            _id = @event.Id;
            _comments.Add(@event.CommentId, new Tuple<string, string>(@event.Comment, @event.UserName));
        }

        public void EditComment(Guid commentId, string comment, string userName)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Can not edit comment inactive post");
            }
            if (!_comments[commentId].Item2.Equals(userName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("Can not edit another user comment");
            }

            RaiseEvent(new CommentUpdatedEvent { Id = _id, CommentId = commentId, UserName = userName, Comment = comment, EditDate = DateTime.Now });
        }

        public void Apply(CommentUpdatedEvent @event)
        {
            _id = @event.Id;
            _comments[@event.CommentId] = new Tuple<string, string>(@event.Comment, @event.UserName);
        }
        
        public void RemoveComment(Guid commentId, string userName)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Can not remove comment inactive post");
            }
            if (!_comments[commentId].Item2.Equals(userName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("Can not remove another user comment");
            }

            RaiseEvent(new CommentRemovedEvent { Id = _id, CommentId = commentId });
        }

        public void Apply(CommentRemovedEvent @event)
        {
            _id = @event.Id;
            _comments.Remove(@event.CommentId);
        }

        public void DeletePost(string userName)
        {
            if (!_active)
            {
                throw new InvalidOperationException("Post already removed");
            }
            if (!_author.Equals(userName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new InvalidOperationException("Can not delete another user post");
            }

            RaiseEvent(new PostRemovedEvent
            {
                Id = _id
            });
        }

        public void Apply(PostRemovedEvent @event)
        {
            _id = @event.Id;
            _active = false;
        }
    }
}
