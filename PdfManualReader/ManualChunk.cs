namespace PdfManualReader
{
    public class ManualChunk
    {
        public int ChunkId { get; set; }
        public int ProductId { get; set; }
        public int PageNumber { get; set; }
        public required string Text { get; set; }
        public required byte[] Embedding { get; set; }
    }

}

