using AssassinBullet.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AssassinBullet.ViewModels;

public sealed partial class VariableBlockViewModel : ConfigBlockViewModel
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private string _variableName = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private string _variableValue = "";

    public VariableBlockViewModel() : base("Değişken") { }
    public override string TypeTitle => "Değişken";
    public override string Description => string.Empty;
    public override string Summary => string.IsNullOrWhiteSpace(VariableName)
        ? "Değişken adı girilmedi" : VariableName + " = " + VariableValue;
    public override Variable ToBlock() => new(VariableName, VariableValue) { BlockName = Name };
}
