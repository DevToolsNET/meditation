using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace Meditation.UI.Utilities
{
    public class FilterableObservableCollection<TElement>
    {
        public ObservableCollection<TElement> View { get; }
        private ImmutableArray<TElement> _allElements;
        private Func<TElement, bool> _activeFilter;

        public FilterableObservableCollection(IEnumerable<TElement> source)
        {
            _allElements = source.ToImmutableArray();
            _activeFilter = static _ => true;
            View = new ObservableCollection<TElement>(_allElements);
        }

        public void Add(TElement element)
        {
            _allElements = _allElements.Add(element);
            if (_activeFilter(element))
                View.Add(element);
        }

        public void ApplyFilter(Func<TElement, bool> predicate)
        {
            _activeFilter = predicate;

            View.Clear();
            foreach (var element in _allElements.Where(predicate))
                View.Add(element);
        }
    }
}
