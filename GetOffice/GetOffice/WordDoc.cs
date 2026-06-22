using System;
using Word;

namespace GetOffice
{
	public class WordDoc
	{
		private const string vbLf = "\n";

		private const string vbCr = "\r";

		private const string vbCrLf = "\r\n";

		public string GetContents(string InFileName)
		{
			try
			{
				Application application = new ApplicationClass();
				application.Options.ShowControlCharacters = false;
				application.Visible = false;
				object FileName = InFileName;
				object ConfirmConversions = Type.Missing;
				Document document = application.Documents.Open(ref FileName, ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions);
				string text = document.Content.Text;
				text = text.Replace("\r\n", "\n"[0].ToString());
				text = text.Replace("\r"[0], "\n"[0]);
				document.Close(ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions);
				application.Quit(ref ConfirmConversions, ref ConfirmConversions, ref ConfirmConversions);
				application = null;
				return text;
			}
			catch
			{
				return "";
			}
		}
	}
}
