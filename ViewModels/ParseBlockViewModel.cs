using AssassinBullet.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AssassinBullet.ViewModels;

public sealed partial class ParseBlockViewModel : ConfigBlockViewModel
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private string _source = "<Source>";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private string _left = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private string _right = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private string _variableName = "";

    public ParseBlockViewModel() : base("Parse") { }
    public override string TypeTitle => "Parse";
    public override string Description => "Sol ve sağ sınır arasındaki metni değişkene kaydet. HTTP gövdesi için kaynak: <Source>.";
    public override string Summary => Source + " • " + Left + " … " + Right + " → " + VariableName;
    public override Parse ToBlock() => new()
    {
        BlockName = Name,
        ParseBlockName = Source,
        LeftParse = Left,
        RightParse = Right,
        VariableName = VariableName
    };
}
