using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Meditation.UI.Utilities
{
    public class FilterableCollectionView<TElement>
    {
        public Task<ImmutableArray<TElement>> View { get; private set; }
        private readonly Task<ImmutableArray<TElement>> _source;

        public FilterableCollectionView(Task<ImmutableArray<TElement>> source)
        {
            _source = source;
            View = source;
        }

        public void ApplyFilter(Func<TElement, bool> predicate)
            => View = _source.ContinueWith(task => task.Result.Where(predicate).ToImmutableArray());
    }
}
