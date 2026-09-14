using AssassinBullet.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AssassinBullet.ViewModels;

public sealed partial class KeyCheckBlockViewModel : ConfigBlockViewModel
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private string _whereCheck = "Body";
    [ObservableProperty] private string _successText = "";
    [ObservableProperty] private string _failureText = "";
    [ObservableProperty] private string _retryText = "";

    public KeyCheckBlockViewModel() : base("KeyCheck") { }
    public string[] Sources { get; } = ["Header", "Body"];
    public override string TypeTitle => "KeyCheck";
    public override string Description => string.Empty;
    public override string Summary => WhereCheck + " • Başarı / hata / retry kontrolü";
    public override KeyCheck ToBlock() => new()
    {
        BlockName = Name,
        WhereCheck = WhereCheck,
        SuccessMessage = ToMessages(SuccessText),
        FailedMessage = ToMessages(FailureText),
        RetryMessage = ToMessages(RetryText)
    };

    private static string[] ToMessages(string value) => string.IsNullOrWhiteSpace(value) ? [] : [value];
}
