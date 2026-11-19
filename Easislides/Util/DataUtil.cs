using System;
using System.Data;
using System.Text;
using Easislides.Module;
using DbDataReader = System.Data.SQLite.SQLiteDataReader;

namespace Easislides.Util
{
	class DataUtil
	{

		public static int GetDataInt(DataRow dr, string Fieldname)
		{
			return GetDataInt(dr[Fieldname]);
		}

		public static int GetDataInt(DbDataReader dr, string Fieldname)
		{
			return GetDataInt(dr[Fieldname]);
		}

		private static int GetDataInt(object fieldValue)
		{
			int number = 0;

			try
			{
				if (fieldValue != null)
				{
					string valueStr = fieldValue.ToString();
					bool canConvert = int.TryParse(valueStr, out number);
					if (!canConvert)
					{
						string upperValue = valueStr.ToUpper();
						if (upperValue == "TRUE")
							number = 1;
						else if (upperValue == "FALSE")
							number = 0;
						else
							number = ObjToInt(fieldValue, Minus1IfBlank: false);
					}
				}

				return number;
			}
			catch
			{
				return number;
			}
		}


		public static int GetDataInt(DataRow dr, string Fieldname, bool Minus1IfBlank)
		{
			try
			{
				return ObjToInt(dr[Fieldname], Minus1IfBlank);
			}
			catch
			{
				return Minus1IfBlank ? (-1) : 0;
			}
		}

		public static int GetDataInt(DbDataReader dr, string Fieldname, bool Minus1IfBlank)
		{
			try
			{
				return ObjToInt(dr[Fieldname], Minus1IfBlank);
			}
			catch
			{
				return Minus1IfBlank ? (-1) : 0;
			}
		}


		public static double GetDataDouble(DataRow dr, string Fieldname)
		{
			try
			{
				return ObjToDouble(dr[Fieldname]);
			}
			catch
			{
				return 0.0;
			}
		}

		public static double GetDataDouble(DbDataReader dr, string Fieldname)
		{
			try
			{
				return ObjToDouble(dr[Fieldname]);
			}
			catch
			{
				return 0.0;
			}
		}


		public static string GetDataString(DataRow rs, string Fieldname)
		{
			try
			{
				return rs[Fieldname]?.ToString() ?? "";
			}
			catch
			{
				return "";
			}
		}

		public static string GetDataString(DbDataReader dr, string Fieldname)
		{
			try
			{
				return dr[Fieldname]?.ToString() ?? "";
			}
			catch
			{
				return "";
			}
		}

		public static int ObjToInt(object InValue)
		{
			bool minus1IfBlank = false;
			return ObjToInt(InValue, minus1IfBlank);
		}

		public static int ObjToInt(object InValue, bool Minus1IfBlank)
		{
			try
			{
				if (string.IsNullOrEmpty(InValue.ToString()))
					return Minus1IfBlank ? (-1) : 0;

				return Convert.ToInt32(InValue);
			}
			catch
			{
				return Minus1IfBlank ? (-1) : 0;
			}
		}

		public static double ObjToDouble(object InValue)
		{
			try
			{
				if (string.IsNullOrEmpty(InValue.ToString()))
					return 0.0;

				return Convert.ToDouble(InValue);
			}
			catch
			{
				return 0.0;
			}
		}

		public static string ObjToString(object InValue)
		{
			try
			{
				return Convert.ToString(InValue);
			}
			catch
			{
				return "";
			}
		}

		public static int StringToInt(char InChar)
		{
			return InChar;
		}

		public static int StringToInt(string InString)
		{
			return StringToInt(InString, Minus1IfBlank: false);
		}

		public static int StringToInt(string InString, bool Minus1IfBlank)
		{
			int retVal = -1;
			try
			{
				bool result = int.TryParse(InString, out retVal); //i now = 108 
				if (!result)
				{
					return Minus1IfBlank ? (-1) : 0;
				}

				return retVal;

				//InString = InString.Replace("*", "");
				//InString = InString.Replace(" ", "");
				//return (int)Convert.ToDouble(InString);
			}
			catch
			{
				return Minus1IfBlank ? (-1) : 0;
			}
		}

