using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Easislides.Wpf.Shell;

/// <summary>
/// 최근 연/저장한 예배 순서 이름을 최신순으로 기억한다(레거시 FrmMain "Recent Edits" 대응).
/// 빠른 재오픈용 — 이름만 보관하며, 실제 항목은 IWorshipListStore 가 로드한다.
/// </summary>
public interface IRecentWorshipLists
{
    /// <summary>이름을 최근 목록 맨 앞으로 올린다(중복 제거, 최대 개수 초과분은 버림). 빈 이름은 무시.</summary>
    void Record(string name);

    /// <summary>최근 예배 순서 이름(최신순).</summary>
    IReadOnlyList<string> GetRecent();
}

/// <summary>
/// 최근 예배 순서 목록을 JSON 파일로 영속화 — `%AppData%/EasislidesNext/recent-worshiplists.json`.
/// 최대 8개, 대소문자 무시 중복 제거, 최신순. 파일 손상/접근 불가는 빈 목록으로 우아하게 처리.
/// </summary>
public sealed class RecentWorshipListsService : IRecentWorshipLists
{
    private const int MaxItems = 8;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    private List<string> _items;

    public RecentWorshipListsService()
        : this(DefaultFilePath())
    {
    }

    public RecentWorshipListsService(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _items = Load();
    }

    private static string DefaultFilePath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "EasislidesNext",
        "recent-worshiplists.json");

    public void Record(string name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        // 기존 동일 이름(대소문자 무시) 제거 후 맨 앞에 추가 → 최신순 유지.
        _items.RemoveAll(n => string.Equals(n, trimmed, StringComparison.OrdinalIgnoreCase));
        _items.Insert(0, trimmed);
        if (_items.Count > MaxItems)
        {
            _items = _items.Take(MaxItems).ToList();
        }

        Save();
    }

    public IReadOnlyList<string> GetRecent() => _items.ToList();

    private List<string> Load()
    {
        if (!File.Exists(_filePath))
        {
            return new List<string>();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var names = JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
            return names?.Where(n => !string.IsNullOrWhiteSpace(n)).Take(MaxItems).ToList() ?? new List<string>();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            // 손상/접근 불가 → 빈 목록(호출자 크래시 방지).
            return new List<string>();
        }
    }

    private void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_filePath, JsonSerializer.Serialize(_items, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // 저장 실패는 조용히 무시 — 최근 목록은 편의 기능이라 실패가 운영을 막으면 안 된다.
        }
    }
}
