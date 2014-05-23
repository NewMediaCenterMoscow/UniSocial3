using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    /// <summary>
    /// Provides helper methods to enable cloud application code to invoke common, globally accessible functions.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/en-us/library/hh690942.aspx"/>
    public static class CloudQueueHelper
    {
        /// <summary>
        /// Verifies whether or not the specified message size can be accommodated in an Azure queue.
        /// </summary>
        /// <param name="size">The message size value to be inspected.</param>
        /// <returns>True if the specified size can be accommodated in an Azure queue, otherwise false.</returns>
        public static bool IsAllowedQueueMessageSize(long size)
        {
            return size >= 0 && size <= (CloudQueueMessage.MaxMessageSize - 1) / 4 * 3;
        }
    }
}
