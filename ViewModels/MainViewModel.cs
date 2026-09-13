using AssassinBullet.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AssassinBullet.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private int allCount;
    [ObservableProperty] private int successCount;
    [ObservableProperty] private int failureCount;
    [ObservableProperty] private int retryCount;

    public ObservableCollection<string> Results { get; } = new();
    public bool HasResults => Results.Count > 0;

    public void RefreshCounters()
    {
        AllCount = System.Threading.Volatile.Read(ref ScanCounter.AllCount);
        SuccessCount = System.Threading.Volatile.Read(ref ScanCounter.SuccessCount);
        FailureCount = System.Threading.Volatile.Read(ref ScanCounter.FailureCount);
        RetryCount = System.Threading.Volatile.Read(ref ScanCounter.RetryCount);
    }







    private string? _outputDirectory;
    public string? OutputDirectory
    {
        get => _outputDirectory;
        set => SetProperty(ref _outputDirectory, value);
    }

    private string? _hitSoundPath;
    public string? HitSoundPath
    {
        get => _hitSoundPath;
        set => SetProperty(ref _hitSoundPath, value);
    }

    public MainViewModel()
    {
        Results.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasResults));

    }

    private string _greeting = "Welcome to Avalonia!";
    public string Greeting
    {
        get => _greeting;
        set => SetProperty(ref _greeting, value ?? "");
    }
}
