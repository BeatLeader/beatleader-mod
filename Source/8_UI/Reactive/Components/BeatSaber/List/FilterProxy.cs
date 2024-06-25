using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatLeader.UI.Reactive.Components {
    internal class FilterProxy<T> : IListFilter<T> {
        #region Proxy
        
        public FilterProxy() {
            _filters = new(
                HandleFilterAdded,
                HandleFilterRemoved,
                HandleAllFiltersRemoved
            );
        }

        public ICollection<IListFilter<T>> Filters => _filters;

        private readonly ObservableSet<IListFilter<T>> _filters;

        #endregion

        #region Filter

        public event Action? FilterUpdatedEvent;

        public bool Matches(T value) {
            return _filters.All(x => x.Matches(value));
        }

        #endregion

        #region Callbacks

        private void HandleFilterAdded(IListFilter<T> filter) {
            filter.FilterUpdatedEvent += HandleFilterUpdated;
        }

        private void HandleFilterRemoved(IListFilter<T> filter) {
            filter.FilterUpdatedEvent -= HandleFilterUpdated;
        }

        private void HandleAllFiltersRemoved(IEnumerable<IListFilter<T>> filters) {
            foreach (var filter in filters) {
                HandleFilterRemoved(filter);
            }
        }

        private void HandleFilterUpdated() {
            FilterUpdatedEvent?.Invoke();
        }

        #endregion
    }
}