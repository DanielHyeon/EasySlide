using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Axicultus.Wpf.Captions;
using Axicultus.Wpf.Shell;
using Axicultus.Wpf.Rendering;
using Xunit;

namespace Axicultus.Wpf.Tests.Shell;

public sealed class CaptionIntegrationTests
{
    [Fact]
    public async Task BrowserReconnectsToLatestConfirmedCaptionAndHonorsBlackout()
    {
        var live = new LiveSessionService();
        using var captions = new CaptionSession(live.ForCaptions());
        using var server = new CaptionBroadcastServer(captions, 0);
        await server.StartAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(7));
        using (var http = new System.Net.Http.HttpClient())
        {
            using var page = await http.GetAsync(server.Address, timeout.Token);
            Assert.True(page.Headers.CacheControl?.NoStore); // OBS must load the current page after an update
            var html = await page.Content.ReadAsStringAsync(timeout.Token);
            Assert.Contains("location.reload()", html); // and reloads it when Axicultus comes back
            Assert.Contains("c.ScoreHeight", html);
        }
        var address = new UriBuilder(server.Address) { Scheme = "ws", Path = "/captions" }.Uri;
        captions.Publish("말씀 <&>");
        using var client = new ClientWebSocket();
        await client.ConnectAsync(address, timeout.Token);
        Assert.Equal("말씀 <&>", await Read(client));
        live.GoLive(new("camera", "Camera", "Media"), "test");
        live.HideOutput(true);
        Assert.Equal("", await Read(client));
        live.Restore();
        Assert.Equal("말씀 <&>", await Read(client));
        captions.SetVisible(false);
        using var second = new ClientWebSocket();
        await second.ConnectAsync(address, timeout.Token);
        Assert.Equal("", await Read(second));

