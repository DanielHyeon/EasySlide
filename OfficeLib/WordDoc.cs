using System;
using NetOffice.WordApi;
using NetOffice.WordApi.Enums;

namespace OfficeLib
{
	public class WordDoc
	{
		/// <summary>
		/// Reads the plain text of a Word document through a hidden Word instance of its own, with line breaks
		/// as "\n". Returns "" when the file cannot be read. The document is opened read-only, without
		/// conversion prompts and without being added to the user's recent files, and Word shows no alerts,
		/// so a locked or converted file cannot leave an invisible dialog waiting.
		/// </summary>
		public string GetContents(string InFileName)
		{
			Application application = null;
			Document document = null;
			object missing = Type.Missing;
			try
			{
				application = new Application();
				application.DisplayAlerts = WdAlertLevel.wdAlertsNone;
				application.Visible = false;
				application.Options.ShowControlCharacters = false;
				document = application.Documents.Open(
					InFileName,
					false,  // ConfirmConversions
					true,   // ReadOnly
					false,  // AddToRecentFiles
					missing, missing, missing, missing, missing, missing, missing,
					false); // Visible
				string documentText = document.Content.Text ?? "";
				return documentText.Replace("\r\n", "\n").Replace('\r', '\n');
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error reading Word document '{InFileName}': {ex.Message}");
				return "";
			}
			finally
			{
				// NetOffice wrappers are released with Dispose, not Marshal.ReleaseComObject (which throws for
				// them: they are managed objects, not __ComObject). Each step runs on its own so a failure in
				// one cannot leave Word running.
				if (document != null)
				{
					try
					{
						document.Close(WdSaveOptions.wdDoNotSaveChanges, missing, missing);
					}
					catch
					{
					}
				}
				if (application != null)
				{
					try
					{
						application.Quit(WdSaveOptions.wdDoNotSaveChanges, missing, missing);
					}
					catch
					{
					}
					try
					{
						// Disposes the Application proxy and every proxy created from it (document included).
						application.Dispose();
					}
					catch
					{
					}
				}
			}
		}
	}
}
