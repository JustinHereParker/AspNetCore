// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components.Forms
{
    /// <summary>
    /// Holds information relating to a single validation process.
    /// </summary>
    public class ValidationContext
    {
        private List<Task> _pendingTasks;

        /// <summary>
        /// Associates an asynchronous operation with the validation process. The validation process will not
        /// be considered complete until this task, and any others also associated, have completed.
        ///
        /// In most cases this should not be used for long-running tasks such as network calls, because the
        /// user experience will be poor if users do not know whether their actions (such as form submissions)
        /// will begin or not. Consider modelling long-running tasks separately from validation so that they
        /// are not triggered as part of form submission.
        /// </summary>
        /// <param name="task">A <see cref="Task"/> representing the asynchronous operation.</param>
        public void AddPendingTask(Task task)
        {
            if (_pendingTasks == null)
            {
                _pendingTasks = new List<Task>();
            }

            _pendingTasks.Add(task);
        }

        /// <summary>
        /// Produces a <see cref="Task"/> representing all pending tasks for this <see cref="ValidationContext"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing all pending tasks for this <see cref="ValidationContext"/>.</returns>
        public Task CombinePendingTasks()
            => _pendingTasks == null ? Task.CompletedTask : Task.WhenAll(_pendingTasks);
    }
}