		// Dates kept as text (SONG.LastModified, registry values): see StoredDate.
		public static string ToStoredDate(DateTime value)
		{
			return StoredDate.ToText(value);
		}

		public static bool TryParseStoredDate(object value, out DateTime result)
		{
			return StoredDate.TryParse(value, out result);
		}

		public static DateTime ObjToDate(object InValue)
		{
			// SONG.LastModified is a TEXT column, so the value is usually a string, not a DateTime.
			return TryParseStoredDate(InValue, out DateTime result) ? result : DateTime.Now;
		}

		public static string Left(ReadOnlySpan<char> sString, int iLen)
		{
			try
			{
				if (iLen <= 0 || sString.Length == 0)
				{
					return "";
				}
				if (iLen < sString.Length)
				{
					return sString.Slice(0, iLen).ToString();
				}
				return sString.ToString();
			}
			catch
			{
				return sString.ToString();
			}
		}

		public static string Right(ReadOnlySpan<char> sString, int iLen)
		{
			try
			{
				int length = sString.Length;

				if (iLen <= 0 || length == 0)
				{
					return ReadOnlySpan<char>.Empty.ToString();
				}

				if (iLen < length)
				{
					return sString.Slice(length - iLen).ToString();
				}

				return sString.ToString();
			}
			catch
			{
				return sString.ToString();
			}
		}

		public static string Mid(ReadOnlySpan<char> sString, int Start)
		{
			return Mid(sString, Start, -1);
		}

		public static string Mid(ReadOnlySpan<char> sString, int Start, int iLen)
		{
			try
			{
				if (iLen == 0 || sString.Length == 0)
				{
					return "";
				}
				if (iLen < 0)
				{
					iLen = sString.Length - Start;
				}
				else if (Start + iLen > sString.Length)
				{
					iLen = sString.Length - Start;
				}
				return sString.Slice(Start, iLen).ToString();
			}
			catch
			{
				return sString.ToString();
			}
		}

		public static string Trim(string InString)
		{
			try
			{
				return InString?.Trim() ?? "";
			}
			catch
			{
				return "";
			}
		}

		public static string TrimEnd(string InString)
		{
			try
			{
				return InString.TrimEnd('\n', '\r');
			}
			catch
			{
				return "";
			}
		}

		public static string TrimStart(string InString)
		{
			try
			{
				return InString.TrimStart('\n', '\r');
			}
			catch
			{
				return "";
			}
		}

		public static bool GetBitBoolean(int InValue, int BitRequired)
		{
			if (GetBitValue(InValue, BitRequired) > 0)
			{
				return true;
			}
			return false;
		}

		public static int GetBitValue(int InValue, int BitRequired)
		{
			if (InValue < 0)
			{
				InValue = 0;
			}
			if (BitRequired < 1 || BitRequired > 8)
			{
				return 0;
			}
			return ((InValue >> (BitRequired - 1)) & 1);
		}

		public static string GetCJKTitle(string InTitle, SortBy CJKMode)
		{
			string FirstCharSym = null;
			return GetCJKTitle(InTitle, CJKMode, ref FirstCharSym);
		}

		public static string GetCJKTitle(string InTitle, SortBy CJKMode, ref string FirstCharSym)
		{
			if (InTitle == "")
			{
				FirstCharSym = "";
				return "";
			}
			string text = "";
			if (CJKMode == SortBy.WordCount)
			{
				text = CJK_WordCount(InTitle);
			}
			if (FirstCharSym != null)
			{
				int num = InTitle[0];
				if (num < 0)
				{
					num += 65536;
				}
				int num2 = Gf.StrokeCount[num];
				if (num2 > 0)
				{
					if (text != "")
					{
						FirstCharSym = text;
					}
					else
					{
						FirstCharSym = num2.ToString("00");
					}
				}
				else
				{
					FirstCharSym = " " + Left(InTitle, 1);
				}
			}
			text += CJK_StrokeCount(InTitle);
			if (text.Length > 100)
			{
				text = DataUtil.Left(text, 100);
			}
			return text;
		}

