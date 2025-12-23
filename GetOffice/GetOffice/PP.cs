using Office;
using PowerPoint;
using System;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Linq;

namespace GetOffice
{
    public delegate void PreviewEvent(int index);

	public class PP
	{
        public PreviewEvent preViewEvent;

		public PowerPoint.Application App;

		public Presentation Pres;

		public string[,] ScreenDumpFullPath = new string[2, 10000];

		public void Init()
		{
			try
			{
				App = new PowerPoint.Application();
			}
			catch
			{
			}
		}

		public string Open(string InPowerpointFullPath, ref string[,] PowerpointList, ref int TotalPowerpointItems)
		{
			string text = "";
			try
			{
				_ = App.Presentations.Count;
			}
			catch
			{
			}
			try
			{
				if (TotalPowerpointItems < 0)
				{
					TotalPowerpointItems = 0;
				}
				text = GetPresentationName(InPowerpointFullPath, PowerpointList, TotalPowerpointItems);
				bool flag = false;
				if (text != "")
				{
					for (int i = 1; i <= App.Presentations.Count; i++)
					{
						if (flag)
						{
							break;
						}
						if (App.Presentations.Item(i).Name == text)
						{
							flag = true;
						}
					}
				}
				try
				{
					if (!flag)
					{
						App.Presentations.Open(InPowerpointFullPath, MsoTriState.msoTrue, MsoTriState.msoTrue);
						PowerpointList[TotalPowerpointItems + 1, 0] = InPowerpointFullPath;
						PowerpointList[TotalPowerpointItems + 1, 1] = App.Presentations.Application.ActivePresentation.FullName;
						TotalPowerpointItems++;
					}
				}
				catch
				{
					return "Error Loading: " + text;
				}
			}
			catch
			{
				return "Error Loading: " + text;
			}
			return "";
		}

		public void Run(int InIndex)
		{
			try
			{
				App.Presentations.Item(InIndex).SlideShowSettings.Run();
			}
			catch
			{
			}
		}

		public string Run(string InPowerpointFullPath, ref string[,] PowerpointList, ref int TotalPowerpointItems)
		{
			string text = "";
			string text2 = "";
			try
			{
				text = GetPresentationName(InPowerpointFullPath, PowerpointList, TotalPowerpointItems);
				text2 = "";
				if (!(text != ""))
				{
					return "Cannot find opened Presentation: " + TotalPowerpointItems + ", " + InPowerpointFullPath;
				}
				for (int i = 1; i <= App.Presentations.Count; i++)
				{
					string text3 = text2;
					text2 = text3 + i + "-" + App.Presentations.Item(i).Name + "(" + App.Presentations.Item(i).FullName + ") ";
					if (App.Presentations.Item(i).FullName.ToUpper() == text.ToUpper())
					{
						App.Presentations.Item(i).SlideShowSettings.Run();
						return "Presentation Run OK";
					}
				}
			}
			catch
			{
				return "Error: " + TotalPowerpointItems + ", " + InPowerpointFullPath;
			}
			return "Can't find Presentations: " + text + ".  " + text2;
		}

