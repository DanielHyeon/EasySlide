 using System;
using System.Runtime.InteropServices;
//using Microsoft.Office.Core;
//using Microsoft.Office.Interop.Word;
//using Application = Microsoft.Office.Interop.Word.Application;
using NetOffice.OfficeApi;
using NetOffice.WordApi;

namespace OfficeLib
{
	public class WordDoc
	{
		private const string vbLf = "\n";

		private const string vbCr = "\r";

		private const string vbCrLf = "\r\n";

		private Application application = null;

		/// <summary>
		/// daniel
		/// </summary>
		/// <param name="InFileName"></param>
		/// <returns></returns>
		public string GetContents(string InFileName)
		{
			try
			{
				application = new Application();
				application.Options.ShowControlCharacters = false;
				application.Visible = false;
				object FileName = InFileName;
				object ConfirmConversions = Type.Missing;
				Document document = application.Documents.Open(FileName, ConfirmConversions, ConfirmConversions, ConfirmConversions, ConfirmConversions, ConfirmConversions, ConfirmConversions, ConfirmConversions, ConfirmConversions, ConfirmConversions, ConfirmConversions, ConfirmConversions);
				string text = document.Content.Text;
				text = text.Replace("\r\n", "\n"[0].ToString());
				text = text.Replace("\r"[0], "\n"[0]);
				document.Close(ConfirmConversions, ConfirmConversions, ConfirmConversions);
				Marshal.ReleaseComObject(document);
				document = null;
				application.Quit(ConfirmConversions, ConfirmConversions, ConfirmConversions);
				return text;
			}
			catch
			{
				return "";
			}
			finally
            {
				if (application != null)
				{
					Marshal.ReleaseComObject(application);
					application = null;
				}

			}
		}
	}
}
