using System.Text.RegularExpressions;

namespace DotNetAI.Core.Utils;

/// <summary>
/// Fast token estimation utility for budget calculations and prompt truncation.
/// </summary>
public static partial class TokenEstimator
{
    // Rule of thumb for English/Code: approx 4 characters per token or ~0.75 words per token.
    private const double CharactersPerToken = 3.8;

    /// <summary>
    /// Estimates token count based on string length and word structure.
    /// </summary>
    public static int EstimateTokenCount(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        int charCount = text.Length;
        int wordCount = CountWords(text);

        // Hybrid approximation
        int byChars = (int)Math.Ceiling(charCount / CharactersPerToken);
        int byWords = (int)Math.Ceiling(wordCount * 1.33);

        return Math.Max(1, (byChars + byWords) / 2);
    }

    /// <summary>
    /// Alias for EstimateTokenCount.
    /// </summary>
    public static int EstimateTokens(string? text) => EstimateTokenCount(text);

    private static int CountWords(string text)
    {
        int count = 0;
        bool inWord = false;

        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i]))
            {
                inWord = false;
            }
            else if (!inWord)
            {
                inWord = true;
                count++;
            }
        }

        return count;
    }
}
