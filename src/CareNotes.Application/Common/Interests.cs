namespace CareNotes.Application.Common;

public static class Interests
{
    /// Trims, lowercases and de-duplicates interests so grouping is consistent.
    public static List<string> Normalize(IEnumerable<string>? interests) =>
        (interests ?? [])
            .Select(i => i.Trim().ToLowerInvariant())
            .Where(i => i.Length > 0)
            .Distinct()
            .ToList();
}
