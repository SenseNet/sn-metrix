namespace SnMetrix.Client.Plugins
{
    public class FlatContentImporterOptions
    {
        public int ContentCount { get; set; } = 10;
        public bool FillContent { get; set; } = false;
        public int DegreeOfParallelism { get; set; } = 1;
        public string Container { get; set; }
    }
}
