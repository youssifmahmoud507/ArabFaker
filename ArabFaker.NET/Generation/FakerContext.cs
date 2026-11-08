using ArabFaker.NET.Providers.Arab.Locations;
using ArabFaker.NET.Providers.Arab.Names;
using ArabFaker.NET.Providers.Arab.Reference;
using ArabFaker.NET.Providers.Common.Dates;
using ArabFaker.NET.Providers.Common.Identifiers;
using ArabFaker.NET.Providers.Common.Internet;
using ArabFaker.NET.Providers.Common.Random;
using ArabFaker.NET.Providers.Common.Text;

namespace ArabFaker.NET.Generation
{
    /// <summary>
    /// Provides access to fake data generators
    /// during an object generation operation.
    /// </summary>
    public sealed class FakerContext
    {
        /// <summary>
        /// Gets the text generator.
        /// </summary>
        public TextGenerator Text { get; }

        /// <summary>
        /// Gets the name generator.
        /// </summary>
        public NameGenerator Names { get; }

        /// <summary>
        /// Gets the internet data generator.
        /// </summary>
        public InternetGenerator Internet { get; }

        /// <summary>
        /// Gets the date generator.
        /// </summary>
        public DatesGenerator Dates { get; }

        /// <summary>
        /// Gets the geographical locations generator.
        /// </summary>
        public LocationsGenerator Locations { get; }

        /// <summary>
        /// Gets the generator of values from closed lists such as courses, departments and job titles.
        /// </summary>
        public ReferenceGenerator Reference { get; }

        /// <summary>
        /// Gets the random data generator.
        /// </summary>
        public RandomDataGenerator RandomData { get; }

        /// <summary>
        /// Gets the identifier generator.
        /// </summary>
        public IdentifierGenerator Identifiers { get; }

        /// <summary>
        /// Gets the zero-based index of the item being generated within its definition.
        /// </summary>
        public ulong ItemIndex { get; }

        internal FakerContext(GenerationContext context)
        {
            Text = new TextGenerator(context.Text);
            Names = new NameGenerator(context.Names);
            Internet = new InternetGenerator(context.Internet);
            Dates = new DatesGenerator(context.Dates);
            Locations = new LocationsGenerator(context.Locations);
            Reference = new ReferenceGenerator(context.Reference);
            RandomData = new RandomDataGenerator(context.Random);
            Identifiers = new IdentifierGenerator(context.Ids);
            ItemIndex = context.ItemIndex;
        }
    }
}
