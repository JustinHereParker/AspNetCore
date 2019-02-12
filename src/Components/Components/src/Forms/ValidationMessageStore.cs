// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components.Forms
{
    /// <summary>
    /// Holds validation messages for an <see cref="EditContext"/>.
    /// </summary>
    public class ValidationMessageStore
    {
        private readonly EditContext _editContext;

        // TODO: Lazily instantiate
        private readonly Dictionary<FieldIdentifier, List<string>> _messages = new Dictionary<FieldIdentifier, List<string>>();
        private readonly Dictionary<FieldIdentifier, Task> _pendingTasks = new Dictionary<FieldIdentifier, Task>();

        /// <summary>
        /// Creates an instance of <see cref="ValidationMessageStore"/>.
        /// </summary>
        /// <param name="editContext">The <see cref="EditContext"/> with which this store should be associated.</param>
        public ValidationMessageStore(EditContext editContext)
        {
            _editContext = editContext ?? throw new ArgumentNullException(nameof(editContext));
        }

        /// <summary>
        /// Adds a validation message for the specified field.
        /// </summary>
        /// <param name="fieldIdentifier">The identifier for the field.</param>
        /// <param name="message">The validation message.</param>
        public void Add(FieldIdentifier fieldIdentifier, string message)
            => GetOrCreateMessagesListForField(fieldIdentifier).Add(message);

        /// <summary>
        /// Adds the messages from the specified collection for the specified field.
        /// </summary>
        /// <param name="fieldIdentifier">The identifier for the field.</param>
        /// <param name="messages">The validation messages to be added.</param>
        public void AddRange(FieldIdentifier fieldIdentifier, IEnumerable<string> messages)
            => GetOrCreateMessagesListForField(fieldIdentifier).AddRange(messages);

        public void AddTask(FieldIdentifier fieldIdentifier, Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (_pendingTasks.TryGetValue(fieldIdentifier, out var existingTask)
                && !existingTask.IsCompleted)
            {
                task = Task.WhenAll(existingTask, task);
            }

            // Add or overwrite
            _pendingTasks[fieldIdentifier] = task;

            AssociateWithField(fieldIdentifier);
        }

        /// <summary>
        /// Gets the validation messages within this <see cref="ValidationMessageStore"/> for the specified field.
        ///
        /// To get the validation messages across all validation message stores, use <see cref="EditContext.GetValidationMessages(FieldIdentifier)"/> instead
        /// </summary>
        /// <param name="fieldIdentifier">The identifier for the field.</param>
        /// <returns>The validation messages for the specified field within this <see cref="ValidationMessageStore"/>.</returns>
        public IEnumerable<string> this[FieldIdentifier fieldIdentifier]
        {
            get => _messages.TryGetValue(fieldIdentifier, out var messages) ? messages : Enumerable.Empty<string>();
        }

        public Task GetPendingTask(FieldIdentifier fieldIdentifier)
            => _pendingTasks.TryGetValue(fieldIdentifier, out var result) ? result : Task.CompletedTask;

        /// <summary>
        /// Removes all messages within this <see cref="ValidationMessageStore"/>.
        /// </summary>
        public void Clear()
        {
            foreach (var fieldIdentifier in _messages.Keys)
            {
                DissociateFromField(fieldIdentifier);
            }

            _messages.Clear();
            _pendingTasks.Clear();
        }

        /// <summary>
        /// Removes all messages within this <see cref="ValidationMessageStore"/> for the specified field.
        /// </summary>
        /// <param name="fieldIdentifier">The identifier for the field.</param>
        public void Clear(FieldIdentifier fieldIdentifier)
        {
            DissociateFromField(fieldIdentifier);
            _messages.Remove(fieldIdentifier);
            _pendingTasks.Remove(fieldIdentifier);
        }

        private List<string> GetOrCreateMessagesListForField(FieldIdentifier fieldIdentifier)
        {
            if (!_messages.TryGetValue(fieldIdentifier, out var messagesForField))
            {
                messagesForField = new List<string>();
                _messages.Add(fieldIdentifier, messagesForField);
                AssociateWithField(fieldIdentifier);
            }

            return messagesForField;
        }

        private void AssociateWithField(FieldIdentifier fieldIdentifier)
            => _editContext.GetFieldState(fieldIdentifier, ensureExists: true).AssociateWithValidationMessageStore(this);

        private void DissociateFromField(FieldIdentifier fieldIdentifier)
            => _editContext.GetFieldState(fieldIdentifier, ensureExists: false)?.DissociateFromValidationMessageStore(this);
    }
}
