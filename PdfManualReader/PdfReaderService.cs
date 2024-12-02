using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Microsoft.SemanticKernel.Text;
using SmartComponents.LocalEmbeddings.SemanticKernel;
using System.Runtime.InteropServices;
using System.Text.Json;
using PdfManualReader;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using System.Numerics.Tensors;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.ML.OnnxRuntime.Tensors;

public interface IPdfReaderService
{
    Task ReadPdf(string filePath);
}

public class PdfReaderService : IPdfReaderService
{
    public async Task ReadPdf(string filePath)
    {
        Console.WriteLine("Ingesting manuals...");


        var sourceDir = "Manuals";

        var outputDir = "C:\\ManualChunks";

        // Prepare
        var manualsSourceDir = Path.Combine(sourceDir, "manuals", "pdf");
        using var embeddingGenerator = new LocalTextEmbeddingGenerationService();
        var chunks = new List<ManualChunk>();
        var paragraphIndex = 0;

        // Loop over each PDF file
        foreach (var file in Directory.GetFiles(manualsSourceDir, "*.pdf"))
        {
            Console.WriteLine($"Generating chunks for {file}...");
            var docId = int.Parse(Path.GetFileNameWithoutExtension(file));

            // Loop over each page in it
            using var pdf = PdfDocument.Open(file);
            foreach (var page in pdf.GetPages())
            {
                // [1] Parse (PDF page -> string)
                var pageText = GetPageText(page);

                // [2] Chunk (split into shorter strings on natural boundaries)
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                List<string> paragraphs = TextChunker.SplitPlainTextParagraphs([pageText], 200);
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

                // [3] Embed (map into semantic space)
                var paragraphsWithEmbeddings = paragraphs.Zip(await embeddingGenerator.GenerateEmbeddingsAsync(paragraphs));

                // [4] Save
                chunks.AddRange(paragraphsWithEmbeddings.Select(p => new ManualChunk
                {
                    ProductId = docId,
                    PageNumber = page.Number,
                    ChunkId = ++paragraphIndex,
                    Text = p.First,
                    Embedding = MemoryMarshal.AsBytes(p.Second.Span).ToArray()
                }));

                string phase = "Why Apple experience unique and cohesive";

                var searchWord = await embeddingGenerator.GenerateEmbeddingAsync(phase);

                //var similarity = TensorPrimitives.CosineSimilarity(searchWord2, searchWord);

                var closest =
                    from p in paragraphsWithEmbeddings
                    let similarity = TensorPrimitives.CosineSimilarity(
                        p.Second.Span, searchWord.Span)
                    orderby similarity descending
                    select new
                    {
                        Text = p.First,
                        Similarity = similarity
                    };

                Console.WriteLine($"\n Search Phase :{phase} \n");

                foreach (var c in closest.Take(1))
                {
                    Console.WriteLine($"________________({c.Similarity}): \n {c.Text}");
                }

            }
        }

        var outputOptions = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(Path.Combine(outputDir, "manual-chunks.json"), JsonSerializer.Serialize(chunks, outputOptions));
        Console.WriteLine($"Wrote {chunks.Count} manual chunks");
    }

    private static string GetPageText(Page pdfPage)
    {
        var letters = pdfPage.Letters;
        var words = NearestNeighbourWordExtractor.Instance.GetWords(letters);
        var textBlocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
        return string.Join(Environment.NewLine + Environment.NewLine,
            textBlocks.Select(t => t.Text.ReplaceLineEndings(" ")));
    }

}