        async Task<string> Read(ClientWebSocket ws)
        {
            byte[] buffer = new byte[2048];
            var result = await ws.ReceiveAsync(buffer.AsMemory(), timeout.Token);
            return JsonSerializer.Deserialize<PublishedCaption>(buffer.AsSpan(0,result.Count))!.Text;
        }
    }

    [Fact]
    public async Task DiskExportCacheHonorsDeckVersionByteLimitAndRejectsDamagedPng()
    {
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            var cache = new PowerPointDiskCache(dir.FullName);
            var key = new ThumbnailCacheKey(@"C:\찬양.pptx",ThumbnailSourceKind.PowerPointSlide,1,640,480,"render",1,100);
            byte[] png = [137,80,78,71,13,10,26,10,10,11,12,13];
            var slide = new PowerPointSlideSnapshot(key.SourcePath,1,5,640,360,png,"image/png",DateTimeOffset.UtcNow);
            await cache.StoreAsync(key,slide,1024);
            var hit = await cache.ReadAsync(key,1024);
            Assert.NotNull(hit);
            Assert.Equal(slide.ImageBytes,hit!.ImageBytes);
            Assert.Equal((640,480),hit.RenderBounds);
            Assert.Null(await cache.ReadAsync(key with { LastWriteUtcTicks = 2 },1024));
            var file = Assert.Single(dir.GetFiles("*.slide"));
            var bytes = await File.ReadAllBytesAsync(file.FullName);
            bytes[^1] ^= 1;
            await File.WriteAllBytesAsync(file.FullName,bytes);
            Assert.Null(await cache.ReadAsync(key,1024));
            cache.Trim(0);
            Assert.Empty(dir.GetFiles("*.slide"));
        }
        finally { dir.Delete(true); }
    }

    [Fact]
    public async Task DamagedLatestRecoversPreviousPlanAndDuplicateSongPositions()
    {
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            using var store = new CaptionRecoveryStore(dir.FullName, MainViewModelCaptionHost.IsRecoverableItem);
            var item = JsonSerializer.SerializeToElement(new WorshipListItemDto("song:7","찬양","Song",1,"가사",null,Sequence:"1 C 2 C"));
            var plan = new CaptionRecovery(1,1,"주일","미송출",null,1,"",new(),[],[item,item],1);
            await store.SaveAsync(plan);
            await store.SaveAsync(plan with { Revision = 2, Draft = "다음 초안" });
            Assert.Null(store.Read("금요"));
            var file = Assert.Single(dir.GetFiles("*.json"));
            await File.WriteAllTextAsync(file.FullName, "{");
            var saved = Assert.IsType<CaptionRecovery>(store.Read("주일"));
            Assert.Equal("미송출",saved.Draft);
            Assert.Equal(1,saved.SelectedIndex);
            Assert.Equal(2,saved.Items.Length);
            await store.SaveAsync(saved with { Draft = "복구 뒤 편집" });
            await File.WriteAllTextAsync(file.FullName, "{");
            Assert.Equal("미송출",store.Read("주일")!.Draft);
        }
        finally { dir.Delete(true); }
    }

    [Fact]
    public async Task InvalidRecoveryEntriesFallBackBeforeReplacingTheQueue()
    {
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            using var store = new CaptionRecoveryStore(dir.FullName, MainViewModelCaptionHost.IsRecoverableItem);
            var dto = new WorshipListItemDto("song:7", "찬양", "Song", 1, null, null);
            var item = JsonSerializer.SerializeToElement(dto);
            var good = new CaptionRecovery(1, 1, "주일", "이전 초안", null, 1, "", new(), [], [item]);
            await store.SaveAsync(good);
            var latest = good with { Revision = 2, Draft = "새 초안" };
            await store.SaveAsync(latest);
            var file = Assert.Single(dir.GetFiles("*.json"));
            foreach (var broken in new[] { latest with { Items = [JsonSerializer.SerializeToElement<object?>(null)] }, latest with { History = [null!] },
                         latest with { Overrides = new() { [1] = null! } } })
            {
                await File.WriteAllTextAsync(file.FullName, JsonSerializer.Serialize(broken));
                var result = Assert.IsType<CaptionRecovery>(store.Read(good.Plan));
                Assert.Equal(good.Draft, result.Draft);
                Assert.Equal(dto, Assert.Single(result.Items).Deserialize<WorshipListItemDto>());
            }
        }
        finally { dir.Delete(true); }
    }

    [Fact]
    public async Task QueueEditsWhileConsoleIsClosedRetainCaptionDraftAndFormatting()
    {
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            using var store = new CaptionRecoveryStore(dir.FullName, MainViewModelCaptionHost.IsRecoverableItem);
            var plan = new CaptionRecovery(1,7,"주일","다음 설교",null,1,"",new() { [1] = "악보 가사" },[],[]);
            await store.SaveAsync(plan);
            var item = JsonSerializer.SerializeToElement(new WorshipListItemDto("song:4","찬양","Song",1,"가사",null,Sequence:"2 C"));
            await store.SaveQueueAsync("주일", [item,item], 1);
            var saved = store.Read("주일")!;
            Assert.Equal("다음 설교",saved.Draft);
            Assert.Equal("악보 가사",saved.Overrides[1]);
            Assert.Equal("2 C",saved.Items[1].Deserialize<WorshipListItemDto>()!.Sequence);
            Assert.Equal(1,saved.SelectedIndex);
        }
        finally { dir.Delete(true); }
    }

    [Fact]
    public void DeckUsesPresentationOrderAndKeepsParagraphsAndLineBreaks()
    {
        var dir = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(dir.FullName, "찬양.pptx");
            using (var package = Package.Open(path, FileMode.Create))
            {
                const string ns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
                var presentation = Add("/ppt/presentation.xml", """<p:presentation xmlns:p="urn:p" xmlns:r="urn:r"><p:sldIdLst><p:sldId id="2" r:id="b"/><p:sldId id="1" r:id="a"/></p:sldIdLst></p:presentation>""");
                package.CreateRelationship(presentation.Uri,TargetMode.Internal,ns+"/officeDocument");
                foreach(var (id, text) in new[] {("a","첫째"),("b","둘째")})
                {
                    var slide = Add("/ppt/slides/"+id+".xml", $"""<p:sld xmlns:p="urn:p" xmlns:a="urn:a"><p:sp><p:nvSpPr><p:cNvPr name="가사"/></p:nvSpPr><p:txBody><a:p><a:r><a:t>{text}</a:t></a:r><a:br/><a:r><a:t>말씀</a:t></a:r></a:p><a:p><a:r><a:t>찬양</a:t></a:r></a:p></p:txBody></p:sp></p:sld>""");
                    presentation.CreateRelationship(PackUriHelper.GetRelativeUri(presentation.Uri,slide.Uri),TargetMode.Internal,ns+"/slide",id);
                }
                PackagePart Add(string name, string xml)
                {
                    var part = package.CreatePart(new Uri(name, UriKind.Relative),"application/xml");
                    using var writer = new StreamWriter(part.GetStream());
                    writer.Write(xml);
                    return part;
                }
            }
            var result = CaptionDeck.Read(path);
            Assert.Equal("둘째\n말씀\n찬양", result.Slides[0].Boxes[^1].Text);
            Assert.Equal([CaptionDeck.AllTextKey, "shape:가사"], result.Slides[1].Boxes.Select(box => box.Key)); // a036: the whole text first
            Assert.Equal("둘째\n말씀\n찬양", result.Slides[0].Boxes[0].Text);
            var live = new LiveSessionService();
            using var captions = new CaptionSession(live.ForCaptions());
            captions.Prepare(result);
            captions.Follow(1,"shape:가사");
            captions.SetVisible(true);
            live.GoLive(new("ppt","찬양","PowerPoint"){ContentPath=path,SlideNumber=2},"audience");
            Assert.Equal("첫째\n말씀\n찬양",captions.PublicText);
        }
        finally { dir.Delete(true); }
    }
}
