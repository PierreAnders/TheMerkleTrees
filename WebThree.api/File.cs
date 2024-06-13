namespace WebThree.api;

public class File
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Hash { get; set; }
    public string Category { get; set; }
    public bool IsPublic { get; set; }
    public string Owner { get; set; }
    private string EncryptionKet { get; set; }
}
