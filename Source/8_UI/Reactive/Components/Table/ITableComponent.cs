using System.Collections.Generic;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// Abstraction for tables
    /// </summary>
    internal interface ITableComponent {
        void Refresh(bool clearSelection = true);

        void ScrollTo(int idx, bool animated = true);

        void Select(int idx);

        /// <summary>
        /// Clears selected cell with the specified index
        /// </summary>
        /// <param name="idx">Index of the cell. -1 for all cells</param>
        void ClearSelection(int idx = -1);
    }

    /// <summary>
    /// Abstraction for tables with value
    /// </summary>
    /// <typeparam name="TItem">Data type</typeparam>
    internal interface ITableComponent<TItem> : ITableComponent {
        IReadOnlyList<TItem> Items { get; }

        void ScrollTo(TItem item, bool animated = true);

        void Select(TItem item);

        void ClearSelection(TItem item);
    }

    /// <summary>
    /// Abstraction for modifiable tables with value
    /// </summary>
    /// <typeparam name="TItem">Data type</typeparam>
    internal interface IModifiableTableComponent<TItem> : ITableComponent<TItem> {
        new IList<TItem> Items { get; }
    }
}