		public static string CJK_WordCount(string DName)
		{
			if (DName == "" || ((DataUtil.StringToInt(DName[0]) > 0) & (DataUtil.StringToInt(DName[0]) < 128)))
			{
				return "000";
			}
			int num = DName.IndexOf("(") - 1;
			if (num < 0)
			{
				num = DName.Length;
			}
			int num2 = DName.IndexOf(" ") - 1;
			if (num2 > 0 && num2 < num)
			{
				num = num2;
			}
			return num.ToString("000");
		}

		public static string CJK_StrokeCount(string DName)
		{
			string str = "";
			int num = 0;
			long num2 = (!(DName == "")) ? DataUtil.StringToInt(DName[0]) : 0;
			if (num2 < 0)
			{
				num2 += 65536;
			}
			int num3 = Gf.StrokeCount[num2];
			if (num3 > 0)
			{
				str = str + num3.ToString("00") + num2.ToString("00000");
				DName = DataUtil.Right(DName, DName.Length - 1);
				while (DName.Length > 0)
				{
					num++;
					if (num < 6)
					{
						num2 = (int)DName[0];
						if (num2 < 0)
						{
							num2 += 65536;
						}
						num3 = Gf.StrokeCount[num2];
						str = ((num3 <= 0) ? (str + "00" + num2.ToString("00000")) : (str + num3.ToString("00") + num2.ToString("00000")));
						DName = DataUtil.Right(DName, DName.Length - 1);
					}
					else
					{
						str += DName;
						DName = "";
					}
				}
				if (str.Length > 100)
				{
					str = DataUtil.Left(str, 100);
				}
			}
			else
			{
				str = "000" + DName;
			}
			return str;
		}

		public static string Format6(string InString)
		{
			InString = Trim(InString);
			if (InString.Length == 0)
			{
				return "000000";
			}
			return InString.Length > 6 
				? InString.Substring(0, 6) 
				: InString.PadLeft(6, '0');
		}

		public static string UnicodeToAscii_RTF(string InString) => EscapeDocumentText(InString, @"\u", '?');

		public static string UnicodeToAscii_HTML(string InString) => EscapeDocumentText(InString, "&#", ';');

		// The file formats encode UTF-16 code units individually, including surrogate pairs.
		private static string EscapeDocumentText(string text, string prefix, char suffix)
		{
			ArgumentNullException.ThrowIfNull(text);
			int firstEscape = text.AsSpan().IndexOfAnyInRange('\u0100', char.MaxValue);
			if (firstEscape < 0)
				return text;

			var result = new StringBuilder(text.Length);
			result.Append(text.AsSpan(0, firstEscape));
			foreach (char c in text.AsSpan(firstEscape))
			{
				if (c > '\u00ff')
					result.Append($"{prefix}{(int)c:D5}{suffix}");
				else
					result.Append(c);
			}
			return result.ToString();
		}

		public static string HexString(string InString)
		{
			ArgumentNullException.ThrowIfNull(InString);
			int firstEscape = InString.AsSpan().IndexOfAnyInRange('\u0080', char.MaxValue);
			if (firstEscape < 0)
				return InString;

			var result = new StringBuilder(InString.Length);
			result.Append(InString.AsSpan(0, firstEscape));
			foreach (char c in InString.AsSpan(firstEscape))
			{
				if (char.IsAscii(c))
					result.Append(c);
				else
					result.Append($"{(int)c:x}");
			}
			return result.ToString();
		}

		public static int CountLf(string InText) => InText.AsSpan().Count('\n');

		public static int Convertv32FormatString(ref string InFormatString, char InSeparator)
		{
			string InfoHeading = "";
			string InfoFolder = "";
			string InfoRotate = "";
			return Convertv32FormatString(ref InFormatString, InSeparator, ref InfoHeading, ref InfoFolder, ref InfoRotate);
		}

