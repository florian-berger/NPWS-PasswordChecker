using System.ComponentModel;
using System.Windows;

namespace PasswordChecker.Resources
{
    /// <summary>
    ///     <see cref="ResourceDictionary" /> providing the possibility to cache resource dict
    /// </summary>
    public class CachedResourceDictionary : ResourceDictionary
    {
        /// <summary>
        ///     Internal cache of loaded dictionaries 
        /// </summary>
        public static Dictionary<Uri, ResourceDictionary> SharedDictionaries = [];

        /// <summary>
        ///     Local member of the source uri
        /// </summary>
        private Uri _sourceUri;

        /// <summary>
        ///     Gets or sets the uniform resource identifier (URI) to load resources from.
        /// </summary>
        public new Uri Source
        {
            get
            {
                if (IsInDesignMode)
                {
                    return base.Source;
                }

                return _sourceUri;
            }
            set
            {
                if (IsInDesignMode)
                {
                    try
                    {
                        _sourceUri = new Uri(value.OriginalString);
                    }
                    catch
                    {
                        // do nothing
                    }

                    return;
                }

                try
                {
                    _sourceUri = new Uri(value.OriginalString);
                }
                catch
                {
                    // do nothing
                }

                if (!SharedDictionaries.ContainsKey(value))
                {
                    // If the dictionary is not yet loaded, load it by setting
                    // the source of the base class

                    base.Source = value;

                    // add it to the cache
                    SharedDictionaries.Add(value, this);
                }
                else
                {
                    // If the dictionary is already loaded, get it from the cache
                    MergedDictionaries.Add(SharedDictionaries[value]);
                }
            }
        }

        private static bool IsInDesignMode =>
            (bool) DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty,
                typeof(DependencyObject)).Metadata.DefaultValue;
    }
}
