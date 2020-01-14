using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Rnd.Core.ConsoleApp.AWS.Dynamo.v3
{
    /// <summary>
    ///     Class with extension methods on <see cref="IDependencyCoordinator"/>
    /// </summary>
    public static class DependencyCoordinatorExtensions
    {
        /// <summary>
        ///     Helps call the <see cref="IDependencyCoordinator"/> with the correct parameters.
        /// </summary>
        /// <typeparam name="TResult">The type of the result</typeparam>
        /// <param name="coordinator">The instance to use as the dependency coordinator</param>
        /// <param name="work">The function to be invoked</param>
        /// <param name="name">The name of the dependency</param>
        /// <param name="addMetadataFunc">Any additional information to be passed into the metadata of the dependency</param>
        /// <returns>an awaitable <see cref="Task{TResult}"/></returns>
        public static Task<TResult> CallDependency<TResult>(
            this IDependencyCoordinator coordinator,
            Func<Task<TResult>> work,
            [CallerMemberName] string? name = null,
            Action<TResult, Dictionary<string, object>>? addMetadataFunc = null)
        {
            var metadata = new DependencyMetadata<TResult>(name, addMetadataFunc);
            return coordinator.ExecuteAsync(work, metadata);
        }

        /// <summary>
        ///     Helps call the <see cref="IDependencyCoordinator"/> with the correct parameters.
        /// </summary>
        /// <param name="coordinator">The instance to use as the dependency coordinator</param>
        /// <param name="work">The function to be invoked</param>
        /// <param name="name">The name of the dependency</param>
        /// <param name="additionalMetadata">Any additional information to be passed into the metadata of the dependency</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        public static Task CallDependency(
            this IDependencyCoordinator coordinator,
            Func<Task> work,
            [CallerMemberName] string? name = null,
            Dictionary<string, object>? additionalMetadata = null)
        {
            var metadata = new DependencyMetadata(name, additionalMetadata);
            return coordinator.ExecuteAsync(work, metadata);
        }
    }

    /// <summary>
    ///     Class which represents metadata for dependency calls
    /// </summary>
    public class DependencyMetadata : OperationMetadata
    {
        /// <summary>
        ///     Constructor to create an instance of <see cref="DependencyMetadata"/> 
        /// </summary>
        public DependencyMetadata()
        {
        }

        /// <summary>
        ///     Constructor to create an instance of <see cref="DependencyMetadata"/>
        /// </summary>
        /// <param name="dependencyName">The name of the dependency call</param>
        /// <param name="additionalMetadata">Additional metadata for the dependency</param>
        public DependencyMetadata(string dependencyName, Dictionary<string, object>? additionalMetadata = null)
        {
            DependencyName = dependencyName;
            AdditionalMetadata = additionalMetadata ?? new Dictionary<string, object>();
        }

        /// <summary>
        ///     The name of the dependency call
        /// </summary>
        public string DependencyName { get; }

        /// <summary>
        ///     Additional metadata for the dependency
        /// </summary>
        public Dictionary<string, object> AdditionalMetadata { get; protected set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Metadata which represents a dependency call. One can add a function to build metadata
    /// which will be logged based on the result
    /// </summary>
    /// <typeparam name="TResult">The type of the operation result</typeparam>
    public class DependencyMetadata<TResult> : DependencyMetadata
    {
        readonly Action<TResult, Dictionary<string, object>> addMetadataFunc;

        /// <summary>
        ///     Constructor to create an instance of <see cref="DependencyMetadata{TResult}"/>
        /// </summary>
        /// <param name="addMetadataFunc">builds metadata from the result</param>
        public DependencyMetadata(Action<TResult, Dictionary<string, object>>? addMetadataFunc = null)
        {
            this.addMetadataFunc = addMetadataFunc;
        }

        /// <summary>
        ///     Constructor to create an instance of <see cref="DependencyMetadata{TResult}"/>
        /// </summary>
        /// <param name="dependencyName">The name of the dependency call</param>
        /// <param name="addMetadataFunc">builds metadata from the result</param>
        public DependencyMetadata(string dependencyName, Action<TResult, Dictionary<string, object>>? addMetadataFunc = null)
            : base(dependencyName)
        {
            this.addMetadataFunc = addMetadataFunc;
        }

        internal void Complete(TResult result)
        {
            Complete();
            addMetadataFunc?.Invoke(result, AdditionalMetadata);
        }
    }

    /// <summary>
    ///     Coordinates calls to dependencies with other actions. E.g. logging
    /// </summary>
    public interface IDependencyCoordinator
    {
        /// <summary>
        ///     Method to execute a piece of work which returns a result of type <code>TResult</code>
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned after executing the work</typeparam>
        /// <param name="work">The work that this coordinator will manage</param>
        /// <param name="metadata">The metadata related to this dependency call, defined by <paramref name="work"/></param>
        /// <returns>an awaitable <see cref="Task{TResult}"/></returns>
        Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> work, DependencyMetadata<TResult> metadata);

        /// <summary>
        ///      Method to execute a piece of work which returns no result
        /// </summary>
        /// <param name="work">The work that this coordinator will manage</param>
        /// <param name="metadata">The metadata related to this dependency call, defined by <paramref name="work"/></param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        Task ExecuteAsync(Func<Task> work, DependencyMetadata metadata);
    }

    /// <summary>
    ///     An implementation of <see cref="IDependencyCoordinator"/> which will execute and log a dependency call. Any exceptions will be logged before being rethrown
    /// </summary>
    public class DependencyCoordinator : IDependencyCoordinator
    {
        /// <summary>
        ///     Constructs an instance of <see cref="DependencyCoordinator"/>
        /// </summary>
        /// <param name="log">The logger to use to log the dependency details</param>
        public DependencyCoordinator()
        {
        }

        void Write(DependencyMetadata metadata)
        {
        }

        /// <summary>
        ///      The method used to perform the logging of the dependency call
        /// </summary>
        /// <typeparam name="TResult">The type of the result to be returned asynchronously</typeparam>
        /// <param name="work">The work that this coordinator will manage</param>
        /// <param name="metadata">The metadata related to this dependency call</param>
        /// <returns>an awaitable <see cref="Task{TResult}"/></returns>
        public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> work, DependencyMetadata<TResult> metadata)
        {
            try
            {
                metadata.Start();
                var result = await work();
                metadata.Complete(result);

                return result;
            }
            catch (Exception ex)
            {
                metadata.CompleteWithException(ex);
                throw;
            }
            finally
            {
                Write(metadata);
            }
        }

        /// <summary>
        ///      The method used to perform the logging of the dependency call
        /// </summary>
        /// <param name="work">The work that this coordinator will manage</param>
        /// <param name="metadata">The metadata related to this dependency call</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        public async Task ExecuteAsync(Func<Task> work, DependencyMetadata metadata)
        {
            try
            {
                metadata.Start();
                await work();
                metadata.Complete();
            }
            catch (Exception ex)
            {
                metadata.CompleteWithException(ex);
                throw;
            }
            finally
            {
                Write(metadata);
            }
        }
    }

    /// <summary>
    ///     An implementation of <see cref="IOperationMetadata"/> which adds core functionality for tracking operations
    /// </summary>
    public class OperationMetadata 
    {
        /// <summary>
        ///     Starts tracking the operation from a timing perspective
        /// </summary>
        internal void Start()
        {
            StartTicks = Stopwatch.GetTimestamp();
        }

        /// <summary>
        ///     Completes the operation as successful and ends it from a timing perspective
        /// </summary>
        internal void Complete()
        {
            Duration = GetDuration();
            Success = true;
        }

        /// <summary>
        ///     Completes the operation as unsuccessful and ends it from a timing perspective whilst keeping the passed in <see cref="System.Exception"></see>
        /// </summary>
        /// <param name="ex"></param>
        internal void CompleteWithException(Exception ex)
        {
            Duration = GetDuration();
            Exception = ex;
            Success = false;
        }

        long GetDuration()
        {
            var currentTimestamp = Stopwatch.GetTimestamp();
            return (currentTimestamp - StartTicks) * 1000 / Stopwatch.Frequency;
        }

        /// <summary>
        ///     Gets the duration of the operation in milliseconds
        /// </summary>
        public long Duration { get; private set; }

        /// <summary>
        ///     Gets the start time of the operation, in Ticks
        /// </summary>
        public long StartTicks { get; private set; }

        /// <summary>
        ///     Gets the <see cref="System.Exception"/> thrown by the operation, in the case of a failure. <code>null</code> otherwise.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        ///     Gets the success status of the operation
        /// </summary>
        public bool Success { get; private set; }
    }
}