using System.ComponentModel;
using System.Linq;
using AssassinBullet.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AssassinBullet.ViewModels;

public partial class ConfigEditorViewModel : BlockEditorViewModelBase
{
    [ObservableProperty] private string _configName = "Yeni Config";
    [ObservableProperty] private string _status = "";
    [ObservableProperty] private bool _isDirty;
    private bool _loading;
    private string _targetUrl = "";

    public string TargetUrl
    {
        get => _targetUrl;
        set
        {
            if (SetProperty(ref _targetUrl, value?.Trim() ?? ""))
                OnPropertyChanged(nameof(TargetUrlDisplay));
        }
    }
    public string TargetUrlDisplay => TargetUrl.Length == 0 ? "Ana sayfadaki link alanını doldur." : TargetUrl;

    public ConfigEditorViewModel()
    {
        SendHttpCommand.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SendHttpCommand.IsRunning)) RefreshCommands();
        };
    }

    partial void OnConfigNameChanged(string value) { if (!_loading) IsDirty = true; }
    protected override void OnBlocksChanged() { if (!_loading) IsDirty = true; }
    protected override void OnSelectionChanged()
    {
        OnPropertyChanged(nameof(HasHttpSelection));
        OnPropertyChanged(nameof(HasResponseCheckSelection));
        InvalidateResponseCheck();
    }
    protected override void OnBlockChanged(ConfigBlockViewModel block, PropertyChangedEventArgs e)
    {
        if (!_loading) IsDirty = true;
        if (!ReferenceEquals(block, SelectedBlock)) return;
        RefreshCommands();
        if (HasResponseCheckSelection) InvalidateResponseCheck();
    }
    protected override void RefreshCommands()
    {
        base.RefreshCommands();
        SendHttpCommand.NotifyCanExecuteChanged();
        UseTargetUrlCommand.NotifyCanExecuteChanged();
        CloseHttpPreviewCommand.NotifyCanExecuteChanged();
        CheckResponseCommand.NotifyCanExecuteChanged();
    }
    public override void ShowStatus(string message) => Status = message;

    [RelayCommand]
    private void AddBlock(string? kindName)
    {
        if (Blocks.Count >= 200)
        {
            Status = "Bir config en fazla 200 blok içerebilir.";
            return;
        }
        ConfigBlockViewModel? block = kindName switch
        {
            "HttpRequest" => new HttpBlockViewModel(),
            "Variable" => new VariableBlockViewModel(),
            "KeyCheck" => new KeyCheckBlockViewModel(),
            "Parse" => new ParseBlockViewModel(),
            "FileWrite" => new FileWriteBlockViewModel(),
            _ => null
        };
        if (block is null) return;
        Blocks.Add(block);
        SelectedBlock = block;
    }

    public Config ToConfig() => new()
    {
        ConfigName = ConfigName.Trim(),
        Blocks = Blocks.Where(block => block.IsEnabled).Select(block => block.ToBlock()).ToList()
    };

    public void StartNewDocument()
    {
        ResetHttpPreview();
        _loading = true;
        try
        {
            SelectedBlock = null;
            Blocks.Clear();
            ConfigName = "Yeni Config";
            IsDirty = false;
        }
        finally { _loading = false; }
        Status = "Yeni config hazır.";
    }
}
