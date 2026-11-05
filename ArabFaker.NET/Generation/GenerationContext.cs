using ArabFaker.NET.Generation.Randomization;
using ArabFaker.NET.Providers.Arab.Locations;
using ArabFaker.NET.Providers.Arab.Names;
using ArabFaker.NET.Providers.Common.Dates;
using ArabFaker.NET.Providers.Common.Internet;
using ArabFaker.NET.Providers.Common.Text;

namespace ArabFaker.NET.Generation
{
    internal sealed class GenerationContext
    {
        private readonly RandomStreamFactory _streamFactory;

        private readonly string _scopeKey;

        private readonly ulong _itemIndex;

        internal RandomGenerator Random { get; }

        internal IdGenerator Ids { get; }
        internal NamesProvider Names { get; }
        internal InternetProvider Internet { get; }
        internal DatesProvider Dates { get; }
        internal LocationsProvider Locations { get; }
        internal TextProvider Text { get; }

        internal ulong ItemIndex => _itemIndex;

        internal GenerationContext(RandomStreamFactory streamFactory, string scopeKey, ulong itemIndex)
        {
            _streamFactory = streamFactory;

            _scopeKey = scopeKey;

            _itemIndex = itemIndex;

            Random = new RandomGenerator(_streamFactory.Create($"{_scopeKey}/random", _itemIndex));
            Ids = new IdGenerator(_streamFactory.Create($"{_scopeKey}/ids", _itemIndex));
            Names = new NamesProvider(new RandomGenerator(_streamFactory.Create($"{_scopeKey}/names", _itemIndex)));
            Internet = new InternetProvider(new RandomGenerator(_streamFactory.Create($"{_scopeKey}/internet", _itemIndex)));
            Dates = new DatesProvider(new RandomGenerator(_streamFactory.Create($"{_scopeKey}/dates", _itemIndex)));
            Locations = new LocationsProvider(new RandomGenerator(_streamFactory.Create($"{_scopeKey}/locations", _itemIndex)));
            Text = new TextProvider(new RandomGenerator(_streamFactory.Create($"{_scopeKey}/text", _itemIndex)));
        }

        internal GenerationContext CreateChild(string childScopeKey)
        {
            string childKey = string.Concat(_scopeKey, "/", childScopeKey);

            return new GenerationContext(_streamFactory, childKey, _itemIndex);
        }
    }
}
