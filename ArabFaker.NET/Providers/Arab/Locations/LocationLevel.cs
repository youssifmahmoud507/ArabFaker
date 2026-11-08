using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Providers.Arab.Locations
{
    /// <summary>
    /// Represents the administrative level of a location.
    /// </summary>
    public enum LocationLevel
    {
        /// <summary>A governorate (top level).</summary>
        Governorate = 1,

        /// <summary>A district, inside a governorate.</summary>
        District = 2,

        /// <summary>A shiyakha, inside a district.</summary>
        Shiyakha = 3
    }

}
