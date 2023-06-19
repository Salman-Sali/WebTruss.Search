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
        /// Except key, only properties whose propertyConfig has been provided will be stored/indexed.
        /// </summary>
        public List<PropertyConfig> PropertyConfigs { get; set; } = null!;

        public class PropertyConfig
        {
            public PropertyConfig(string name, Field.Store store, Field.Index index)
            {
                Name = name;
                Store = store;
                Index = index;
            }

            /// <summary>
            /// Name of property
            /// </summary>
            public string Name { get; set; } = null!;

            /// <summary>
            /// Similar to Included Columns concept of Ms-Sql. 
            /// When value is YES, data is stored along with the index and thus can be returned as part of the search result.
            /// When value is NO, the property value cannot be fetched along with search reuslt but can be fetched by using Get(ByKey) method.
            /// </summary>
            public Field.Store Store { get; set; }

            public Field.Index Index { get; set; }
        }
    }
}
