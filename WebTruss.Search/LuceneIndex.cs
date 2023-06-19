using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System.Text.RegularExpressions;

namespace WebTruss.Search
{
    public class LuceneIndex<T> where T : class
    {
        private readonly Directory directory;

        private readonly LuceneIndexConfig config;

        public LuceneIndex(LuceneIndexConfig config)
        {
            this.config = config;
            directory = FSDirectory.Open(new System.IO.DirectoryInfo(config.Path));
        }

        /// <summary>
        /// Add/Update a document to the index.
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="Exception"></exception>
        public void Put(T data)
        {
            var document = new Document();
            var keyProperty = typeof(T).GetProperties().Where(x => x.Name == config.KeyPropertyName).FirstOrDefault();
            if (keyProperty == null)
            {
                throw new Exception("Supplied key property not found.");
            }

            document.Add(new Field(keyProperty.Name, keyProperty.GetValue(data)!.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            foreach (var property in typeof(T)
                .GetProperties()
                .Where(a => a.Name != config.KeyPropertyName && config.PropertyConfigs.Select(a=>a.Name).Contains(a.Name))
                .ToList())
            {
                var propertyConfig = config.PropertyConfigs.Where(a => a.Name == property.Name).FirstOrDefault();
                string value = string.Empty;
                if (property.GetValue(data) != null)
                {
                    value = property!.GetValue(data)!.ToString()!;
                }

                if (propertyConfig == null)
                {
                    document.Add(new Field(property.Name, value, Field.Store.YES, Field.Index.NO));
                }
                else
                {
                    document.Add(new Field(property.Name, value, propertyConfig.Store, propertyConfig.Index));
                }
            }

            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(1000)))
            {
                writer.AddDocument(document);
                writer.Optimize();
                writer.Flush(true, true, true);
            }
        }


        /// <summary>
        /// Deletes the document with the key from the index.
        /// </summary>
        /// <param name="key"></param>
        public void Delete(string key)
        {
            var idTerm = new Term(config.KeyPropertyName, key);
            using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
            using (var writer = new IndexWriter(directory, analyzer, new IndexWriter.MaxFieldLength(1000)))
            {
                try
                {
                    writer.DeleteDocuments(idTerm);
                }
                catch (OutOfMemoryException e)
                {
                    writer.Dispose();
                    throw e;
                }
                catch
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Performs search based on SearchConfig
        /// </summary>
        /// <param name="searchConfig"></param>
        /// <returns></returns>
        public List<T> Search(SearchConfig searchConfig)
        {
            var results = new List<T>();

            var rawQuery = FixQuery(searchConfig.Query) + (searchConfig.Fuzzy ? "~" : string.Empty);
            using (var reader = IndexReader.Open(directory, true))
            using (var searcher = new IndexSearcher(reader))
            {
                using (Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                {
                    var queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, searchConfig.TargetProperty, analyzer);
                    var query = queryParser.Parse(rawQuery);
                    var collector = TopScoreDocCollector.Create(searchConfig.Count, true);
                    searcher.Search(query, collector);
                    var matches = collector.TopDocs().ScoreDocs;
                    foreach (var match in matches)
                    {
                        var doc = searcher.Doc(match.Doc);
                        var resultItem = (T)Activator.CreateInstance(typeof(T))!;
                        foreach (var field in doc.GetFields())
                        {
                            var property = typeof(T).GetProperties().Where(a=>a.Name == field.Name).FirstOrDefault();
                            if(property == null)
                            {
                                continue;
                            }
                            if (property.PropertyType == typeof(Guid))
                            {
                                property.SetValue(resultItem, Guid.Parse(field.StringValue));
                            }
                            else
                            {
                                property.SetValue(resultItem, Convert.ChangeType(field.StringValue, property.PropertyType));
                            }
                        }
                        results.Add(resultItem);
                    }
                }
            }
            return results;
        }

        private string FixQuery(string query)
        {
            query = Regex.Replace(query, "[!-\\/:-@[-`{-~]", "");
            return query.TrimEnd();
        }
    }
}
