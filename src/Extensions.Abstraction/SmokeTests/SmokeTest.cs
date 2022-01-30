using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Diagnostics.SmokeTests
{
    /// <summary>
    /// The smoke test result executor.
    /// </summary>
    /// <typeparam name="TResult">The type of smoke test result.</typeparam>
    public interface ISmokeTest<TResult>
    {
        /// <summary>
        /// Gets the result of smoke test.
        /// </summary>
        /// <returns>The task for smoke test result.</returns>
        Task<TResult> GetAsync();
    }

    /// <summary>
    /// The base class for all smoke test.
    /// </summary>
    /// <typeparam name="TResult">The type of smoke test result.</typeparam>
    public abstract class SmokeTestBase<TResult> : ISmokeTest<TResult>
    {
        /// <summary>
        /// Gets the result of smoke test.
        /// </summary>
        /// <returns>The result for smoke test.</returns>
        protected virtual TResult Get()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the result of smoke test.
        /// </summary>
        /// <returns>The task for smoke test result.</returns>
        protected virtual Task<TResult> GetAsync()
        {
            return Task.FromResult(Get());
        }

        /// <inheritdoc />
        Task<TResult> ISmokeTest<TResult>.GetAsync()
        {
            return GetAsync();
        }
    }
}
