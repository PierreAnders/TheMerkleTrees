namespace TheMerkleTrees.Domain.Models;

public class Shortcut
{
    public string Id { get; set; } = null!;
    public string Url { get; set; }
    public string Name { get; set; }
    public string Owner {get ; set; } = null!;
}   