using System;
using System.Collections.Generic;
using System.Text;

namespace ArabFaker.NET.Providers.Arab.Names
{
    internal readonly record struct NameEntry(string Arabic, string Latin)
    {
        internal bool TryGetValue(NameLanguage language, out string value)
        {
            value = language switch
            {
                NameLanguage.Arabic => Arabic,
                NameLanguage.Latin => Latin,
                _ => string.Empty
            };

            return value.Length > 0;
        }
    }

}
