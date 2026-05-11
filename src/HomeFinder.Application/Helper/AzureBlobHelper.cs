using System;
using System.Collections.Generic;
using System.Text;

namespace HomeFinder.Application.Helper
{
    internal static class AzureBlobHelper
    {
        /// <summary>
        /// Blob 操作を指数バックオフなしで簡易リトライする。
        /// </summary>
        internal static async Task<T> ExecuteBlobWithRetryAsync<T>(
            Func<CancellationToken, Task<T>> action,
            string operation,
            int retryCount = 3,
            CancellationToken cancellationToken = default)
        {
            Exception? lastException = null;
            for (var attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    return await action(cancellationToken);
                }
                catch (Exception ex) when (attempt < retryCount)
                {
                    lastException = ex;
                    await Task.Delay(20 * attempt, cancellationToken);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    break;
                }
            }

            throw lastException ?? new InvalidOperationException($"Blob {operation} failed.");
        }
    }
}
