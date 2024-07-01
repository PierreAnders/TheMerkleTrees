namespace TheMerkleTrees.Domain.Models;

public class File
{
    public string Id { get; set; } = null!;
    public string Hash { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public bool IsPublic { get; set; }
    public string Owner { get; set; } = null!;
    public string EncryptionKey { get; set; } = null!;
}