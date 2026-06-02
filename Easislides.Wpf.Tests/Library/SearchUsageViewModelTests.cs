using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Easislides.Wpf.Data;
using Easislides.Wpf.Library;
using Easislides.Wpf.Settings;
using FluentAssertions;
using Xunit;

namespace Easislides.Wpf.Tests.Library;

public class SearchUsageViewModelTests
{
    [Fact]
    public async Task LoadAsync_DerivesDatabasePathsAndFolders()
    {
        using var fixture = new SearchUsageViewModelFixture();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);

        var sut = fixture.CreateViewModel();

        await sut.LoadAsync();

        sut.DatabasePath.Should().Be(Path.GetFullPath(fixture.AdminDatabasePath));
        sut.UsageDatabasePath.Should().Be(Path.GetFullPath(fixture.UsageDatabasePath));
        sut.SearchFolders.Select(folder => folder.Name).Should().Equal("Morning", "Evening");
        sut.SearchFolders.Should().OnlyContain(folder => folder.IsSelected);
    }

    [Fact]
    public async Task SearchAndLookupAsync_UsesSelectedFieldsAndFolders()
    {
        using var fixture = new SearchUsageViewModelFixture();
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var sut = fixture.CreateViewModel();
        await sut.LoadAsync();
        sut.SearchText = "grace";
        sut.SearchFolders[1].IsSelected = false;
        sut.SearchLyrics = false;
        sut.SearchBookReference = true;

        await sut.SearchSongsAsync();
        await sut.LookupTitlesAsync();

        fixture.Service.LastSearchRequest.Should().NotBeNull();
        fixture.Service.LastSearchRequest!.FolderNos.Should().Equal(1);
        fixture.Service.LastSearchRequest.Fields.Should().HaveFlag(SongSearchFields.Title);
        fixture.Service.LastSearchRequest.Fields.Should().HaveFlag(SongSearchFields.BookReference);
        fixture.Service.LastSearchRequest.Fields.Should().NotHaveFlag(SongSearchFields.Lyrics);
        fixture.Service.LastLookupPattern.Should().Be("grace");
        sut.SearchResults.Should().ContainSingle().Which.Title.Should().Be("Amazing Grace");
        sut.LookupCandidates.Should().ContainSingle().Which.Title.Should().Be("Amazing Grace");
    }

    [Fact]
    public void HasNoSearchResults_IsFalse_BeforeAnySearch()
    {
        // 검색 전 첫 화면 — 결과 목록이 비어 있어도 "결과 없음" 안내는 뜨지 않아야 한다(검색을 안 했을 뿐).
        using var fixture = new SearchUsageViewModelFixture();
        var sut = fixture.CreateViewModel();

        sut.HasSearched.Should().BeFalse("아직 검색을 실행하지 않음");
        sut.HasNoSearchResults.Should().BeFalse("검색 전엔 빈 상태 안내가 뜨면 안 됨");
    }

    [Fact]
    public async Task HasNoSearchResults_IsTrue_AfterSearchReturnsEmpty()
    {
        // 검색을 실행했는데 결과가 0건 → "결과 없음" 안내가 뜬다.
        using var fixture = new SearchUsageViewModelFixture();
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        fixture.Service.SearchResultsOverride = []; // 결과 없음 강제.
        var sut = fixture.CreateViewModel();
        await sut.LoadAsync();
        sut.SearchText = "없는곡";

        await sut.SearchSongsAsync();

        sut.HasSearched.Should().BeTrue("검색을 실행함");
        sut.SearchResults.Should().BeEmpty();
        sut.HasNoSearchResults.Should().BeTrue("검색했는데 결과가 없으면 빈 상태 안내 표시");
    }

    [Fact]
    public async Task HasNoSearchResults_IsFalse_AfterSearchReturnsHits()
    {
        // 검색 결과가 있으면 "결과 없음" 안내는 뜨지 않는다.
        using var fixture = new SearchUsageViewModelFixture();
        fixture.Settings.Set(EasiSettingKeys.AdminDatabasePath, fixture.AdminDatabasePath);
        var sut = fixture.CreateViewModel();
        await sut.LoadAsync();
        sut.SearchText = "grace";

        await sut.SearchSongsAsync();

        sut.SearchResults.Should().NotBeEmpty();
        sut.HasNoSearchResults.Should().BeFalse("결과가 있으면 빈 상태 안내 숨김");
    }

    [Fact]
    public async Task RefreshDeleteAndExportUsageAsync_UsesCurrentUsageReport()
    {
        using var fixture = new SearchUsageViewModelFixture();
        fixture.Settings.Set(EasiSettingKeys.WorkingFolder, fixture.WorkingFolder);
        var sut = fixture.CreateViewModel();
        await sut.LoadAsync();
        sut.UsageFrom = new DateTime(2026, 5, 1);
        sut.UsageTo = new DateTime(2026, 5, 31);
        sut.SelectedUsageSession = "Sunday AM";
        sut.UsageReportOutputPath = Path.Combine(fixture.Root, "usage.rtf");

        await sut.RefreshUsageAsync();
        sut.UsageRecords[0].IsSelected = true;
        await sut.DeleteSelectedUsageAsync();
        await sut.ExportUsageReportAsync();

        fixture.Service.LastUsageRequest.Should().NotBeNull();
        fixture.Service.LastUsageRequest!.SelectedSession.Should().Be("Sunday AM");
        fixture.Service.LastDeletedRecordIds.Should().Equal(100);
        fixture.Service.LastExportPath.Should().Be(sut.UsageReportOutputPath);
        sut.UsageSummary.Should().ContainSingle().Which.Occurrences.Should().Be(2);
    }

    [Fact]
    public async Task DeleteSelectedUsageAsync_WhenConfirmationDeclines_DoesNotDelete()
    {
        using var fixture = new SearchUsageViewModelFixture();
        fixture.Confirmation.NextResult = false;
        var sut = fixture.CreateViewModel();
        await sut.LoadAsync();
        await sut.RefreshUsageAsync();
        sut.UsageRecords[0].IsSelected = true;

        await sut.DeleteSelectedUsageAsync();

        fixture.Confirmation.LastRequest.Should().NotBeNull();
        fixture.Confirmation.LastRequest!.RecordCount.Should().Be(1);
        fixture.Confirmation.LastRequest.Session.Should().Be("");
        fixture.Confirmation.LastRequest.Records.Should().ContainSingle()
            .Which.RecordId.Should().Be(100);
        fixture.Service.LastDeletedRecordIds.Should().BeEmpty();
        fixture.Service.GetUsageCallCount.Should().Be(1);
        sut.StatusMessage.Should().Be("Usage delete canceled.");
        sut.UsageRecords.Should().ContainSingle();
    }

    [Fact]
    public async Task DeleteSelectedUsageAsync_WhenConfirmationApproves_DeletesAndRefreshesUsage()
    {
        using var fixture = new SearchUsageViewModelFixture();
        fixture.Service.UsageRecordsAfterDelete = [];
        var sut = fixture.CreateViewModel();
        await sut.LoadAsync();
        await sut.RefreshUsageAsync();
        sut.UsageRecords[0].IsSelected = true;

        await sut.DeleteSelectedUsageAsync();

        fixture.Confirmation.LastRequest.Should().NotBeNull();
        fixture.Service.LastDeletedRecordIds.Should().Equal(100);
        fixture.Service.GetUsageCallCount.Should().Be(2);
        sut.UsageRecords.Should().BeEmpty();
        sut.StatusMessage.Should().Be("1 usage records deleted.");
    }

    private sealed class SearchUsageViewModelFixture : IDisposable
    {
        public SearchUsageViewModelFixture()
        {
            Root = Path.Combine(Path.GetTempPath(), $"EasiSlides_SearchUsageVm_{Guid.NewGuid():N}");
            WorkingFolder = Path.Combine(Root, "Work");
            AdminDatabasePath = Path.Combine(WorkingFolder, "Admin", "Database", "EasiSlidesDb.db");
            UsageDatabasePath = Path.Combine(WorkingFolder, "Admin", "Database", "EsUsage.db");
            Directory.CreateDirectory(Path.GetDirectoryName(AdminDatabasePath)!);
            File.WriteAllText(AdminDatabasePath, "");
            File.WriteAllText(UsageDatabasePath, "");
            Settings = new SettingsService(new SettingsServiceOptions(Path.Combine(Root, "settings.json"), Path.Combine(Root, "Backups")));
            Service = new FakeSearchUsageService();
        }

        public string Root { get; }

        public string WorkingFolder { get; }

        public string AdminDatabasePath { get; }

        public string UsageDatabasePath { get; }

        public SettingsService Settings { get; }

        public FakeSearchUsageService Service { get; }

        public FakeUsageDeleteConfirmation Confirmation { get; } = new();

        public SearchUsageViewModel CreateViewModel()
            => new(Settings, Service, Confirmation);

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
            }
        }
    }

    private sealed class FakeSearchUsageService : ISearchUsageService
    {
        public SongSearchRequest? LastSearchRequest { get; private set; }

        public string? LastLookupPattern { get; private set; }

        public UsageRequest? LastUsageRequest { get; private set; }

        public IReadOnlyList<long> LastDeletedRecordIds { get; private set; } = [];

        public int GetUsageCallCount { get; private set; }

        public IReadOnlyList<UsageRecord>? UsageRecordsAfterDelete { get; set; }

        public string? LastExportPath { get; private set; }

        public Task<IReadOnlyList<SongFolderSummary>> GetFoldersAsync(string databasePath)
            => Task.FromResult<IReadOnlyList<SongFolderSummary>>([
                new SongFolderSummary(1, "Morning", IsEnabled: true, SongCount: 1),
                new SongFolderSummary(2, "Evening", IsEnabled: true, SongCount: 1),
            ]);

        /// <summary>설정하면 검색이 이 결과를 돌려준다(빈 리스트로 "결과 없음" 케이스 테스트). 기본은 Amazing Grace 한 건.</summary>
        public IReadOnlyList<SongSearchResult>? SearchResultsOverride { get; set; }

        public Task<IReadOnlyList<SongSearchResult>> SearchSongsAsync(SongSearchRequest request)
        {
            LastSearchRequest = request;
            return Task.FromResult(SearchResultsOverride ?? (IReadOnlyList<SongSearchResult>)[
                new SongSearchResult(10, 1, "Morning", "Amazing Grace", "", 7, "", "G", ["Title"], "Amazing Grace"),
            ]);
        }

        public Task<IReadOnlyList<LookupTitleCandidate>> LookupTitlesAsync(string databasePath, string titlePattern)
        {
            LastLookupPattern = titlePattern;
            return Task.FromResult<IReadOnlyList<LookupTitleCandidate>>([
                new LookupTitleCandidate(10, "Amazing Grace", "", 1, "Morning", "Ps 23", "CCLI"),
            ]);
        }

        public Task<UsageReport> GetUsageAsync(UsageRequest request)
        {
            LastUsageRequest = request;
            GetUsageCallCount++;
            var records = LastDeletedRecordIds.Count > 0 && UsageRecordsAfterDelete is not null
                ? UsageRecordsAfterDelete
                : [new UsageRecord(100, new DateTime(2026, 5, 1), "Sunday AM", "Amazing Grace", 7, 10, "A1", "A2")];
            IReadOnlyList<UsageSummary> summary = records.Count == 0
                ? []
                : [new UsageSummary("Amazing Grace", 7, 10, 2)];
            return Task.FromResult(new UsageReport(
                true,
                request.DatabasePath,
                request.From,
                request.To,
                ["", "Sunday AM"],
                request.SelectedSession,
                records,
                summary,
                []));
        }

        public Task<UsageDeleteReport> DeleteUsageRecordsAsync(string databasePath, IReadOnlyList<long> recordIds)
        {
            LastDeletedRecordIds = recordIds;
            return Task.FromResult(new UsageDeleteReport(true, databasePath, recordIds.Count, []));
        }

        public Task<UsageExportReport> ExportUsageReportAsync(UsageReport report, string outputPath)
        {
            LastExportPath = outputPath;
            return Task.FromResult(new UsageExportReport(true, outputPath, []));
        }

        public string GetDefaultUsageDatabasePath(string workingFolder)
            => Path.Combine(workingFolder, "Admin", "Database", "EsUsage.db");
    }

    private sealed class FakeUsageDeleteConfirmation : IUsageDeleteConfirmation
    {
        public bool NextResult { get; set; } = true;

        public UsageDeleteConfirmationRequest? LastRequest { get; private set; }

        public Task<bool> ConfirmAsync(UsageDeleteConfirmationRequest request)
        {
            LastRequest = request;
            return Task.FromResult(NextResult);
        }
    }
}
