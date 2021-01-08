using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Menus
{
    /// <inheritdoc cref="ISubmenuBuilder" />
    internal class ConcreteSubmenuBuilder : ConcreteMenuEntryBase, ISubmenuBuilder, ISubmenu
    {
        /// <summary>
        /// The internal entries.
        /// </summary>
        private readonly List<ConcreteMenuEntryBuilder> entries;

        /// <summary>
        /// Whether this entry is finalized.
        /// </summary>
        private bool finalized;

        /// <summary>
        /// Instantiate the <see cref="ISubmenuBuilder"/>.
        /// </summary>
        public ConcreteSubmenuBuilder()
        {
            entries = new List<ConcreteMenuEntryBuilder>();
        }

        /// <inheritdoc />
        public IMenuEntry this[int index] => entries[index];

        /// <inheritdoc />
        public override string[] ToCheck => Array.Empty<string>();

        /// <inheritdoc />
        public int Count => entries.Count;

        /// <inheritdoc />
        public override bool Finalized => finalized;

        /// <inheritdoc />
        public IEnumerator<IMenuEntry> GetEnumerator() => entries.GetEnumerator();

        /// <inheritdoc />
        public override void Contribute()
        {
            if (finalized) return;
            finalized = true;
            base.Contribute();
            entries.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            entries.ForEach(a => a.Contribute());
        }

        /// <inheritdoc />
        public IMenuEntryBuilder HasEntry(int priority)
        {
            if (finalized) throw new InvalidOperationException();
            var entry = new ConcreteMenuEntryBuilder { Priority = priority };
            entries.Add(entry);
            return entry;
        }
    }
}
