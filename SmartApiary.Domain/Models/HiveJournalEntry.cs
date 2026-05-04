namespace SmartApiary.Domain.Models;

using SmartApiary.Domain.Common;

public class HiveJournalEntry : AggregateRoot
{
    public Guid HiveId { get; private set; }
    public DateTime EntryDate { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string BottomBoardColor { get; private set; } = string.Empty;
    public int HoneyFrames { get; private set; }
    public double HoneyKg { get; private set; }
    public int BroodFrames { get; private set; }
    public bool QueenPresent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private HiveJournalEntry() { }

    public static HiveJournalEntry Create(
        Guid hiveId,
        DateTime entryDate,
        string title,
        string content,
        string bottomBoardColor,
        int honeyFrames,
        double honeyKg,
        int broodFrames,
        bool queenPresent)
    {
        return new HiveJournalEntry
        {
            HiveId = hiveId,
            EntryDate = entryDate,
            Title = title,
            Content = content,
            BottomBoardColor = bottomBoardColor,
            HoneyFrames = honeyFrames,
            HoneyKg = honeyKg,
            BroodFrames = broodFrames,
            QueenPresent = queenPresent,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static HiveJournalEntry Rehydrate(
        Guid id,
        Guid hiveId,
        DateTime entryDate,
        string title,
        string content,
        string bottomBoardColor,
        int honeyFrames,
        double honeyKg,
        int broodFrames,
        bool queenPresent,
        DateTime createdAt)
    {
        return new HiveJournalEntry
        {
            Id = id,
            HiveId = hiveId,
            EntryDate = entryDate,
            Title = title,
            Content = content,
            BottomBoardColor = bottomBoardColor,
            HoneyFrames = honeyFrames,
            HoneyKg = honeyKg,
            BroodFrames = broodFrames,
            QueenPresent = queenPresent,
            CreatedAt = createdAt
        };
    }
}
