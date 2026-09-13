using AssassinBullet.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AssassinBullet.ViewModels;

public abstract partial class ConfigBlockViewModel : ViewModelBase
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isEnabled = true;

    protected ConfigBlockViewModel(string name) => _name = name;

    public abstract string TypeTitle { get; }
    public abstract string Description { get; }
    public abstract string Summary { get; }
    public abstract Blocks ToBlock();
}
