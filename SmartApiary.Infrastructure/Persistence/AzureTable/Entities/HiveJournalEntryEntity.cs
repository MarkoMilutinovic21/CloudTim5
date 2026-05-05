namespace SmartApiary.Infrastructure.Persistence.AzureTable.Entities;

public class HiveJournalEntryEntity : BaseTableEntity
{
    public string HiveId { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string BottomBoardColor { get; set; } = string.Empty;
    public int HoneyFrames { get; set; }
    public double HoneyKg { get; set; }
    public int BroodFrames { get; set; }
    public bool QueenPresent { get; set; }
    public DateTime CreatedAt { get; set; }
}
