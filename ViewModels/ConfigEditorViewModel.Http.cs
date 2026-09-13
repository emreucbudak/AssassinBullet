using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AssassinBullet.Models;
using AssassinBullet.Services;
using CommunityToolkit.Mvvm.Input;

namespace AssassinBullet.ViewModels;

public partial class ConfigEditorViewModel
{
    private int _httpPreviewGeneration;
    private readonly BlockExecutorService _httpRequestService = new();

    public bool HasHttpSelection => SelectedBlock is HttpBlockViewModel;
    private bool _isHttpPreviewVisible;
    private string _httpRequestLabel = "";
    private string _httpResponseSummary = "";
    private string _httpResponseBody = "";
    private string _httpResponseHeaders = "";

    // Explicit observable properties also work when the IDE's generator pass is incomplete.
    public bool IsHttpPreviewVisible
    {
        get => _isHttpPreviewVisible;
        set => SetProperty(ref _isHttpPreviewVisible, value);
    }
    public string HttpRequestLabel
    {
        get => _httpRequestLabel;
        set => SetProperty(ref _httpRequestLabel, value ?? "");
    }
    public string HttpResponseSummary
    {
        get => _httpResponseSummary;
        set => SetProperty(ref _httpResponseSummary, value ?? "");
    }
    public string HttpResponseBody
    {
        get => _httpResponseBody;
        set => SetProperty(ref _httpResponseBody, value ?? "");
    }
    public string HttpResponseHeaders
    {
        get => _httpResponseHeaders;
        set => SetProperty(ref _httpResponseHeaders, value ?? "");
    }

    private bool CanSendHttp() => HasHttpSelection && CanEdit() && SelectedBlock!.IsEnabled;

    private bool CanUseTargetUrl() => HasHttpSelection && CanEdit();

    [RelayCommand(CanExecute = nameof(CanUseTargetUrl))]
    private void UseTargetUrl()
    {
        if (!CanUseTargetUrl()) return;
        ((HttpBlockViewModel)SelectedBlock!).UseHomePageUrl = true;
        ShowStatus("Ana sayfa URL'si seçildi.");
    }

    [RelayCommand(CanExecute = nameof(CanSendHttp), IncludeCancelCommand = true)]
    private async Task SendHttpAsync(CancellationToken cancellationToken)
    {
        if (!CanSendHttp()) return;
        var block = (HttpBlockViewModel)SelectedBlock!;
        if (block.UseHomePageUrl && string.IsNullOrWhiteSpace(TargetUrl))
        {
            ShowStatus("Ana sayfa URL'sini kullanmak için ana sayfadaki URL alanını doldur.");
            return;
        }
        var request = CreateHttpRequest(block);
        var generation = ++_httpPreviewGeneration;
        SetManualResponse(null);
        IsHttpPreviewVisible = true;
        HttpRequestLabel = request.Method + " " + request.TargetUrl;
        HttpResponseSummary = "Gönderiliyor…";
        HttpResponseBody = "";
        HttpResponseHeaders = "";
        try
        {
            var response = await _httpRequestService.MakeHttpRequest(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (generation != _httpPreviewGeneration) return;
            HttpResponseSummary = "İstek tamamlandı";
            HttpResponseBody = response["Body"];
            HttpResponseHeaders = response["Headers"];
            SetManualResponse(HttpResponseBody);
            ShowStatus("HTTP isteği tamamlandı.");
        }
        catch (OperationCanceledException)
        {
            if (generation != _httpPreviewGeneration) return;
            HttpResponseSummary = "İstek iptal edildi";
            HttpResponseBody = "İstek durduruldu.";
            ShowStatus(HttpResponseSummary);
        }
        catch (Exception ex)
        {
            if (generation != _httpPreviewGeneration) return;
            HttpResponseSummary = "İstek tamamlanamadı";
            HttpResponseBody = ex.Message;
            ShowStatus("HTTP önizlemesindeki hata açıklamasını kontrol et.");
        }
    }

    private HttpRequest CreateHttpRequest(HttpBlockViewModel block)
    {
        var request = block.ToBlock();
        request.TargetUrl = request.UseHomePageUrl ? TargetUrl : request.TargetUrl.Trim();
        return request;
    }
    private bool CanCloseHttpPreview() => true;

    [RelayCommand(CanExecute = nameof(CanCloseHttpPreview))]
    private void CloseHttpPreview() => ResetHttpPreview();

    private void ResetHttpPreview()
    {
        ++_httpPreviewGeneration;
        SetManualResponse(null);
        if (SendHttpCommand.IsRunning) SendHttpCommand.Cancel();
        IsHttpPreviewVisible = false;
        HttpRequestLabel = "";
        HttpResponseSummary = "";
        HttpResponseBody = "";
        HttpResponseHeaders = "";
    }
}


