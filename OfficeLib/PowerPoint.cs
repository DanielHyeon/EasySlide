using System;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using NetOffice.PowerPointApi;
using NetOffice.OfficeApi.Enums;
using Application = NetOffice.PowerPointApi.Application;
//using System.Runtime.InteropServices;
//using NetOffice.WordApi;
//using Microsoft.Office.Core;
//using Microsoft.Office.Interop.PowerPoint;
//using Application = Microsoft.Office.Interop.PowerPoint.Application;
//using NetOffice.PowerPointApi.Enums;
using System.Runtime.InteropServices;
using NetOffice.PowerPointApi.Enums;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;

namespace OfficeLib
{
	public delegate void PreviewEvent(int index);
	public delegate void PreViewToOutputEvent();


    public class PowerPoint
	{
		public PreviewEvent preViewEvent;

		public PreViewToOutputEvent preViewToOutputEvent;

		public Application prePowerPointApp;

        public _Presentation presentation;

		public string[,] ScreenDumpFullPath = new string[2, 10000];

		public bool isEditable = false;

		public bool isLive = false;

		//������Ʈ���� ���?�Ǵ� Screen Device Name
		public string displayName = "None";

        public void Init()
		{
			try
			{
				prePowerPointApp = new Application();
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
				_ = prePowerPointApp.Presentations.Count;
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
					for (int i = 1; i <= prePowerPointApp.Presentations.Count; i++)
					{
						if (flag)
						{
							break;
						}
						if (prePowerPointApp.Presentations[i].Name == text)
						{
							flag = true;
						}
					}
				}
				try
				{
					if (!flag)
					{

						Presentation presentation = prePowerPointApp.Presentations.Open(InPowerpointFullPath, MsoTriState.msoTrue, MsoTriState.msoTrue);
						PowerpointList[TotalPowerpointItems + 1, 0] = InPowerpointFullPath;
						PowerpointList[TotalPowerpointItems + 1, 1] = presentation.FullName;//prePowerPointApp.Presentations.Application.ActivePresentation.FullName;
																							//prePowerPointApp.WindowState = PpWindowState.ppWindowMinimized;
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
				prePowerPointApp.Presentations[InIndex].SlideShowSettings.Run();
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
				string version = prePowerPointApp.Version;

				SaveRegValue(version, @"PowerPoint\Options", "DisplayMonitor", displayName);

				text = GetPresentationName(InPowerpointFullPath, PowerpointList, TotalPowerpointItems);
				text2 = "";
				if (!(text != ""))
				{
					return "Cannot find opened Presentation: " + TotalPowerpointItems + ", " + InPowerpointFullPath;
				}

				for (int i = 1; i <= prePowerPointApp.Presentations.Count; i++)
				{
					string text3 = text2;
					text2 = text3 + i + "-" + prePowerPointApp.Presentations[i].Name + "(" + prePowerPointApp.Presentations[i].FullName + ") ";
					if (prePowerPointApp.Presentations[i].FullName.ToUpper() == text.ToUpper())
					{
						using SlideShowWindow window = prePowerPointApp.Presentations[i].SlideShowSettings.Run();

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
				prePowerPointApp.ActivePresentation.SlideShowWindow.View.First();
			}
			catch
			{
			}
		}

		public void Last()
		{
			try
			{
				prePowerPointApp.ActivePresentation.SlideShowWindow.View.Last();
			}
			catch
			{
			}
		}

		public void GotoSlide(int StartingSlide)
		{
			try
			{
				prePowerPointApp.ActivePresentation.SlideShowWindow.View.GotoSlide(StartingSlide);
			}
			catch
			{
			}
		}

		public int Count()
		{
			try
			{
				return prePowerPointApp.ActivePresentation.Slides.Count;
			}
			catch
			{
				return 0;
			}
		}


		public static void SaveRegValue(string Version, string Section, string Name, string Value)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Office\{Version}\{Section}"))
				{
					registryKey.SetValue(Name, Value, RegistryValueKind.String);
				}
			}
			catch
			{
			}
		}

		public static object GetRegValue(string Version, string Section, string Name, string defaultValue)
		{

			using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey($@"HKEY_CURRENT_USER\Software\Microsoft\Office\{Version}\{Section}"))
			{
				return registryKey.GetValue(Name, defaultValue);
			}
		}

