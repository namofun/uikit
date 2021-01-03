namespace System.Threading.Tasks
{
    /// <summary>
    /// The locker for asynchronous functions using <see cref="SemaphoreSlim"/>.
    /// </summary>
    public sealed class AsyncLock : IDisposable
    {
        /// <summary>
        /// The internal semaphore.
        /// </summary>
        private readonly SemaphoreSlim semaphore;

        /// <summary>
        /// The releaser.
        /// </summary>
        private readonly Releaser releaser;

        /// <summary>
        /// Instantiate a locker.
        /// </summary>
        public AsyncLock()
        {
            semaphore = new SemaphoreSlim(1);
            releaser = new Releaser(this);
        }

        /// <summary>
        /// Wait for the critical section.
        /// </summary>
        /// <returns>The disposer to release from critical section.</returns>
        public async Task<IDisposable> LockAsync()
        {
            await semaphore.WaitAsync();
            return releaser;
        }

        /// <summary>
        /// The releaser for locker.
        /// </summary>
        private class Releaser : IDisposable
        {
            /// <summary>
            /// The locker.
            /// </summary>
            private readonly AsyncLock @lock;

            /// <summary>
            /// Instantiate the <see cref="Releaser"/>.
            /// </summary>
            /// <param name="asyncLock">The locker.</param>
            public Releaser(AsyncLock asyncLock)
            {
                @lock = asyncLock;
            }

            /// <summary>
            /// Release the critical section.
            /// </summary>
            public void Dispose()
            {
                if (@lock != null)
                {
                    @lock.semaphore.Release();
                }
            }
        }

        /// <summary>
        /// Release the locker.
        /// </summary>
        public void Dispose()
        {
            semaphore.Dispose();
        }
    }
}
