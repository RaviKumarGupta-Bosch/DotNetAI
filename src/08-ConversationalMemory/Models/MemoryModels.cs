namespace DotNetAI.ConversationalMemory.Models;

public record MemoryItem(
    string Role,
    string Content,
    DateTime Timestamp,
    int EstimatedTokens
);

public record ExtractedEntity(
    string Key,
    string Value,
    string Category,
    DateTime LastUpdated
);

public record MemorySummary(
    string RunningSummary,
    int TotalTurnCount,
    int TotalTokensSaved
);

public record MemoryDiagnostics(
    int TotalMessagesStored,
    int WindowSize,
    int ActiveWindowMessages,
    string? RunningSummary,
    IReadOnlyDictionary<string, ExtractedEntity> KnownEntities
);
