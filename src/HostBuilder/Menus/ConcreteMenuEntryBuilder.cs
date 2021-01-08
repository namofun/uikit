using System;

namespace Microsoft.AspNetCore.Mvc.Menus
{
    /// <inheritdoc cref="IMenuEntryBuilder" />
    internal class ConcreteMenuEntryBuilder : ConcreteMenuEntryBase, IMenuEntryBuilder, IMenuEntry
    {
        /// <inheritdoc />
        public override string[] ToCheck => Array.Empty<string>();

        /// <summary>
        /// Whether this entry is finalized.
        /// </summary>
        private bool finalized;

        /// <inheritdoc />
        public override void Contribute()
        {
            if (finalized) return;
            finalized = true;
            base.Contribute();
        }

        /// <inheritdoc />
        public override bool Finalized => finalized;
    }
}
