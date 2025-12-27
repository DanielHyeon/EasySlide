using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easislides.Util
{

	class FileUtil
    {
		public static readonly Encoding Utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

		public enum FileContentsType
		{
			DoubleByte,
			Ascii_Rtf,
			Ascii_Html
		}

		public static bool MakeDir(string sDir)
		{
			try
			{
				Directory.CreateDirectory(sDir);
				return true;
			}
			catch
			{
				if (Directory.Exists(sDir))
				{
					return true;
				}
				return false;
			}
		}

		public static bool CreateNewFile(string FILE_NAME)
		{
			return CreateNewFile(FILE_NAME, FileContentsType.DoubleByte);
		}

		public static bool CreateNewFile(string FILE_NAME, FileContentsType Mode)
		{
			return CreateNewFile(FILE_NAME, Mode, "");
		}

		public static bool CreateNewFile(string FILE_NAME, FileContentsType Mode, string Contents)
		{
			try
			{
				if (Mode == FileContentsType.DoubleByte)
				{
					using FileStream fileStream = new FileStream(FILE_NAME, FileMode.Create);
					BinaryWriter w = new BinaryWriter(fileStream);
					w.Write(byte.MaxValue);
					w.Write((byte)254);
					WriteStringToBinaryFile(ref w, Contents);
					w.Flush();
					w.Dispose();
					//fileStream.Close();
					//fileStream = null;
					return true;
				}

				using StreamWriter streamWriter = new StreamWriter(FILE_NAME, append: false, Utf8WithBom);
				streamWriter.AutoFlush = true;
				streamWriter.Write((Mode == FileContentsType.Ascii_Rtf) ? DataUtil.UnicodeToAscii_RTF(Contents) : DataUtil.UnicodeToAscii_HTML(Contents));
				//streamWriter.Flush();
				//streamWriter.Close();
			}
			catch
			{
			}
			return false;
		}

		public static void CopyFiles(string SrcFolder, string DestFolder)
		{
			FileUtil.MakeDir(DestFolder);
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(SrcFolder);
				string[] files = Directory.GetFiles(SrcFolder, "*.*");
				string[] array = files;
				foreach (string text in array)
				{
					try
					{
						File.Copy(text, DestFolder + "\\" + Path.GetFileName(text), overwrite: false);
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}


		public static void WriteStringToBinaryFile(ref BinaryWriter w, string InString)
		{
			w.Write(Encoding.Unicode.GetBytes(InString));
		}
	}
}
