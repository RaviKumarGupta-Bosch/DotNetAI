using DotNetAI.RAG.Models;

namespace DotNetAI.RAG.Services;

public interface IDocumentChunker
{
    IReadOnlyList<DocumentChunk> Chunk(KnowledgeDocument document, int chunkSize = 300, int chunkOverlap = 50);
}

/// <summary>
/// Recursive text chunker that splits on paragraph boundaries, sentences, or word boundaries
/// to maintain semantic integrity within each chunk.
/// </summary>
public class RecursiveDocumentChunker : IDocumentChunker
{
    public IReadOnlyList<DocumentChunk> Chunk(KnowledgeDocument document, int chunkSize = 300, int chunkOverlap = 50)
    {
        var chunks = new List<DocumentChunk>();
        var text = document.Content.Trim();

        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        if (text.Length <= chunkSize)
        {
            chunks.Add(new DocumentChunk(
                ChunkId: $"{document.Id}_c0",
                DocumentId: document.Id,
                DocumentTitle: document.Title,
                Text: text,
                ChunkIndex: 0,
                StartOffset: 0,
                EndOffset: text.Length
            ));
            return chunks;
        }

        int start = 0;
        int chunkIdx = 0;

        while (start < text.Length)
        {
            int targetEnd = Math.Min(start + chunkSize, text.Length);

            // Find best split point (newline > period/question/exclamation > space)
            int splitPoint = targetEnd;
            if (targetEnd < text.Length)
            {
                int newlinePos = text.LastIndexOf('\n', targetEnd, Math.Min(chunkSize / 2, targetEnd - start));
                if (newlinePos > start + (chunkSize / 4))
                {
                    splitPoint = newlinePos;
                }
                else
                {
                    int sentencePos = text.LastIndexOfAny(new[] { '.', '?', '!' }, targetEnd, Math.Min(chunkSize / 2, targetEnd - start));
                    if (sentencePos > start + (chunkSize / 4))
                    {
                        splitPoint = sentencePos + 1;
                    }
                    else
                    {
                        int spacePos = text.LastIndexOf(' ', targetEnd, Math.Min(chunkSize / 3, targetEnd - start));
                        if (spacePos > start)
                        {
                            splitPoint = spacePos;
                        }
                    }
                }
            }

            string chunkText = text.Substring(start, splitPoint - start).Trim();
            if (!string.IsNullOrWhiteSpace(chunkText))
            {
                chunks.Add(new DocumentChunk(
                    ChunkId: $"{document.Id}_c{chunkIdx}",
                    DocumentId: document.Id,
                    DocumentTitle: document.Title,
                    Text: chunkText,
                    ChunkIndex: chunkIdx,
                    StartOffset: start,
                    EndOffset: splitPoint
                ));
                chunkIdx++;
            }

            if (splitPoint >= text.Length)
                break;

            start = Math.Max(splitPoint - chunkOverlap, start + 1);
        }

        return chunks;
    }
}