		public static int Convertv32FormatString(ref string InFormatString, char InSeparator, ref string InfoHeading, ref string InfoFolder, ref string InfoRotate)
		{
			InFormatString = InFormatString.Replace("[", "");
			InFormatString = InFormatString.Replace("]", "");
			if (InFormatString == "")
			{
				return -1;
			}
			var convertedFormat = new StringBuilder();
			int result = -1;
			ReadOnlySpan<char> header = InFormatString;
			foreach (Range range in header.Split(InSeparator))
			{
				var field = header[range];
				if (field.IsEmpty || field[0] == '=')
					continue;
				int remappedFieldIndex = Remap32Field(field[0]);
				int equals = field.IndexOf('=');
				var value = equals < 0 ? ReadOnlySpan<char>.Empty : field[(equals + 1)..];
				if (remappedFieldIndex > 1 && remappedFieldIndex < 10000)
				{
					convertedFormat.Append(remappedFieldIndex).Append('=').Append(value).Append('>');
				}
				switch (remappedFieldIndex)
				{
					case 1:
						int.TryParse(value, out result);
						break;
					case 10001:
						InfoHeading = value.ToString();
						break;
					case 10002:
						InfoFolder = value.ToString();
						break;
					case 10003:
						InfoRotate = value.ToString();
						break;
				}
			}
			InFormatString = convertedFormat.ToString();
			return result;
		}

		public static int Remap32Index(string InString) => string.IsNullOrEmpty(InString) ? -1 : Remap32Field(InString[0]);

		private static int Remap32Field(int legacyFieldCode) => legacyFieldCode switch
		{
			1 => 1,
			2 => 11,
			3 => 12,
			4 => 13,
			5 => 14,
			6 => 15,
			8 => 22,
			9 => 25,
			10 => 26,
			11 => 27,
			12 => 28,
			13 => 29,
			14 => 30,
			15 => 31,
			16 => 41,
			17 => 42,
			19 => 43,
			20 => 44,
			21 => 45,
			22 => 46,
			23 => 47,
			24 => 48,
			26 => 61,
			27 => 62,
			28 => 63,
			29 => 64,
			30 => 65,
			41 => 101,
			42 => 102,
			46 => 106,
			47 => 107,
			51 => 111,
			52 => 112,
			56 => 116,
			57 => 117,
			61 => 121,
			62 => 122,
			66 => 131,
			67 => 132,
			71 => 136,
			72 => 137,
			81 => 151,
			82 => 153,
			83 => 154,
			84 => 155,
			85 => 170,
			86 => 171,
			87 => 172,
			95 => 174,
			96 => 180,
			110 => 10001,
			111 => 10002,
			31 => 10003,
			_ => -1
		};

		public static string ExtractOneInfo(ref string InString, char ExtractSym)
		{
			return ExtractOneInfo(ref InString, ExtractSym, RemoveExtract: true);
		}

		public static string ExtractOneInfo(ref string InString, char ExtractSym, bool RemoveExtract)
		{
			return ExtractOneInfo(ref InString, ExtractSym, RemoveExtract, MinusOneIfBlank: true);
		}

		public static string ExtractOneInfo(ref string InString, char ExtractSym, bool RemoveExtract, bool MinusOneIfBlank)
		{
			if (InString == null || InString.Length == 0)
			{
				if (MinusOneIfBlank)
				{
					return "-1";
				}
				return "";
			}
			int num = InString.IndexOf(ExtractSym);
			string result;
			if (num >= 0)
			{
				result = DataUtil.Mid(InString, 0, num);
				if (RemoveExtract)
				{
					InString = DataUtil.Mid(InString, num + 1);
				}
				return result;
			}
			result = InString;
			if (RemoveExtract)
			{
				InString = "";
			}
			if (result == "" && MinusOneIfBlank)
			{
				return "-1";
			}
			return result;
		}
	}
}
