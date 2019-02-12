// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components.Forms
{
    internal class FieldState
    {
        private readonly FieldIdentifier _fieldIdentifier;

        // We track which ValidationMessageStore instances have a nonempty set of messages for this field so that
        // we can quickly evaluate the list of messages for the field without having to query all stores. This is
        // relevant because each validation component may define its own message store, so there might be as many
        // stores are there are fields or UI elements.
        private HashSet<ValidationMessageStore> _validationMessageStores;

        public FieldState(FieldIdentifier fieldIdentifier)
        {
            _fieldIdentifier = fieldIdentifier;
        }

        public bool IsModified { get; set; }

        public IEnumerable<string> GetValidationMessages()
            => _validationMessageStores == null ? Enumerable.Empty<string>() : _validationMessageStores.SelectMany(store => store[_fieldIdentifier]);

        public Task GetPendingValidationTask()
        {
            if (_validationMessageStores != null)
            {
                List<Task> pendingTasks = null;

                foreach (var store in _validationMessageStores)
                {
                    var pendingTaskForStore = store.GetPendingTask(_fieldIdentifier);
                    if (!pendingTaskForStore.IsCompleted)
                    {
                        if (pendingTasks == null)
                        {
                            pendingTasks = new List<Task>();
                        }

                        pendingTasks.Add(pendingTaskForStore);
                    }
                }

                if (pendingTasks != null)
                {
                    return Task.WhenAll(pendingTasks);
                }
            }

            return Task.CompletedTask;
        }

        public void AssociateWithValidationMessageStore(ValidationMessageStore validationMessageStore)
        {
            if (_validationMessageStores == null)
            {
                _validationMessageStores = new HashSet<ValidationMessageStore>();
            }

            _validationMessageStores.Add(validationMessageStore);
        }

        public void DissociateFromValidationMessageStore(ValidationMessageStore validationMessageStore)
            => _validationMessageStores?.Remove(validationMessageStore);
    }
}
