namespace WebTruss.Search
{
    public enum DirectoryImplementation
    {
        FSDirectory,//For windows
        NIOFSDirectory,//For linux
        SimpleFSDirectory,
        MMapDirectory//Uses ram as directory
    }
}
