using AssassinBullet.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AssassinBullet.ViewModels;

public sealed partial class FileWriteBlockViewModel : ConfigBlockViewModel
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private string _fileContent = "";

    public FileWriteBlockViewModel() : base("FileWrite") { }
    public override string TypeTitle => "FileWrite";
    public override string Description => "Dosyaya yazılacak metni gir. Değişkenleri <isim> şeklinde kullanabilirsin.";
    public override string Summary => string.IsNullOrWhiteSpace(FileContent)
        ? "Yazılacak metin girilmedi" : FileContent;
    public override FileWrite ToBlock() => new()
    {
        BlockName = Name,
        FileContent = FileContent
    };
}
