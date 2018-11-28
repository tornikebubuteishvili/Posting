using MediatR;
using Posting.Application.Interfaces;
using Posting.Domain.AggregatesModel.PostAggregate;
using Posting.Domain.AggregatesModel.PosterAggregate;
using Posting.Domain.Events.Posting;
using Posting.Domain.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Posting.Application.DomainEventHandlers
{
    public class PostContentUpdatedDomainEventHandler : INotificationHandler<PostContentUpdatedDomainEvent>
    {

        private readonly IRepository<Poster> _posterRepository;

        public PostContentUpdatedDomainEventHandler(IRepository<Poster> posterRepository)
        {
            _posterRepository = posterRepository ?? throw new ArgumentNullException(nameof(posterRepository));
        }

        public async Task Handle(PostContentUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {

            var uploadedContents = notification.NewUploadedContents.Select(newUploadedContent =>
                new UploadedContent(newUploadedContent.Content, newUploadedContent.UploadedContentTypeId,
                    newUploadedContent.Capacity)).ToList();

            var poster = await _posterRepository.GetByIdAsync(notification.PosterId);

            if (poster == null)
                throw new PosterNotFoundException($"Poster with id {notification.PosterId} not found");

            poster.UpdatePostContent(uploadedContents);

            _posterRepository.Update(poster);
        }
    }
}
