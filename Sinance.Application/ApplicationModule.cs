using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sinance.Application.Behaviours;
using Sinance.Application.Command.Validators;
using Sinance.Application.DomainEventHandlers;

namespace Sinance.Application
{
    public static class ApplicationModule
    {
        public static IServiceCollection RegisterApplicationModule(this IServiceCollection services)
        {
            typeof(SplitAccountTransactionCommandValidator).Assembly.GetTypes().Where(x => x.IsClosedTypeOf(typeof(IValidator<>)))
                .ToList()
                .ForEach(x =>
                {
                    var serviceTypes = x.GetClosedTypesOf(typeof(IValidator<>));
                    foreach (var serviceType in serviceTypes)
                    {
                        services.AddTransient(serviceType, x);
                    }
                });

            return services.AddMediatR(Assembly.GetExecutingAssembly())
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));
        }

        /// <summary>
        /// Checks whether this type is a closed type of a given generic type.
        /// </summary>
        /// <param name="this">The type we are checking.</param>
        /// <param name="openGeneric">The open generic type to validate against.</param>
        /// <returns>True if <paramref name="this"/> is a closed type of <paramref name="openGeneric"/>. False otherwise.</returns>
        public static List<Type> GetClosedTypesOf(this Type @this, Type openGeneric)
        {
            return TypesAssignableFrom(@this).Where(t => t.IsGenericType && !@this.ContainsGenericParameters && t.GetGenericTypeDefinition() == openGeneric).ToList();
        }

        /// <summary>
        /// Checks whether this type is a closed type of a given generic type.
        /// </summary>
        /// <param name="this">The type we are checking.</param>
        /// <param name="openGeneric">The open generic type to validate against.</param>
        /// <returns>True if <paramref name="this"/> is a closed type of <paramref name="openGeneric"/>. False otherwise.</returns>
        public static bool IsClosedTypeOf(this Type @this, Type openGeneric)
        {
            return TypesAssignableFrom(@this).Any(t => t.IsGenericType && !@this.ContainsGenericParameters && t.GetGenericTypeDefinition() == openGeneric);
        }

        private static IEnumerable<Type> TypesAssignableFrom(Type candidateType)
        {
            return candidateType.GetInterfaces().Concat(
                Traverse.Across(candidateType, t => t.BaseType!));
        }
        /// <summary>
        /// Provides a method to support traversing structures.
        /// </summary>
        internal static class Traverse
        {
            /// <summary>
            /// Traverse across a set, taking the first item in the set, and a function to determine the next item.
            /// </summary>
            /// <typeparam name="T">The set type.</typeparam>
            /// <param name="first">The first item in the set.</param>
            /// <param name="next">A callback that will take the current item in the set, and output the next one.</param>
            /// <returns>An enumerable of the set.</returns>
            public static IEnumerable<T> Across<T>(T first, Func<T, T> next)
                where T : class
            {
                var item = first;
                while (item != null)
                {
                    yield return item;
                    item = next(item);
                }
            }
        }
    }
}