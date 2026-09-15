using AssassinBullet.Channel;
using AssassinBullet.Models;
using AssassinBullet.Services;
using AssassinBullet.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;


namespace AssassinBullet.Views;

public partial class MainWindow : Window
{
    private static readonly IBrush ReadyBrush = Brush.Parse("#79E5A8");
    private bool IsComboListLoaded = false;
    private bool isConfigLoaded = false;
    private bool isProxyLoaded = false;
    private IStorageFile? selectedConfigFile;
    private IStorageFile? selectedProxyFile;
    private IStorageFile? selectedCombolistFile;
    WorkChannel workChannel;
    ChannelWriter writer;
    ChannelReader read;
    private CancellationTokenSource? _scanCts;
    private bool _statsClosed;
    public MainWindow()
    {
        InitializeComponent();
        ScanCounter.CountersUpdated += OnCountersUpdated;
        FileWriteService.ResultWritten += OnResultWritten;
        DataContextChanged += (_, _) => OnCountersUpdated();
        Closed += (_, _) =>
        {
            _statsClosed = true;
            ScanCounter.CountersUpdated -= OnCountersUpdated;
            FileWriteService.ResultWritten -= OnResultWritten;
        };
        OnCountersUpdated();
        SyncEditorTargetUrl();
        TargetAddressTextBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
                SyncEditorTargetUrl();
        };
    }

    private void OnResultWritten(string content)
    {
        if (_statsClosed) return;
        Dispatcher.UIThread.Post(() =>
        {
            if (!_statsClosed && DataContext is MainViewModel vm)
                vm.Results.Add(content);
        });
    }
    private void OnCountersUpdated()
    {
        if (_statsClosed) return;
        Dispatcher.UIThread.Post(() =>
        {
            if (!_statsClosed && DataContext is MainViewModel vm)
                vm.RefreshCounters();
        });
    }
    private void SyncEditorTargetUrl()
    {
        ConfigEditorPage.Editor.TargetUrl = TargetAddressTextBox.Text ?? "";
    }

    private void Navigate_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not string page ||
            page is not ("scan" or "config"))
        {
            return;
        }

        foreach (var menuButton in new[] { ScanMenuButton, ConfigMenuButton })
        {
            menuButton.Classes.Remove("selected");
        }
        button.Classes.Add("selected");

        
        ScanPage.IsVisible = page == "scan";
        ConfigEditorPage.IsVisible = page == "config";

    }

    private async void SelectConfig_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Config Dosyası Seç",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Config dosyaları")
                {
                    Patterns = ["*.json", "*.cfg", "*.ini", "*.txt"]
                },
                new FilePickerFileType("Tüm dosyalar")
                {
                    Patterns = ["*.*"]
                }
            ]
        });

        selectedConfigFile = files.FirstOrDefault();
        if (selectedConfigFile is null)
        {
            return;
        }

        ConfigFileText.Text = selectedConfigFile.Name;
        ConfigFileText.Foreground = ReadyBrush;
        ConfigDetailText.Text = selectedConfigFile.Name;
        ScanStatusText.Text = "Config dosyası seçildi.";
        isConfigLoaded = true;
    }

    private async void SelectProxy_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Proxy Dosyası Seç",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Proxy listeleri")
                {
                    Patterns = ["*.txt", "*.list", "*.csv"]
                },
                new FilePickerFileType("Tüm dosyalar")
                {
                    Patterns = ["*.*"]
                }
            ]
        });

        selectedProxyFile = files.FirstOrDefault();
        if (selectedProxyFile is null)
        {
            return;
        }

        ProxyFileText.Text = selectedProxyFile.Name;
        ProxyFileText.Foreground = ReadyBrush;
        ProxyDetailText.Text = selectedProxyFile.Name;
        ScanStatusText.Text = "Proxy listesi seçildi.";
        isProxyLoaded = true;
    }

    private async void SelectCombolist_Click(object? sender, RoutedEventArgs e)
    {
        if (!StorageProvider.CanOpen)
        {
            ScanStatusText.Text = "Bu ortamda dosya seçici kullanılamıyor.";
            return;
        }

        try
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Combolist Dosyası Seç",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Liste dosyaları")
                    {
                        Patterns = ["*.txt", "*.list", "*.csv"]
                    },
                    new FilePickerFileType("Tüm dosyalar")
                    {
                        Patterns = ["*.*"]
                    }
                ]
            });

            selectedCombolistFile = files.FirstOrDefault();
            if (selectedCombolistFile is null)
            {
                return;
            }

            // This UI step only displays the name; it does not read or process the file.
            CombolistFileText.Text = selectedCombolistFile.Name;
            CombolistFileText.Foreground = ReadyBrush;
            ToolTip.SetTip(CombolistFileText, selectedCombolistFile.Name);
            ScanStatusText.Text = "Combolist dosyası seçildi.";
            IsComboListLoaded = true;
        }
        catch (Exception)
        {
            ScanStatusText.Text = "Combolist dosyası seçilemedi. Tekrar deneyebilirsin.";
        }
    }

    private async void SelectOutputDirectory_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        if (!StorageProvider.CanPickFolder)
        {
            ScanStatusText.Text = "Bu ortamda klasör seçici kullanılamıyor.";
            return;
        }

        try
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Kayıt Yeri Seç",
                AllowMultiple = false
            });

            var selectedFolder = folders.FirstOrDefault();
            if (selectedFolder is null)
            {
                return;
            }

            var localPath = selectedFolder.TryGetLocalPath();
            if (string.IsNullOrWhiteSpace(localPath))
            {
                ScanStatusText.Text = "Lütfen bilgisayardaki bir kayıt klasörünü seç.";
                return;
            }

            // Keep the selection for this session; choosing a folder does not write files.
            viewModel.OutputDirectory = localPath;
            ScanStatusText.Text = "Kayıt klasörü seçildi.";
        }
        catch (Exception)
        {
            ScanStatusText.Text = "Kayıt klasörü seçilemedi. Tekrar deneyebilirsin.";
        }
    }

    private async void StartScan_Click(object? sender, RoutedEventArgs e)
    {
        var targetAddress = TargetAddressTextBox.Text?.Trim();
        var kuyruk = new Queue<string>();
        if (!string.IsNullOrWhiteSpace(targetAddress))
        {
            for(int i = 0; i< targetAddress.Length; i++)
            {
                var first = targetAddress.IndexOf("<",i);
                var last = targetAddress.IndexOf(">",i);
                if( last == -1)
                {
                    break;
                }

                if(first != -1 && last != -1 && last> first)
                {
                    kuyruk.Enqueue(targetAddress.Substring(first+1,last-first+-1));
                }
                i = last;

            }

        }

        if (selectedConfigFile is null || selectedCombolistFile is null ) 
        {
            ScanStatusText.Text = "Lütfen config ve combolist dosyalarını seç.";
            return;
        }
        if (string.IsNullOrEmpty(OutputDirectoryTextBox.Text))
        {
            ScanStatusText.Text = "Kayıt Yeri Seçilmedi!";
        }
        FileWriteSettings.FileName = DateOnly.FromDateTime(DateTime.Now) +$"{Random.Shared.Next(1,100)}"+ "scanresult.txt";
        FileWriteSettings.FileWriteLocation = OutputDirectoryTextBox.Text;
        ScanStateText.Text = "Çalışıyor";
        ScanStatusText.Text = "Tarama çalışıyor.";
        StartScanButton.IsVisible = false;
        StopScanButton.IsVisible = true;
        SelectCombolistButton.IsVisible = false;
        SelectProxyButton.IsVisible = false;
        SelectConfigButton.IsVisible = false;
        SelectOutputDirectoryButton.IsVisible = false;
        int botCount = (int)(BotCountInput.Value ?? 1m);
        workChannel = new WorkChannel(botCount);
        try
        {
            if(selectedProxyFile is not null)
            {
                await using var stream = await selectedProxyFile.OpenReadAsync();
                using var reader = new StreamReader(stream);
                string content = await reader.ReadToEndAsync();
                if (selectedProxyFile is not null)
                {
                    if (!string.IsNullOrEmpty(content))
                    {
                        ProxyStore.Proxies.Clear();
    
                        ProxyStore.Proxies.AddRange(
                            content
                                .Split(new[] { "\r\n", "\n" },
                                    StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim()));
                    }
                }
            }
             writer = new ChannelWriter(workChannel);
            var config = new ConfigLoadingService().Load(await File.ReadAllTextAsync(selectedConfigFile.TryGetLocalPath()));
            await using var strea = await selectedCombolistFile.OpenReadAsync();
            using var reade = new StreamReader(strea);
            read = new ChannelReader(workChannel);
            var taskBot =  read.CreateBot(botCount);
            string firstKey, lastKey;
            if (kuyruk.Count > 0)
            {
                firstKey = kuyruk.Dequeue();
                lastKey = kuyruk.Dequeue();
            }
            else
            {
                firstKey = "Param1";
                lastKey = "Param2";
            }
            _scanCts = new CancellationTokenSource();
            while (await reade.ReadLineAsync() is { } line)
            {
                var items = line.Split(":",StringSplitOptions.RemoveEmptyEntries);
                if (items.Length < 2)
                {
                    throw new Exception("Combolist Hatalı");
                }
                
                Dictionary<string,string> DynamicParameters = new Dictionary<string, string>();
                DynamicParameters.Add(firstKey, items[0]);
                DynamicParameters.Add(lastKey, items[1]);
    
                
                await writer.WriteToChannel(targetAddress, config, DynamicParameters,cancellationToken:_scanCts.Token);
            }
            workChannel.channel.Writer.TryComplete();
            await taskBot;
            var cancelled = _scanCts?.IsCancellationRequested == true;
            ScanStateText.Text = cancelled ? "Durduruldu" : "Tamamlandı";
            ScanStatusText.Text = cancelled ? "Tarama durduruldu." : "Tarama tamamlandı.";
        }
        catch (Exception ex)
        {
            workChannel.channel.Writer.TryComplete(ex);
            ScanStateText.Text = _scanCts?.IsCancellationRequested == true ? "Durduruldu" : "Hata";
            ScanStatusText.Text = _scanCts?.IsCancellationRequested == true
                ? "Tarama durduruldu." : "Tarama durduruldu: " + ex.Message;
        }
        StartScanButton.IsVisible = true;
        StopScanButton.IsVisible = false;
        SelectConfigButton.IsVisible = true;
        SelectProxyButton.IsVisible = true;
        SelectCombolistButton.IsVisible = true;
        SelectOutputDirectoryButton.IsVisible = true;
      

    }
    private void StopScan_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            
            workChannel.channel.Writer.TryComplete();
            _scanCts.Cancel();
            if (ResultsListBox.ItemsSource is System.Collections.IList results)
            {
                results.Clear();
            }
            ScanStateText.Text = "Durduruldu";
            ScanStatusText.Text = "Tarama durduruldu.";
            StopScanButton.IsVisible = false;
            StartScanButton.IsVisible = true;
            SelectConfigButton.IsVisible = true;
            SelectProxyButton.IsVisible = true;
            SelectCombolistButton.IsVisible = true;
            SelectOutputDirectoryButton.IsVisible = true;

            ScanCounter.Reset();
        }
        catch
        {
            ScanStatusText.Text = "Tarama durdurulamadı. Lütfen tekrar deneyin.";
        }



    }
}
