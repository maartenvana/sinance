using MediatR;
using Sinance.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinance.Infrastructure.Extensions
{
    public static class MediatorExtensions
    {
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, SinanceContext context)
        {
            // Mark all entities that are going to be deleted to be deleted so any domain events can be emitted;
            context.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.State == Microsoft.EntityFrameworkCore.EntityState.Deleted)
                .ToList()
                .ForEach(x => x.Entity.MarkAsDeleted());
            
            var domainEntities = context.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
                await mediator.Publish(domainEvent);
        }
    }
}
