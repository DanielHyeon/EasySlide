# Step 1: StringBuilder로 문자열 연결 최적화

$filePath = "Easislides\Global\gfFileIO.cs"

Write-Host "Reading $filePath..."
$content = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)

# Backup
Copy-Item $filePath "$filePath.bak_refactor1"

# 기존 메서드를 새 메서드로 교체
$oldMethod = @'
	public static string GetOpenFileDialogMediaString(MediaBackgroundStyle InMediaType)
	{
		if (TotalMediaFileExt == 0)
		{
			return "";
		}
		string str = "";
		string text = "";
		int num = 0;
		switch (InMediaType)
		{
			case MediaBackgroundStyle.Audio:
				str = "Audio Files (";
				break;
			case MediaBackgroundStyle.Video:
				str = "Video Files (";
				break;
		}
		bool flag = true;
		for (int i = 0; i < TotalMediaFileExt; i++)
		{
			if (InMediaType == MediaBackgroundStyle.None || MediaFileExtension[i, 1] == InMediaType.ToString())
			{
				str = str + (flag ? "" : ",") + "*" + MediaFileExtension[i, 0];
				text = text + (flag ? "" : ";") + "*" + MediaFileExtension[i, 0];
				flag = false;
			}
		}
		if (flag)
		{
			return "";
		}
		str = ((InMediaType != 0) ? (str + ")") : "Media Files (all types)");
		return str + "|" + text;
	}
'@

$newMethod = @'
	public static string GetOpenFileDialogMediaString(MediaBackgroundStyle InMediaType)
	{
		if (TotalMediaFileExt == 0)
		{
			return "";
		}

		StringBuilder displayName = new StringBuilder();
		StringBuilder extensions = new StringBuilder();

		switch (InMediaType)
		{
			case MediaBackgroundStyle.Audio:
				displayName.Append("Audio Files (");
				break;
			case MediaBackgroundStyle.Video:
				displayName.Append("Video Files (");
				break;
		}

		bool isFirst = true;
		for (int i = 0; i < TotalMediaFileExt; i++)
		{
			if (InMediaType == MediaBackgroundStyle.None || MediaFileExtension[i, 1] == InMediaType.ToString())
			{
				if (!isFirst)
				{
					displayName.Append(',');
					extensions.Append(';');
				}

				string ext = "*" + MediaFileExtension[i, 0];
				displayName.Append(ext);
				extensions.Append(ext);
				isFirst = false;
			}
		}

		if (isFirst)
		{
			return "";
		}

		if (InMediaType != MediaBackgroundStyle.None)
		{
			displayName.Append(')');
		}
		else
		{
			displayName.Append("Media Files (all types)");
		}

		return $"{displayName}|{extensions}";
	}
'@

$newContent = $content.Replace($oldMethod, $newMethod)

if ($newContent -eq $content)
{
    Write-Host "Warning: No replacement made. Pattern not found." -ForegroundColor Yellow
    exit 1
}

[System.IO.File]::WriteAllText($filePath, $newContent, [System.Text.Encoding]::UTF8)

Write-Host "Step 1 completed: StringBuilder optimization applied" -ForegroundColor Green
