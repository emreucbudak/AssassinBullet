using System;
using AssassinBullet.Models;
using CommunityToolkit.Mvvm.Input;

namespace AssassinBullet.ViewModels;

public partial class ConfigEditorViewModel
{
    private string? _lastManualResponse;
    public bool HasResponseCheckSelection => SelectedBlock is KeyCheckBlockViewModel;
    private string _responseCheckMessage =
        "Önce bir HTTP isteği gönder, sonra yanıtı kontrol et.";
    public string ResponseCheckMessage
    {
        get => _responseCheckMessage;
        set => SetProperty(ref _responseCheckMessage, value ?? "");
    }

    private bool CanCheckResponse() => HasResponseCheckSelection && CanEdit() &&
        SelectedBlock!.IsEnabled && IsHttpPreviewVisible &&
        _lastManualResponse is not null && !SendHttpCommand.IsRunning;

    [RelayCommand(CanExecute = nameof(CanCheckResponse))]
    private void CheckResponse()
    {
        if (!CanCheckResponse()) return;
        var block = (KeyCheckBlockViewModel)SelectedBlock!;
        var successText = block.SuccessText;
        var failureText = block.FailureText;
        var retryText = block.RetryText;
        var response = block.WhereCheck == "Header" ? HttpResponseHeaders : _lastManualResponse!;
        var success = !string.IsNullOrWhiteSpace(successText) &&
                      response.Contains(successText, StringComparison.OrdinalIgnoreCase);
        var failure = !string.IsNullOrWhiteSpace(failureText) &&
                      response.Contains(failureText, StringComparison.OrdinalIgnoreCase);
        var retry = !string.IsNullOrWhiteSpace(retryText) &&
                    response.Contains(retryText, StringComparison.OrdinalIgnoreCase);
        ResponseCheckMessage = (success, failure, retry) switch
        {
            (true, false, false) => "Başarılı • Başarı metni bulundu.",
            (false, true, false) => "Başarısız • Hata metni bulundu.",
            (false, false, true) => "Retry • Yeniden deneme metni bulundu.",
            (false, false, false) => "Belirsiz • Hiçbir metin bulunamadı.",
            _ => "Belirsiz • Birden fazla sonuç metni birlikte bulundu."
        };
    }

    private void InvalidateResponseCheck()
    {
        ResponseCheckMessage = _lastManualResponse is null
            ? "Önce bir HTTP isteği gönder."
            : "Hazır • Son HTTP yanıtını kontrol etmek için düğmeye bas.";
        CheckResponseCommand.NotifyCanExecuteChanged();
    }

    private void SetManualResponse(string? response)
    {
        _lastManualResponse = response;
        InvalidateResponseCheck();
    }
}

