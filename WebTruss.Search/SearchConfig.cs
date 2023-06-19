namespace WebTruss.Search
{
    public class SearchConfig
    {
        /// <summary>
        /// The search term.
        /// </summary>
        public string Query { get; set; } = null!;

        /// <summary>
        /// Set true to enable fuzzy search. 
        /// </summary>
        public bool Fuzzy { get; set; } = false;

        /// <summary>
        /// The property which the Query targets.
        /// </summary>
        public string TargetProperty { get; set; } = null!;

        /// <summary>
        /// Count of max elements that can be returned.
        /// </summary>
        public int Count { get; set; }
    }
}
