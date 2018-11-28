using MediatR;
using Microsoft.Extensions.Logging;
using Posting.Application.Helpers;
using Posting.Application.Interfaces;
using Posting.Domain.AggregatesModel.CommentAggregate;
using Posting.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Posting.Application.Command
{
    public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, bool>
    {

        private readonly IRepository<Comment> _commentRepository;
        private readonly ILoggerFactory _loggerFactory;

        public UpdateCommentCommandHandler(IRepository<Comment> commentRepository,
            ILoggerFactory loggerFactory)
        {
            _commentRepository = commentRepository ?? throw new ArgumentNullException(nameof(commentRepository));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task<bool> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var uploadedContents = UploadedContentHelper.Extract(request);

            var comment = await _commentRepository.GetByIdAsync(request.CommentId);

            if (comment == null)
                throw new CommentNotFoundException($"Comment with id {request.CommentId} was not found");

            if (!comment.HasOwner(request.PosterId))
                throw new PosterIsNotCommentOwnerException(
                    $"Poster with id {request.PosterId} is not allowed to delete comment with id {request.CommentId}");

            comment.UpdateContent(uploadedContents);

            _commentRepository.Update(comment);

            var result = await _commentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            _loggerFactory.CreateLogger(nameof(UpdateCommentCommandHandler))
                .LogTrace(
                    $"Poster with id {request.PosterId} updated comment with id {request.CommentId} successfully");

            return result;
        }
    }
}
