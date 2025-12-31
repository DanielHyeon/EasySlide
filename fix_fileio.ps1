# Fix LoadInfoFile - using 문에서 ref 파라미터 사용 불가 문제 수정

$filePath = "Easislides\Global\gfFileIO.cs"
$content = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)

$oldCode = @'
		using (XmlTextReader reader = new XmlTextReader(InFileName))
			{
				try
				{
					bool flag = false;
					if (ValidateEasiSlidesXML(ref reader))
					{
						string itemID = InItem.ItemID;
						ExtractEasiSlidesXMLItem(ref reader, ref InItem);
						InItem.ItemID = itemID;
					}
					else
					{
						Load32InfoFile(InFileName, ref InItem, ref ThisHeaderData);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error in LoadInfoFile (inner): {ex.Message}");
					Console.WriteLine(ex.StackTrace);
				}
			}
'@

$newCode = @'
			XmlTextReader reader = new XmlTextReader(InFileName);
			try
			{
				if (ValidateEasiSlidesXML(ref reader))
				{
					string itemID = InItem.ItemID;
					ExtractEasiSlidesXMLItem(ref reader, ref InItem);
					InItem.ItemID = itemID;
				}
				else
				{
					Load32InfoFile(InFileName, ref InItem, ref ThisHeaderData);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in LoadInfoFile (inner): {ex.Message}");
				Console.WriteLine(ex.StackTrace);
			}
			finally
			{
				reader?.Close();
			}
'@

$newContent = $content.Replace($oldCode, $newCode)

if ($newContent -eq $content)
{
    Write-Host "Warning: No replacement made" -ForegroundColor Yellow
    exit 1
}

[System.IO.File]::WriteAllText($filePath, $newContent, [System.Text.Encoding]::UTF8)
Write-Host "Fixed LoadInfoFile method" -ForegroundColor Green
