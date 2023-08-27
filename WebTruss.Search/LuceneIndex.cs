using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Directory = Lucene.Net.Store.Directory;
using Lucene.Net.Search;
using System.Text.RegularExpressions;
using System;
using Lucene.Net.Util;
using System.Globalization;
using System.Text;

namespace WebTruss.Search
{
    public class LuceneIndex<T> where T : class
    {
        private Directory directory;

        private readonly LuceneIndexConfig config;

        private static LuceneVersion version = Lucene.Net.Util.LuceneVersion.LUCENE_48;

        public LuceneIndex(LuceneIndexConfig config)
        {
            this.config = config;
            ChangeDirectoryPath(config.Path);
        }

        public void ChangeDirectoryPath(string path)
        {
            switch (config.DirectoryImplementation)
            {
                case DirectoryImplementation.FSDirectory:
                    directory = FSDirectory.Open(new System.IO.DirectoryInfo(path));
                    break;

                case DirectoryImplementation.NIOFSDirectory:
                    directory = NIOFSDirectory.Open(new System.IO.DirectoryInfo(path));
                    break;

                case DirectoryImplementation.SimpleFSDirectory:
                    directory = SimpleFSDirectory.Open(new System.IO.DirectoryInfo(path));
                    break;

                case DirectoryImplementation.MMapDirectory:
                    directory = MMapDirectory.Open(new System.IO.DirectoryInfo(path));
                    break;
            }
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

            document.Add(new StringField(keyProperty.Name, keyProperty.GetValue(data)!.ToString(), Field.Store.YES));
            foreach (var property in typeof(T)
                .GetProperties()
                .Where(a => a.Name != config.KeyPropertyName && config.PropertyConfigs.Select(a => a.Name).Contains(a.Name))
                .ToList())
            {
                var propertyConfig = config.PropertyConfigs.Where(a => a.Name == property.Name).FirstOrDefault();
                string value = string.Empty;
                if (property.GetValue(data) != null)
                {
                    value = property!.GetValue(data)!.ToString()!;
                }

                if (propertyConfig != null)
                {
                    if (propertyConfig.Index)
                    {
                        document.Add(new TextField(property.Name, value, Field.Store.YES));
                    }
                    else
                    {
                        document.Add(new StringField(property.Name, value, Field.Store.YES));
                    }

                }
            }


            using (Analyzer analyzer = new StandardAnalyzer(version))
            {
                var config = new IndexWriterConfig(version, analyzer);
                using (var writer = new IndexWriter(directory, config))
                {
                    writer.AddDocument(document);
                    writer.Flush(true, true);
                }
            }
        }


        /// <summary>
        /// Deletes the document with the key from the index.
        /// </summary>
        /// <param name="key"></param>
        public void Delete(string key)
        {
            var idTerm = new Term(config.KeyPropertyName, key);
            using (Analyzer analyzer = new StandardAnalyzer(version))
            {
                var config = new IndexWriterConfig(version, analyzer);
                using (var writer = new IndexWriter(directory, config))
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
        }

        /// <summary>
        /// Performs search based on SearchConfig
        /// </summary>
        /// <param name="searchConfig"></param>
        /// <returns></returns>
        public List<T> Search(SearchConfig searchConfig)
        {
            var results = new List<T>();

            var rawQuery = FixQuery(searchConfig.Query);
            using (var reader = DirectoryReader.Open(directory))
            {
                var searcher = new IndexSearcher(reader);
                using (Analyzer analyzer = new StandardAnalyzer(version))
                {

                    var query = new BooleanQuery();
                    var parts = rawQuery.Split(' ');
                    foreach (var item in parts)
                    {
                        if (searchConfig.BeginWildCard || searchConfig.EndWildCard)
                        {
                            var wildcardQuery = (searchConfig.BeginWildCard ? "*" : string.Empty) + (searchConfig.EndWildCard ? item + '*' : item);
                            query.Add(new WildcardQuery(new Term(searchConfig.TargetProperty, wildcardQuery)), Occur.SHOULD);
                        }
                        if (searchConfig.Fuzzy)
                        {
                            query.Add(new FuzzyQuery(new Term(searchConfig.TargetProperty, item)), Occur.SHOULD);
                        }
                        else
                        {
                            query.Add(new TermQuery(new Term(searchConfig.TargetProperty, item)), Occur.SHOULD);
                        }
                    }

                    var matches = searcher.Search(query, searchConfig.Count).ScoreDocs;
                    foreach (var match in matches)
                    {
                        var doc = searcher.Doc(match.Doc);
                        var resultItem = (T)Activator.CreateInstance(typeof(T))!;
                        foreach (var field in doc.Fields)
                        {
                            var property = typeof(T).GetProperties().Where(a => a.Name == field.Name).FirstOrDefault();
                            if (property == null)
                            {
                                continue;
                            }
                            if (property.PropertyType == typeof(Guid))
                            {
                                property.SetValue(resultItem, Guid.Parse(field.GetStringValue()));
                            }
                            else
                            {
                                property.SetValue(resultItem, Convert.ChangeType(field.GetStringValue(), property.PropertyType));
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
            return CleanSearchTerm(query.TrimEnd());
        }

        private static string CleanSearchTerm(string query)
        {
            if (string.IsNullOrEmpty(query))
                return query;

            //replace double spaces and the asterix
            query = query.Trim().Replace("  ", " ").Replace("*", "");

            //replace special characters
            var decomposed = query.Normalize(NormalizationForm.FormD);
            var filtered = decomposed.Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);

            //return the new string
            return new string(filtered.ToArray()).ToLower();
        }
    }
}
