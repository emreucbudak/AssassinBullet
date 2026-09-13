using System;
using System.Collections.Generic;
using AssassinBullet.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AssassinBullet.ViewModels;

public sealed partial class HttpBlockViewModel : ConfigBlockViewModel
{
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private string _url = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private string _method = "GET";
    [ObservableProperty] private string _headers = "";
    [ObservableProperty] private string _body = "";
    [ObservableProperty, NotifyPropertyChangedFor(nameof(Summary))]
    private bool _useHomePageUrl;

    public HttpBlockViewModel() : base("HTTP İsteği") { }
    public string[] Methods { get; } = ["GET", "POST"];
    public override string TypeTitle => "HTTP İsteği";
    public override string Description => "Gönderilecek HTTP isteğini yapılandır.";
    public override string Summary => UseHomePageUrl ? Method + " • Ana sayfa URL'si"
        : string.IsNullOrWhiteSpace(Url) ? "URL henüz girilmedi" : Method + " " + Url;

    public override HttpRequest ToBlock()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in Headers.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = line.IndexOf(':');
            if (separator <= 0) continue;
            var key = line[..separator].Trim();
            if (key.Length > 0) headers[key] = line[(separator + 1)..].Trim();
        }
        return new HttpRequest(Url, Method, Body, headers,useHomePageUrl:UseHomePageUrl)
        {
            BlockName = Name,
            UseHomePageUrl = UseHomePageUrl
        };
    }
}

