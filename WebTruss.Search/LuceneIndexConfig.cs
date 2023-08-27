using System.Reflection;
using Lucene.Net.Documents;

namespace WebTruss.Search
{
    public class LuceneIndexConfig
    {
        public LuceneIndexConfig()
        {
            PropertyConfigs = new List<PropertyConfig>();
        }

        /// <summary>
        /// Path on disk where lucene will store data.
        /// </summary>
        public string Path { get; set; } = null!;

        /// <summary>
        /// Name of the property which has the unique key
        /// </summary>
        public string KeyPropertyName { get; set; } = null!;

        /// <summary>
        /// Select type of directory implemenation. Default = FSDirectory (Windows)
        /// </summary>
        public DirectoryImplementation DirectoryImplementation { get; set; } = DirectoryImplementation.FSDirectory;

        /// <summary>
        /// Except key, only properties whose propertyConfig has been provided will be stored/indexed.
        /// </summary>
        public List<PropertyConfig> PropertyConfigs { get; set; } = null!;

        public class PropertyConfig
        {
            public PropertyConfig(string name, bool index)
            {
                Name = name;
                Index = index;
            }

            /// <summary>
            /// Name of property
            /// </summary>
            public string Name { get; set; } = null!;

            /// <summary>
            /// True if you want to perform search on this field
            /// </summary>
            public bool Index { get; set; }
        }
    }
}
