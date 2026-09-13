using System;
using System.Threading.Tasks;
using AssassinBullet.Models;
using AssassinBullet.Services;
using AssassinBullet.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AssassinBullet.Views;

public partial class ConfigEditorView : UserControl
{
    public ConfigEditorViewModel Editor { get; } = new();
    private readonly ConfigCreationService _configCreationService = new();
    private bool _busy;
    private Config configEdit;
    public ConfigEditorView()
    {
        InitializeComponent();
        DataContext = Editor;
        DetachedFromVisualTree += (_, _) =>
        {
            if (Editor.SendHttpCommand.IsRunning) Editor.SendHttpCommand.Cancel();
        };
        configEdit = new Config();

    }

    private void New_Click(object? sender, RoutedEventArgs e) => Editor.StartNewDocument();

    private async void Save_Click(object? sender, RoutedEventArgs e)
    {
        await RunActionAsync(async () =>
        {
            configEdit = Editor.ToConfig();
            var path = await _configCreationService.CreateConfig(configEdit.ConfigName, configEdit.Blocks);
            Editor.ShowStatus("Config kaydedildi: " + path);
        });
    }
    private async Task RunActionAsync(Func<Task> action)
    {
        if (_busy) return;
        _busy = true;
        IsEnabled = false;
        try { await action(); }
        catch (Exception ex) { Editor.ShowStatus("İşlem tamamlanamadı: " + ex.Message); }
        finally { IsEnabled = true; _busy = false; }
    }
}