        public void SetShowWindow(float Left, float Top, float Width, float Height)
        {
            try
            {
                prePowerPointApp.ActivePresentation.SlideShowWindow.Left = Left;
                prePowerPointApp.ActivePresentation.SlideShowWindow.Top = Top;
                prePowerPointApp.ActivePresentation.SlideShowWindow.Width = Width;
                prePowerPointApp.ActivePresentation.SlideShowWindow.Height = Height;
            }
            catch
            {
            }
        }

        public void ResSetAllShowWindows()
		{
			try
			{
				if (prePowerPointApp.Presentations.Count > 0)
				{
					for (int i = 1; i <= prePowerPointApp.Presentations.Count; i++)
					{
						try
						{
							prePowerPointApp.Presentations[i].SlideShowWindow.Left = 0f;
							prePowerPointApp.Presentations[i].SlideShowWindow.Top = 0f;
							prePowerPointApp.Presentations[i].SlideShowWindow.Width = 1f;
							prePowerPointApp.Presentations[i].SlideShowWindow.Height = 1f;
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

		/// <summary>
		/// daniel
		/// �� ���α׷����� �����?�Ŀ�����Ʈ�� ����
		/// �ٸ� �۾� ������ �� �Ŀ�����Ʈ ������ ���� ����
		/// </summary>
		/// <param name="PowerpointList"></param>
		/// <param name="TotalPowerpointItems"></param>
		/// <returns></returns>
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
				if(prePowerPointApp != null && prePowerPointApp.Presentations != null)
                {
					num2 = prePowerPointApp.Presentations.Count;
					if (TotalPowerpointItems > 0 && prePowerPointApp.Presentations.Count > 0)
					{
						for (int num4 = prePowerPointApp.Presentations.Count; num4 > 0; num4--)
						{
							try
							{
								prePowerPointApp.Presentations[num4].Close();
							}
							catch
							{ }
						}
					}
				}
			}
			catch
			{
				flag = true;
			}
			TotalPowerpointItems = 0;

			//daniel PowerPoint App ����
			QuitPowerPointApp(prePowerPointApp);

			return "Opened PP=" + num2 + ", Total On List=" + num + ", Numbers Closed=" + num3 + (flag ? ". Error When Closing" : ". No Error");
		}




		/// daniel 
		/// �Ŀ�����Ʈ Slide Show�� �ɶ� ������ â�� Minimized ���·� �ٲ۴�.
		/// </summary>
		public void NewApp()
		{
			try
			{
				prePowerPointApp = new Application();
				//prePowerPointApp.PresentationOpenEvent += PrePowerPointApp_PresentationOpenEvent;
				//prePowerPointApp.Activate();
				//prePowerPointApp.WindowState = PpWindowState.ppWindowMinimized;
			}
			catch
			{
			}
		}

		/// <summary>
		/// â�� �ּ�ȭ ���ش�.
		/// </summary>
		/// <param name="Pres"></param>
		//private void PrePowerPointApp_PresentationOpenEvent(Presentation Pres)
		//{
		//	if (isLive && !isEditable)
		//	{
		//		Pres.Application.WindowState = PpWindowState.ppWindowMinimized;
		//	}
		//	else if (!isLive && isEditable)
		//	{
		//		Pres.Application.WindowState = PpWindowState.ppWindowNormal;
		//	}
		//	else
		//	{
		//		Pres.Application.WindowState = PpWindowState.ppWindowNormal;
		//	}

		//	Pres.Application.PresentationOpenEvent -= PrePowerPointApp_PresentationOpenEvent;
		//	//prePowerPointApp.PresentationOpenEvent -= PrePowerPointApp_PresentationOpenEvent;
		//}

		/// <summary>
		/// ?�블?�릭 ??PowerPoint ?�라?�드??창을 ?�성?�하�??�니메이???�영?�을 ?�생?�는 메서??
		/// </summary>
		public void ActivateSlideShowAndTriggerClick(int slideNumber = -1)
		{
			try
			{
				if (prePowerPointApp == null)
				{
					return;
				}

				_Presentation activePresentation = prePowerPointApp.ActivePresentation;
				if (activePresentation != null && activePresentation.SlideShowWindow != null)
				{
					// ?�재 ?�라?�드 ?�인
					int currentSlide = activePresentation.SlideShowWindow.View.Slide.SlideIndex;

					// ?�라?�드 번호가 지?�되?�고, ?�재 ?�라?�드?� ?�른 경우?�만 ?�동
					if (slideNumber > 0)
					{
						activePresentation.SlideShowWindow.View.GotoSlide(slideNumber, MsoTriState.msoTrue);
						//Thread.Sleep(200);
					}

					activePresentation.SlideShowWindow.Activate();
					activePresentation.SlideShowWindow.View.GotoClick(0);
					activePresentation.SlideShowWindow.View.Next();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error activating SlideShow: {ex.Message}");
			}
		}

		public int ImplementPowerpointSlideMovement(ref int InCurSlide, int InCurMaxSlide, OfficeLibKeys InKey)
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

		public int ImplementPowerpointSlideMovement(ref int InCurSlide, int InCurMaxSlide, OfficeLibKeys InKey, int InSlideNo)
		{
			try
			{
				if (prePowerPointApp == null)
				{
					return 1;
				}

                _Presentation activePresentation = prePowerPointApp.ActivePresentation;                
            
                switch (InKey)
				{
					case OfficeLibKeys.Left:
                        activePresentation.SlideShowSettings.ShowPresenterView = MsoTriState.msoFalse;
                        activePresentation.SlideShowWindow.View.First();
                        InCurSlide = 1;
                        break;
					case OfficeLibKeys.Up:
						activePresentation.SlideShowWindow.View.Previous();
						InCurSlide = activePresentation.SlideShowWindow.View.Slide.SlideIndex;
                        break;
					case OfficeLibKeys.Down:
						activePresentation.SlideShowWindow.View.Next();
						if (activePresentation.SlideShowWindow.View.State == PpSlideShowState.ppSlideShowDone)
						{
							activePresentation.SlideShowWindow.View.Last();
                            InCurSlide = -1;
						}
						else
						{
							InCurSlide = activePresentation.SlideShowWindow.View.Slide.SlideIndex;
                        }
                        break;
					case OfficeLibKeys.Right:
						activePresentation.SlideShowWindow.View.Last();
						InCurSlide = InCurMaxSlide;
                        break;
					case OfficeLibKeys.None:
						activePresentation.SlideShowWindow.View.GotoSlide(InSlideNo, MsoTriState.msoFalse);                        
                        InCurSlide = InSlideNo;
                        break;
					case OfficeLibKeys.Space:

						if (InCurSlide == -1)
							return InCurSlide;

                        if (activePresentation.SlideShowWindow.View.State == PpSlideShowState.ppSlideShowDone)
						{
							//activePresentation.SlideShowWindow.View.Last();
                            InCurSlide = -1;
						}
						else
						{
							//int nIndex = activePresentation.SlideShowWindow.View.GetClickIndex();
							//int nCount = activePresentation.SlideShowWindow.View.GetClickCount();

							
                            //activePresentation.SlideShowWindow.View.GotoClick(0);
                            //int ClickCount = activePresentation.SlideShowWindow.View.GetClickCount();

                            activePresentation.SlideShowWindow.Activate();
                            activePresentation.SlideShowWindow.View.Next();

                            if (activePresentation.SlideShowWindow.View.State == PpSlideShowState.ppSlideShowDone)
							{
								InCurSlide = -1;
                            }
							else
							{
								if (InCurSlide != activePresentation.SlideShowWindow.View.Slide.SlideIndex)
								{
                                    InCurSlide = activePresentation.SlideShowWindow.View.Slide.SlideIndex;
                                }
                            }
                        }
                        break;
					case OfficeLibKeys.F5:
						activePresentation.SlideShowWindow.View.GotoSlide(InCurSlide, MsoTriState.msoFalse);
                        break;
				}
            }
			catch
			{
				if(preViewToOutputEvent != null)
					preViewToOutputEvent();
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
				for (int i = 1; i <= prePowerPointApp.ActivePresentation.Slides.Count; i++)
				{
					try
					{
						if (prePowerPointApp.ActivePresentation.Slides[i].NotesPage.Shapes.Placeholders[index].PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody && prePowerPointApp.ActivePresentation.Slides[i].NotesPage.Shapes.Placeholders[index].TextFrame.TextRange.Text.Length == 1)
						{
							num = GetVerseIndicator(prePowerPointApp.ActivePresentation.Slides[i].NotesPage.Shapes.Placeholders[index].TextFrame.TextRange.Text);
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

			String hashFileName = PowerPoint.GetMD5(FilePath);

			var hashFileInfo = new FileInfo(hashFileName);

			//���� �ִ��� Ȯ�� ������(true), ������(false)
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
		public bool IsBuildedFileCheck(string FilePath, string FilePrefix, ref int TotalSlides)
		{
			bool result = true;

			if (string.IsNullOrEmpty(FilePath))
				return false;

			var pptFileInfo = new FileInfo(FilePath);

			FileInfo jpgFileInfo = new FileInfo(FilePrefix + Convert.ToString(1) + ".jpg");
			//���� �ִ��� Ȯ�� ������(true), ������(false)
			Console.WriteLine(FilePrefix + Convert.ToString(1) + ".jpg");

			if (jpgFileInfo.Exists)
			{
				if (pptFileInfo.LastWriteTimeUtc < jpgFileInfo.LastWriteTimeUtc)
				{
					Console.WriteLine(pptFileInfo.LastWriteTimeUtc.ToString() + "|" + jpgFileInfo.LastWriteTimeUtc);
					_Presentation presentation = null;
					try
					{
						presentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
						TotalSlides = presentation.Slides.Count;
					}
					catch
					{
						result = false;
					}
                    finally
                    {
                        if (presentation != null)
                            ClosePresentation(ref presentation);
                    }

                    result = true;
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

		public bool IsBuildedFileCheck(_Presentation presentation, string FilePath, string FilePrefix, ref int TotalSlides)
		{
			bool result = true;
			bool isJpgFileExist = true;

			if (string.IsNullOrEmpty(FilePath))
				return false;

			int checkSlides = TotalSlides > 2 ? 2 : TotalSlides;			

			for (int i = 1; i <= checkSlides; i++)
			{
				FileInfo jpgFileInfo = new FileInfo(FilePrefix + Convert.ToString(i) + ".jpg");
				Console.WriteLine(FilePrefix + Convert.ToString(i) + ".jpg");
				if (!jpgFileInfo.Exists)
				{
					isJpgFileExist = false;
					break;
				}
				//���� �ִ��� Ȯ�� ������(true), ������(false)
			}

			if (isJpgFileExist)
			{
				var pptFileInfo = new FileInfo(FilePath);
				FileInfo jpgFileInfo = new FileInfo(FilePrefix + Convert.ToString(1) + ".jpg");

				if (pptFileInfo.LastWriteTimeUtc < jpgFileInfo.LastWriteTimeUtc)
				{
					Console.WriteLine(pptFileInfo.LastWriteTimeUtc.ToString() + "|" + jpgFileInfo.LastWriteTimeUtc);
					try
					{
						TotalSlides = presentation.Slides.Count;
					}
					catch
					{
						result = false;
					}
					result = true;
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

		public bool BuildScreenPreDumps(string FilePath, string FilePrefix, ref int TotalSlides, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			bool result = true;
			try
			{
				//for (int i = 1; i < DB_MAXSLIDES; i++)
				//{
				//	Slide[i, 0] = -1;
				//}
				//for (int j = 0; j <= MAX_VERSES; j++)
				//{
				//	SongVerses[j] = 0;
				//}
				//if (App == null)
				//{
				//	return result;
				//}
				try
				{
					/// ����Slide ī��Ʈ, SongVerses ī��Ʈ ��ŭ�� �ʱ�ȭ
					for (int i = 1; i < TotalSlides; i++)
					{
						Slide[i, 0] = -1;
					}

					for (int j = 0; j <= MAX_VERSES; j++)
					{
						SongVerses[j] = 0;
					}
				} 
				catch (Exception)
				{ }

				if (prePowerPointApp == null)
				{
					return result;
				}

				// daniel 2019/11/22
				//--------------------------------------------------------------------------------------------------------
				//if (IsFileChanged(FilePath))
				//{
				//	return true;
				//}

				presentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				Console.WriteLine(FilePath);
				if ((presentation == null) & (TotalSlides > 0))
				{
					return result;
				}
				TotalSlides = presentation.Slides.Count;
				String strPreFileName = "";
				String strOutFileName = "";


				if (!IsBuildedFileCheck(presentation, FilePath, FilePrefix, ref TotalSlides))
				{
					for (int i = 1; i <= TotalSlides; i++)
					{
						strPreFileName = FilePrefix + Convert.ToString(i) + ".jpg";
						presentation.Slides[i].Export(strPreFileName, "JPG", 640, 480);
						//Console.WriteLine("SlideID:{0}, SlideNumber: {1} ", Pres.Slides.Item(l).SlideID, Pres.Slides.Item(l).SlideNumber);
						strOutFileName = strPreFileName.Replace("~PREPPPreview$", "~OUTPPPreview$");
						File.Copy(strPreFileName, strOutFileName, true);
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
						if (presentation.Slides[i].NotesPage.Shapes.Placeholders[index].PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody && presentation.Slides[i].NotesPage.Shapes.Placeholders[index].TextFrame.TextRange.Text.Length == 1)
						{
							int verseIndicator = GetVerseIndicator(presentation.Slides[i].NotesPage.Shapes.Placeholders[index].TextFrame.TextRange.Text);
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
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
			finally
			{
				ClosePresentation(ref presentation);
			}
		}

		public bool BuildScreenOutDumps(string FilePath, string FilePrefix, ref int TotalSlides, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			bool result = true;
			try
			{
				//for (int i = 1; i < DB_MAXSLIDES; i++)
				//{
				//	Slide[i, 0] = -1;
				//}
				//for (int j = 0; j <= MAX_VERSES; j++)
				//{
				//	SongVerses[j] = 0;
				//}
				//if (App == null)
				//{
				//	return result;
				//}

				for (int i = 1; i < TotalSlides; i++)
				{
					Slide[i, 0] = -1;
				}
				for (int j = 0; j <= TotalSlides; j++)
				{
					SongVerses[j] = 0;
				}
				if (prePowerPointApp == null)
				{
					return result;
				}

				// daniel 2019/11/22
				//--------------------------------------------------------------------------------------------------------
				//if (IsFileChanged(FilePath))
				//{
				//	return true;
				//}

				presentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				Console.WriteLine(FilePath);
				if ((presentation == null) & (TotalSlides > 0))
				{
					return result;
				}
				TotalSlides = presentation.Slides.Count;
				String strPreFileName = "";
				String strOutFileName = "";


				if (!IsBuildedFileCheck(presentation, FilePath, FilePrefix, ref TotalSlides))
				{
					for (int i = 1; i <= TotalSlides; i++)
					{
						strOutFileName = FilePrefix + Convert.ToString(i) + ".jpg";
						presentation.Slides[i].Export(strOutFileName, "JPG", 640, 480);
						//Console.WriteLine("SlideID:{0}, SlideNumber: {1} ", Pres.Slides.Item(l).SlideID, Pres.Slides.Item(l).SlideNumber);
						strPreFileName = strOutFileName.Replace("~OUTPPPreview$", "~PREPPPreview$");
						File.Copy(strOutFileName, strPreFileName, true);
					}

					//for (int i = 1; i <= TotalSlides; i++)
					//{
					//	strPreFileName = FilePrefix + Convert.ToString(i) + ".jpg";
					//	presentation.Slides[i].Export(strPreFileName, "JPG", 400, 300);
					//	//Console.WriteLine("SlideID:{0}, SlideNumber: {1} ", Pres.Slides.Item(l).SlideID, Pres.Slides.Item(l).SlideNumber);
					//	strOutFileName = strPreFileName.Replace("~PREPPPreview$", "~OUTPPPreview$");
					//	File.Copy(strPreFileName, strOutFileName, true);
					//}
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
						if (presentation.Slides[i].NotesPage.Shapes.Placeholders[index].PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody && presentation.Slides[i].NotesPage.Shapes.Placeholders[index].TextFrame.TextRange.Text.Length == 1)
						{
							int verseIndicator = GetVerseIndicator(presentation.Slides[i].NotesPage.Shapes.Placeholders[index].TextFrame.TextRange.Text);
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
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
			finally
			{
				ClosePresentation(ref presentation);
			}
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
				//for (int i = 1; i < DB_MAXSLIDES; i++)
				//{
				//	Slide[i, 0] = -1;
				//}
				//for (int j = 0; j <= MAX_VERSES; j++)
				//{
				//	SongVerses[j] = 0;
				//}
				//if (App == null)
				//{
				//	return result;
				//}

				for (int i = 1; i < TotalSlides; i++)
				{
					Slide[i, 0] = -1;
				}
				for (int j = 0; j <= TotalSlides; j++)
				{
					SongVerses[j] = 0;
				}
				if (prePowerPointApp == null)
				{
					return result;
				}

				// daniel 2019/11/22
				//--------------------------------------------------------------------------------------------------------
				//if (IsFileChanged(FilePath))
				//{
				//	return true;
				//}

				presentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				Console.WriteLine(FilePath);
				if ((presentation == null) & (TotalSlides > 0))
				{
					return result;
				}
				TotalSlides = presentation.Slides.Count;
				String strFileName = "";

				if (!IsBuildedFileCheck(FilePath, FilePrefix, ref TotalSlides))
				{
					for (int i = 1; i <= TotalSlides; i++)
					{
						strFileName = FilePrefix + Convert.ToString(i) + ".jpg";
						presentation.Slides[i].Export(strFileName, "JPG", 640, 480);
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
						if (presentation.Slides[i].NotesPage.Shapes.Placeholders[index].PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody && presentation.Slides[i].NotesPage.Shapes.Placeholders[index].TextFrame.TextRange.Text.Length == 1)
						{
							int verseIndicator = GetVerseIndicator(presentation.Slides[i].NotesPage.Shapes.Placeholders[index].TextFrame.TextRange.Text);
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
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				return false;
			}
			finally
			{
				ClosePresentation(ref presentation);
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
				if (prePowerPointApp == null)
				{
					return result;
				}

				presentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				TotalSlides = presentation.Slides.Count;
				if ((presentation == null) & (TotalSlides > 0))
				{
					return result;
				}

				for (int k = 1; k <= TotalSlides; k++)
				{
					presentation.Slides[k].Export(FilePrefix + Convert.ToString(k) + ".jpg", "JPG", 640, 480);
				}
				int index = 2;
				bool flag = false;
				SequenceSymbol.GetUpperBound(0);
				for (int i = 1; i <= TotalSlides; i++)
				{
					try
					{
						if (presentation.Slides[i].NotesPage.Shapes.Placeholders[index].PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody && presentation.Slides[i].NotesPage.Shapes.Placeholders[index].TextFrame.TextRange.Text.Length == 1)
						{
							int verseIndicator = GetVerseIndicator(presentation.Slides[i].NotesPage.Shapes.Placeholders[index].TextFrame.TextRange.Text);
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
				ClosePresentation(ref presentation);
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
				if (prePowerPointApp == null)
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
				ClosePresentation(ref presentation);
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
				if (prePowerPointApp == null)
				{
					return result;
				}
				presentation = prePowerPointApp.Presentations.Open(ScreenDumpFullPath[0, num], MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				presentation.Slides[1].Export(ScreenDumpFullPath[1, num], "JPG", 640, 480);

				return result;
			}
			catch
			{
				return false;
			}
			finally
            {
				ClosePresentation(ref presentation);
			}
		}

        public void ClosePresentation(ref _Presentation presentation)
        {
            try
            {
                if (presentation != null)
                {
                    presentation.Close();
                    presentation.Dispose();  // NetOffice���� �����ϴ� Dispose() ���?
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error closing presentation: {e.Message}");
            }
            finally
            {
                presentation = null;
            }
        }

        public void QuitPowerPointApp(Application prePowerPointApp)
        {
            try
            {
                if (prePowerPointApp != null && !prePowerPointApp.IsDisposed)
                {
                    prePowerPointApp.Quit();
                    prePowerPointApp.Dispose();  // Dispose()�� Quit() ���Ŀ� ȣ��
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("PowerPoint application was already disposed.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error quitting PowerPoint: {e.Message}");
                ForceKillPowerPoint();
            }
            finally
            {
                prePowerPointApp = null;
            }
        }

        private void ForceKillPowerPoint()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("POWERPNT"))
                {
                    process.Kill();
                }
                Console.WriteLine("PowerPoint process was forcibly terminated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error killing PowerPoint process: {ex.Message}");
            }
        }

        public void ClosePresentation_Old(ref _Presentation presentation)
		{
			try
			{
				if (presentation != null)
				{
					presentation.Close();

					presentation.Dispose(true);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
			finally
			{
				presentation = null;
			}
		}

		public void QuitPowerPointApp_Old()
		{
			try
			{
				if (prePowerPointApp != null)
				{
                    prePowerPointApp.Quit();

                    prePowerPointApp.Dispose();
                    prePowerPointApp = null;
                }
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
			finally
			{
				if (prePowerPointApp != null)
				{
					prePowerPointApp.Dispose();
					prePowerPointApp = null;
				}
			}
		}
	}
}