		public string GetPresentationName(string InPowerpointFullPath, string[,] PowerpointList, int TotalPowerpointItems)
		{
			try
			{
				for (int i = 1; i <= TotalPowerpointItems; i++)
				{
					try
					{
						if (PowerpointList[i, 0] == InPowerpointFullPath)
						{
							return PowerpointList[i, 1];
						}
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
			return "";
		}

		public void First()
		{
			try
			{
				App.ActivePresentation.SlideShowWindow.View.First();
			}
			catch
			{
			}
		}

		public void Last()
		{
			try
			{
				App.ActivePresentation.SlideShowWindow.View.Last();
			}
			catch
			{
			}
		}

		public void GotoSlide(int StartingSlide)
		{
			try
			{
				App.ActivePresentation.SlideShowWindow.View.GotoSlide(StartingSlide);
			}
			catch
			{
			}
		}

		public int Count()
		{
			try
			{
				return App.ActivePresentation.Slides.Count;
			}
			catch
			{
				return 0;
			}
		}

		public void SetShowWindow(float Left, float Top, float Width, float Height)
		{
			try
			{
				App.ActivePresentation.SlideShowWindow.Left = Left;
				App.ActivePresentation.SlideShowWindow.Top = Top;
				App.ActivePresentation.SlideShowWindow.Width = Width;
				App.ActivePresentation.SlideShowWindow.Height = Height;
			}
			catch
			{
			}
		}

		public void ResSetAllShowWindows()
		{
			try
			{
				if (App.Presentations.Count > 0)
				{
					for (int i = 1; i <= App.Presentations.Count; i++)
					{
						try
						{
							App.Presentations.Item(i).SlideShowWindow.Left = 0f;
							App.Presentations.Item(i).SlideShowWindow.Top = 0f;
							App.Presentations.Item(i).SlideShowWindow.Width = 1f;
							App.Presentations.Item(i).SlideShowWindow.Height = 1f;
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
		}

		public string ClearUpPowerpointWindows(ref string[,] PowerpointList, ref int TotalPowerpointItems)
		{
			if (TotalPowerpointItems < 1)
			{
				return "";
			}
			int num = TotalPowerpointItems;
			int num2 = 0;
			int num3 = 0;
			bool flag = false;
			try
			{
				num2 = App.Presentations.Count;
				if (TotalPowerpointItems > 0 && App.Presentations.Count > 0)
				{
					for (int num4 = App.Presentations.Count; num4 > 0; num4--)
					{
						for (int i = 1; i <= TotalPowerpointItems; i++)
						{
							try
							{
								if (App.Presentations.Item(num4).FullName.ToUpper() == PowerpointList[i, 1].ToUpper())
								{
									App.Presentations.Item(num4).Close();
									num3++;
								}
							}
							catch
							{
							}
						}
					}
				}
			}
			catch
			{
				flag = true;
			}
			TotalPowerpointItems = 0;
			try
			{
				if (App.Presentations.Count == 0)
				{
					App.Quit();
				}
			}
			catch
			{
			}
			return "Opened PP=" + num2 + ", Total On List=" + num + ", Numbers Closed=" + num3 + (flag ? ". Error When Closing" : ". No Error");
		}

		public void NewApp()
		{
			try
			{
				App = new PowerPoint.Application();
				App.Activate();
				App.WindowState = PpWindowState.ppWindowMinimized;
			}
			catch
			{
			}
		}

		public int ImplementPowerpointSlideMovement(ref int InCurSlide, int InCurMaxSlide, Keys InKey)
		{
			try
			{
				return ImplementPowerpointSlideMovement(ref InCurSlide, InCurMaxSlide, InKey, 1);
			}
			catch
			{
				return InCurSlide;
			}
		}

		public int ImplementPowerpointSlideMovement(ref int InCurSlide, int InCurMaxSlide, Keys InKey, int InSlideNo)
		{
			try
			{
				if (App == null)
				{
					return 1;
				}
				switch (InKey)
				{
					case Keys.Left:
						App.ActivePresentation.SlideShowWindow.View.First();
						InCurSlide = 1;
						break;
					case Keys.Up:
						App.ActivePresentation.SlideShowWindow.View.Previous();
						InCurSlide = App.ActivePresentation.SlideShowWindow.View.Slide.SlideIndex;
						break;
					case Keys.Down:
						App.ActivePresentation.SlideShowWindow.View.Next();
						if (App.ActivePresentation.SlideShowWindow.View.State == PpSlideShowState.ppSlideShowDone)
						{
							App.ActivePresentation.SlideShowWindow.View.Last();
							InCurSlide = -1;
						}
						else
						{
							InCurSlide = App.ActivePresentation.SlideShowWindow.View.Slide.SlideIndex;
						}
						break;
					case Keys.Right:
						App.ActivePresentation.SlideShowWindow.View.Last();
						InCurSlide = InCurMaxSlide;
						break;
					case Keys.None:
						App.ActivePresentation.SlideShowWindow.View.GotoSlide(InSlideNo, MsoTriState.msoFalse);
						InCurSlide = InSlideNo;
						break;
					case Keys.F5:
						App.ActivePresentation.SlideShowWindow.View.GotoSlide(InCurSlide, MsoTriState.msoFalse);
						break;
				}
			}
			catch
			{
			}
			return InCurSlide;
		}

		public void LoadVersesAndSlides(ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			try
			{
				int index = 2;
				int num = -1;
				bool flag = false;
				SequenceSymbol.GetUpperBound(0);
				for (int i = 1; i <= App.ActivePresentation.Slides.Count; i++)
				{
					try
					{
						if (App.ActivePresentation.Slides.Item(i).NotesPage.Shapes.Placeholders.Item(index).PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody && App.ActivePresentation.Slides.Item(i).NotesPage.Shapes.Placeholders.Item(index).TextFrame.TextRange.Text.Length == 1)
						{
							num = GetVerseIndicator(App.ActivePresentation.Slides.Item(i).NotesPage.Shapes.Placeholders.Item(index).TextFrame.TextRange.Text);
							if (num >= 0)
							{
								if (num == 0)
								{
									if (!flag)
									{
										Slide[i, 0] = num;
										SongVerses[num] = i;
										flag = true;
									}
								}
								else if (num <= 12)
								{
									SongVerses[num] = i;
									Slide[i, 0] = num;
								}
								else
								{
									Slide[i, 0] = num;
								}
							}
						}
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

		public int GetVerseIndicator(string InString)
		{
			if (InString == null || InString == "")
			{
				return -1;
			}
			InString = InString.ToLower().Trim();
			try
			{
				int num = Convert.ToInt32(InString);
				if (num >= 0 && num <= 12)
				{
					return num;
				}
				return -1;
			}
			catch
			{
				if (InString.Length == 1)
				{
					try
					{
						switch (InString[0])
						{
						case 'c':
							return 0;
						case 'b':
							return 100;
						case 'e':
							return 101;
						case 't':
							return 102;
						case 'w':
							return 103;
						case 'p':
							return 111;
						case 'q':
							return 112;
						}
					}
					catch
					{
					}
				}
			}
			return -1;
		}

		public bool IsFileChanged(string FilePath)
		{
			bool result = true;

			String hashFileName = PP.GetMD5(FilePath);

			var hashFileInfo = new FileInfo(hashFileName);

            //파일 있는지 확인 있을때(true), 없으면(false)
            if (hashFileInfo.Exists)
            {
				result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

		/// <summary>
		/// daniel
		/// </summary>
		/// <param name="FilePath"></param>
		/// <param name="FilePrefix"></param>
		/// <returns></returns>
		public bool IsBuildedFileCheck(string FilePath, string FilePrefix)
        {
            bool result = true;

            var pptFileInfo = new FileInfo(FilePath);

            FileInfo jpgFileInfo = new FileInfo(FilePrefix + Convert.ToString(1) + ".jpg");
			//파일 있는지 확인 있을때(true), 없으면(false)
			Console.WriteLine(FilePrefix + Convert.ToString(1) + ".jpg");

			if (jpgFileInfo.Exists)
            {
                if (pptFileInfo.LastWriteTimeUtc < jpgFileInfo.LastWriteTimeUtc)
                {
                    Console.WriteLine(pptFileInfo.LastWriteTimeUtc.ToString() + "|" + jpgFileInfo.LastWriteTimeUtc);
                    return result;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

		private static string GetMD5(string path)
		{
			using (var fs = File.OpenRead(path))
			using (var md5 = new MD5CryptoServiceProvider())
				return string.Join("", md5.ComputeHash(fs).ToArray().Select(i => i.ToString("X2")));
		}

		/// <summary>
		/// daniel
		/// </summary>
		/// <param name="FilePath"></param>
		/// <param name="FilePrefix"></param>
		/// <param name="TotalSlides"></param>
		/// <param name="MAX_VERSES"></param>
		/// <param name="DB_MAXSLIDES"></param>
		/// <param name="SongVerses"></param>
		/// <param name="Slide"></param>
		/// <param name="SequenceSymbol"></param>
		/// <returns></returns>
		public bool BuildScreenDumps1(string FilePath, string FilePrefix, ref int TotalSlides, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			bool result = true;
			try
			{
				for (int i = 1; i < DB_MAXSLIDES; i++)
				{
					Slide[i, 0] = -1;
				}
				for (int j = 0; j <= MAX_VERSES; j++)
				{
					SongVerses[j] = 0;
				}
				if (App == null)
				{
					return result;
				}

				// daniel 2019/11/22
				//--------------------------------------------------------------------------------------------------------
				//if (IsFileChanged(FilePath))
				//{
				//	return true;
				//}

				Pres = App.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				Console.WriteLine(FilePath);
				TotalSlides = Pres.Slides.Count;
				if ((Pres == null) & (TotalSlides > 0))
				{
					return result;
				}

				if (!IsBuildedFileCheck(FilePath, FilePrefix))
				{
					for (int i = 1; i <= TotalSlides; i++)
					{
						Console.WriteLine(FilePrefix + Convert.ToString(i) + ".jpg");
						Pres.Slides.Item(i).Export(FilePrefix + Convert.ToString(i) + ".jpg", "JPG", 400, 300);
						//Console.WriteLine("SlideID:{0}, SlideNumber: {1} ", Pres.Slides.Item(l).SlideID, Pres.Slides.Item(l).SlideNumber);
					}
				}
				//else
				//{
				//	return result;
				//}

				//--------------------------------------------------------------------------------------------------------

				//String MD5HashString = PP.GetMD5(FilePath);
				//Console.WriteLine(MD5HashString);
				//System.IO.File.Create(MD5HashString);

				int index = 2;
				bool flag = false;
				SequenceSymbol.GetUpperBound(0);
				for (int i = 1; i <= TotalSlides; i++)
				{
					try
					{
						if (Pres.Slides.Item(i).NotesPage.Shapes.Placeholders.Item(index).PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody && Pres.Slides.Item(i).NotesPage.Shapes.Placeholders.Item(index).TextFrame.TextRange.Text.Length == 1)
						{
							int verseIndicator = GetVerseIndicator(Pres.Slides.Item(i).NotesPage.Shapes.Placeholders.Item(index).TextFrame.TextRange.Text);
							if (verseIndicator >= 0)
							{
								if (verseIndicator == 0)
								{
									if (!flag)
									{
										Slide[i, 0] = verseIndicator;
										SongVerses[verseIndicator] = i;
										flag = true;
									}
								}
								else if (verseIndicator <= 12)
								{
									SongVerses[verseIndicator] = i;
									Slide[i, 0] = verseIndicator;
								}
								else
								{
									Slide[i, 0] = verseIndicator;
								}
							}
						}
					}
					catch
					{
					}
				}
				return result;
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
			finally
			{
				if (Pres != null)
				{
					Pres.Close();
				}
				Pres = null;
			}
		}
		
		public bool BuildScreenDumps(string FilePath, string FilePrefix, ref int TotalSlides, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			bool result = true;
			try
			{
				for (int i = 1; i < DB_MAXSLIDES; i++)
				{
					Slide[i, 0] = -1;
				}
				for (int j = 0; j <= MAX_VERSES; j++)
				{
					SongVerses[j] = 0;
				}
				if (App == null)
				{
					return result;
				}

				Pres = App.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				TotalSlides = Pres.Slides.Count;
				if ((Pres == null) & (TotalSlides > 0))
				{
					return result;
				}

				for (int k = 1; k <= TotalSlides; k++)
				{
					Pres.Slides.Item(k).Export(FilePrefix + Convert.ToString(k) + ".jpg", "JPG", 400, 300);
                    
                }
                int index = 2;
				bool flag = false;
				SequenceSymbol.GetUpperBound(0);
				for (int l = 1; l <= TotalSlides; l++)
				{
					try
					{
						if (Pres.Slides.Item(l).NotesPage.Shapes.Placeholders.Item(index).PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody && Pres.Slides.Item(l).NotesPage.Shapes.Placeholders.Item(index).TextFrame.TextRange.Text.Length == 1)
						{
							int verseIndicator = GetVerseIndicator(Pres.Slides.Item(l).NotesPage.Shapes.Placeholders.Item(index).TextFrame.TextRange.Text);
							if (verseIndicator >= 0)
							{
								if (verseIndicator == 0)
								{
									if (!flag)
									{
										Slide[l, 0] = verseIndicator;
										SongVerses[verseIndicator] = l;
										flag = true;
									}
								}
								else if (verseIndicator <= 12)
								{
									SongVerses[verseIndicator] = l;
									Slide[l, 0] = verseIndicator;
								}
								else
								{
									Slide[l, 0] = verseIndicator;
								}
							}
						}
                        //preViewEvent(l-1);
                    }
					catch
					{
					}
				}
				return result;
			}
			catch
			{
				return false;
			}
			finally
			{
				if (Pres != null)
				{
					Pres.Close();
				}
				Pres = null;
			}
		}

		public int StringToInt(string InValue)
		{
			bool minus1IfBlank = false;
			return StringToInt(InValue, minus1IfBlank);
		}

		public int StringToInt(string InValue, bool Minus1IfBlank)
		{
			try
			{
				return Convert.ToInt32(InValue);
			}
			catch
			{
				return Minus1IfBlank ? (-1) : 0;
			}
		}

		public bool BuildFirstScreenDump(string[] InFileList, int MaxFileCount, string DumpFolderPrefix)
		{
			bool result = true;
			try
			{
				if (App == null)
				{
					return result;
				}
				for (int i = 0; i < MaxFileCount; i++)
				{
					ScreenDumpFullPath[0, i] = InFileList[i];
					ScreenDumpFullPath[1, i] = DumpFolderPrefix + Path.GetFileNameWithoutExtension(InFileList[i]) + ".jpg";
				}
				return result;
			}
			catch
			{
				return false;
			}
			finally
			{
				if (Pres != null)
				{
					Pres = null;
				}
			}
		}

		public bool BuildOneFirstScreenDump(int PPNumbering)
		{
			int num = PPNumbering - 1;
			if (num < 0 || ScreenDumpFullPath[0, num] == null)
			{
				return false;
			}
			bool result = true;
			try
			{
				if (App == null)
				{
					return result;
				}
				Pres = App.Presentations.Open(ScreenDumpFullPath[0, num], MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				Pres.Slides.Item(1).Export(ScreenDumpFullPath[1, num], "JPG", 400, 300);
				Pres.Close();
				return result;
			}
			catch
			{
				return false;
			}
		}
	}
}
