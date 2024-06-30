namespace TheMerkleTrees.Client.Models;

public class FileStateContainer
{
    public List<File> Files { get; private set; } = new List<File>();
    public event Action OnChange;

    public void SetFiles(List<File> files)
    {
        Files = files;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}