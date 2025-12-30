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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace OfficeLib
{
	public delegate void PreviewEvent(int index);
	public delegate void PreViewToOutputEvent();


    public class PowerPoint
	{
		// 상수 정의
		private const int NOTES_PLACEHOLDER_INDEX = 2;
		private const int EXPORT_WIDTH = 640;
		private const int EXPORT_HEIGHT = 480;
		private const int MAX_VERSE_NUMBER = 12;
		private const int VERSE_CHORUS = 0;
		private const int VERSE_BRIDGE = 100;
		private const int VERSE_ENDING = 101;
		private const int VERSE_TAG = 102;
		private const int VERSE_WORSHIP = 103;
		private const int VERSE_PRAYER = 111;
		private const int VERSE_QUIET = 112;

		public PreviewEvent preViewEvent;

		public PreViewToOutputEvent preViewToOutputEvent;

		public Application prePowerPointApp;

        public _Presentation presentation;

		public string[,] ScreenDumpFullPath = new string[2, 10000];

		public bool isEditable = false;

		public bool isLive = false;

		//������Ʈ���� ���?�Ǵ� Screen Device Name
		public string displayName = "None";

		// ✅ 캐싱: 파일 체크 결과 저장 (성능 최적화)
		private Dictionary<string, (bool Exists, DateTime LastCheck, int TotalSlides)> _fileCheckCache = new Dictionary<string, (bool, DateTime, int)>();
		private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public void Init()
		{
			try
			{
				prePowerPointApp = new Application();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error initializing PowerPoint Application: {ex.Message}");
				throw;
			}
		}

		public string Open(string InPowerpointFullPath, ref string[,] PowerpointList, ref int TotalPowerpointItems)
		{
			string text = "";
			Presentation presentation = null;

			try
			{
				_ = prePowerPointApp.Presentations.Count;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error accessing Presentations: {ex.Message}");
				return "Error: PowerPoint application not available";
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
						presentation = prePowerPointApp.Presentations.Open(InPowerpointFullPath, MsoTriState.msoTrue, MsoTriState.msoTrue);
						if (presentation != null)
						{
							PowerpointList[TotalPowerpointItems + 1, 0] = InPowerpointFullPath;
							PowerpointList[TotalPowerpointItems + 1, 1] = presentation.FullName;
							TotalPowerpointItems++;
							// presentation은 Presentations 컬렉션에서 관리되므로 여기서는 Dispose하지 않음
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error opening presentation '{InPowerpointFullPath}': {ex.Message}");
					// 에러 발생 시 열린 presentation 정리
					if (presentation != null)
					{
						try
						{
							presentation.Close();
							presentation.Dispose();
						}
						catch { }
					}
					return "Error Loading: " + text;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in Open method: {ex.Message}");
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
			catch (Exception ex)
			{
				Console.WriteLine($"Error running presentation at index {InIndex}: {ex.Message}");
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
					if (string.Equals(prePowerPointApp.Presentations[i].FullName, text, StringComparison.OrdinalIgnoreCase))
					{
						using SlideShowWindow window = prePowerPointApp.Presentations[i].SlideShowSettings.Run();

						return "Presentation Run OK";
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error running presentation: {ex.Message}");
				return "Error: " + TotalPowerpointItems + ", " + InPowerpointFullPath;
			}
			return "Can't find Presentations: " + text + ".  " + text2;
		}

		public string GetPresentationName(string InPowerpointFullPath, string[,] PowerpointList, int TotalPowerpointItems)
		{
			if (string.IsNullOrEmpty(InPowerpointFullPath))
			{
				Console.WriteLine("GetPresentationName: InPowerpointFullPath is null or empty");
				return "";
			}

			if (PowerpointList == null)
			{
				Console.WriteLine("GetPresentationName: PowerpointList is null");
				return "";
			}

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
					catch (Exception ex)
					{
						Console.WriteLine($"Error accessing PowerpointList at index {i}: {ex.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in GetPresentationName: {ex.Message}");
			}
			return "";
		}

		public void First()
		{
			try
			{
				prePowerPointApp.ActivePresentation.SlideShowWindow.View.First();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error going to first slide: {ex.Message}");
			}
		}

		public void Last()
		{
			try
			{
				prePowerPointApp.ActivePresentation.SlideShowWindow.View.Last();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error going to last slide: {ex.Message}");
			}
		}

		public void GotoSlide(int StartingSlide)
		{
			if (StartingSlide <= 0)
			{
				Console.WriteLine($"GotoSlide: Invalid slide number {StartingSlide}");
				return;
			}

			try
			{
				prePowerPointApp.ActivePresentation.SlideShowWindow.View.GotoSlide(StartingSlide);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error going to slide {StartingSlide}: {ex.Message}");
			}
		}

		public int Count()
		{
			try
			{
				return prePowerPointApp.ActivePresentation.Slides.Count;
			}
			catch (Exception)
		{
			// ActivePresentation이 없거나 접근 불가 시 0 반환
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
			catch (Exception ex)
			{
				Console.WriteLine($"Error saving registry value {Name}: {ex.Message}");
			}
		}

		public static object GetRegValue(string Version, string Section, string Name, string defaultValue)
		{
			try
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey($@"Software\Microsoft\Office\{Version}\{Section}"))
				{
					return registryKey.GetValue(Name, defaultValue);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting registry value {Name}: {ex.Message}");
				return defaultValue;
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
            catch (Exception)
            {
				// 예외 무시
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
						catch (Exception)
            {
				// 예외 무시
            }
					}
				}
			}
			catch (Exception)
            {
				// 예외 무시
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
			}
			catch (Exception)
            {
				// 예외 무시
            }
		}

        private DateTime lastClickTime = DateTime.MinValue;

        public void SafePlayNext(int slideNumber = -1)
        {
            var activePresentation = prePowerPointApp.ActivePresentation;
            if (activePresentation == null || activePresentation.SlideShowWindow == null) return;

            // 1. 디바운스 (유지)
            if ((DateTime.Now - lastClickTime).TotalMilliseconds < 500) return;
            lastClickTime = DateTime.Now;

            try
            {
                var view = activePresentation.SlideShowWindow.View;

                // [핵심 수정 사항] 
                // 입력된 번호가 유효하고(>0), '현재 보고 있는 슬라이드'와 '다를 때만' 이동합니다.
                // 같을 때 이동하면 애니메이션이 리셋되기 때문입니다.
                if (slideNumber > 0 && view.Slide.SlideIndex != slideNumber)
                {
                    // 다른 슬라이드로 이동할 때만 실행
                    view.GotoSlide(slideNumber, MsoTriState.msoTrue);
                }

                activePresentation.SlideShowWindow.Activate();

                // 현재 상태 다시 조회 (GotoSlide를 했을 수도 있으므로 여기서 조회)
                int totalClicks = view.Slide.TimeLine.MainSequence.Count;
                int currentIndex = view.GetClickIndex();

                // 2. 애니메이션 실행 로직
                if (currentIndex < totalClicks)
                {
                    view.Next();
                    System.Diagnostics.Debug.WriteLine($"Playing animation {currentIndex + 1} of {totalClicks}");
                }
                else
                {
                    // 3. 애니메이션 끝난 후 처리
                    System.Diagnostics.Debug.WriteLine("Animation finished. Resetting loop.");

                    // 만약 '다음 슬라이드'로 자동으로 넘어가길 원하시면:
                    // view.Next(); 

                    // 현재 로직 유지 (반복 재생):
                    view.GotoClick(0);
                    view.Next();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
            }
        }
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
                
                // SlideShowWindow가 존재하는지 확인 (슬라이드쇼가 실행 중이어야 함)
                SlideShowWindow slideShowWindow = null;
                try
                {
                    slideShowWindow = activePresentation.SlideShowWindow;
                }
                catch (COMException)
                {
                    // 슬라이드쇼가 실행 중이 아니면 현재 슬라이드 번호를 그대로 반환
                    return InCurSlide;
                }
                
                if (slideShowWindow == null)
                {
                    return InCurSlide;
                }
            
                switch (InKey)
				{
					case OfficeLibKeys.Left:
                        activePresentation.SlideShowSettings.ShowPresenterView = MsoTriState.msoFalse;
                        slideShowWindow.View.First();
                        InCurSlide = 1;
                        break;
					case OfficeLibKeys.Up:
						slideShowWindow.View.Previous();
						InCurSlide = slideShowWindow.View.Slide.SlideIndex;
                        break;
					case OfficeLibKeys.Down:
						slideShowWindow.View.Next();
						if (slideShowWindow.View.State == PpSlideShowState.ppSlideShowDone)
						{
							slideShowWindow.View.Last();
                            InCurSlide = -1;
						}
						else
						{
							InCurSlide = slideShowWindow.View.Slide.SlideIndex;
                        }
                        break;
					case OfficeLibKeys.Right:
						slideShowWindow.View.Last();
						InCurSlide = InCurMaxSlide;
                        break;
					case OfficeLibKeys.None:
						slideShowWindow.View.GotoSlide(InSlideNo, MsoTriState.msoFalse);                        
                        InCurSlide = InSlideNo;
                        break;
					case OfficeLibKeys.Space:

						if (InCurSlide == -1)
							return InCurSlide;

                        if (slideShowWindow.View.State == PpSlideShowState.ppSlideShowDone)
						{
							//slideShowWindow.View.Last();
                            InCurSlide = -1;
						}
						else
						{

                            slideShowWindow.Activate();
                            slideShowWindow.View.Next();

                            if (slideShowWindow.View.State == PpSlideShowState.ppSlideShowDone)
							{
								InCurSlide = -1;
                            }
							else
							{
								if (InCurSlide != slideShowWindow.View.Slide.SlideIndex)
								{
                                    InCurSlide = slideShowWindow.View.Slide.SlideIndex;
                                }
                            }
                        }
                        break;
					case OfficeLibKeys.F5:
						slideShowWindow.View.GotoSlide(InCurSlide, MsoTriState.msoFalse);
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
				int num = -1;
				bool flag = false;
				SequenceSymbol.GetUpperBound(0);
				for (int i = 1; i <= prePowerPointApp.ActivePresentation.Slides.Count; i++)
				{
					try
					{
						if (prePowerPointApp.ActivePresentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody &&
							prePowerPointApp.ActivePresentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].TextFrame.TextRange.Text.Length == 1)
						{
							num = GetVerseIndicator(prePowerPointApp.ActivePresentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].TextFrame.TextRange.Text);
							if (num >= 0)
							{
								if (num == VERSE_CHORUS)
								{
									if (!flag)
									{
										Slide[i, 0] = num;
										SongVerses[num] = i;
										flag = true;
									}
								}
								else if (num <= MAX_VERSE_NUMBER)
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
					catch (Exception ex)
					{
						Console.WriteLine($"Error loading verse for slide {i}: {ex.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in LoadVersesAndSlides: {ex.Message}");
			}
		}

		public int GetVerseIndicator(string InString)
		{
			if (string.IsNullOrEmpty(InString))
			{
				return -1;
			}
			InString = InString.ToLower().Trim();
			try
			{
				int num = Convert.ToInt32(InString);
				if (num >= 0 && num <= MAX_VERSE_NUMBER)
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
								return VERSE_CHORUS;
							case 'b':
								return VERSE_BRIDGE;
							case 'e':
								return VERSE_ENDING;
							case 't':
								return VERSE_TAG;
							case 'w':
								return VERSE_WORSHIP;
							case 'p':
								return VERSE_PRAYER;
							case 'q':
								return VERSE_QUIET;
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error parsing verse character '{InString}': {ex.Message}");
					}
				}
			}
			return -1;
		}

		/// <summary>
		/// 프레젠테이션에서 Verse 정보를 파싱하여 배열에 저장
		/// </summary>
		private void ParseVersesFromPresentation(_Presentation presentation, int totalSlides, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			bool flag = false;
			SequenceSymbol.GetUpperBound(0);
			for (int i = 1; i <= totalSlides; i++)
			{
				try
				{
					if (presentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].PlaceholderFormat.Type == PpPlaceholderType.ppPlaceholderBody &&
						presentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].TextFrame.TextRange.Text.Length == 1)
					{
						int verseIndicator = GetVerseIndicator(presentation.Slides[i].NotesPage.Shapes.Placeholders[NOTES_PLACEHOLDER_INDEX].TextFrame.TextRange.Text);
						if (verseIndicator >= 0)
						{
							if (verseIndicator == VERSE_CHORUS)
							{
								if (!flag)
								{
									Slide[i, 0] = verseIndicator;
									SongVerses[verseIndicator] = i;
									flag = true;
								}
							}
							else if (verseIndicator <= MAX_VERSE_NUMBER)
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
				catch (Exception ex)
				{
					Console.WriteLine($"Error processing verse for slide {i}: {ex.Message}");
				}
			}
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
		/// JPG 파일이 이미 빌드되었는지 확인
		/// </summary>
		/// <param name="FilePath">PowerPoint 파일 경로</param>
		/// <param name="FilePrefix">JPG 파일 접두사</param>
		/// <param name="TotalSlides">총 슬라이드 수 (출력)</param>
		/// <returns>true: 이미 빌드됨(재빌드 불필요), false: 재빌드 필요</returns>
		public bool IsBuildedFileCheck(string FilePath, string FilePrefix, ref int TotalSlides, _Presentation presentation = null)
	{
		if (string.IsNullOrEmpty(FilePath))
		{
			Console.WriteLine("FilePath is null or empty");
			return false;
		}

		var pptFileInfo = new FileInfo(FilePath);
		if (!pptFileInfo.Exists)
		{
			Console.WriteLine($"PPT file not found: {FilePath}");
			return false;
		}

		Console.WriteLine($"[DEBUG] PPT LastWriteTimeUtc: {pptFileInfo.LastWriteTimeUtc} ({pptFileInfo.LastWriteTimeUtc.Ticks})");

		// ✅ 캐싱: 파일 수정 시간을 포함한 캐시 키 생성
		string cacheKey = FilePath + "|" + FilePrefix + "|" + pptFileInfo.LastWriteTimeUtc.Ticks;
		if (_fileCheckCache.ContainsKey(cacheKey))
		{
			var cached = _fileCheckCache[cacheKey];
			if (DateTime.Now - cached.LastCheck < _cacheExpiration)
			{
				TotalSlides = cached.TotalSlides;
				Console.WriteLine($"[Cache Hit] File check: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
				return cached.Exists;
			}
		}

		// 이전 캐시 키 패턴과 일치하는 항목 제거 (파일이 수정되었으므로)
		var oldCacheKeys = _fileCheckCache.Keys.Where(k => k.StartsWith(FilePath + "|" + FilePrefix + "|")).ToList();
		foreach (var oldKey in oldCacheKeys)
		{
			if (oldKey != cacheKey)
			{
				_fileCheckCache.Remove(oldKey);
			}
		}

		FileInfo jpgFileInfo = new FileInfo(FilePrefix + Convert.ToString(1) + ".jpg");
		Console.WriteLine($"Checking JPG: {jpgFileInfo.FullName}");

		// JPG 파일이 없으면 재빌드 필요
		if (!jpgFileInfo.Exists)
		{
			Console.WriteLine("JPG file not found - rebuild required");
			
			// 슬라이드 수를 가져오기 위해 프레젠테이션 열기 (필요한 경우)
			if (TotalSlides == 0 || presentation == null)
			{
				_Presentation tempPresentation = presentation;
				bool shouldClose = false;
				try
				{
					if (tempPresentation == null)
					{
						tempPresentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
						shouldClose = true;
					}
					TotalSlides = tempPresentation.Slides.Count;
					Console.WriteLine($"Presentation opened for slide count: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error opening presentation to get slide count: {ex.Message}");
				}
				finally
				{
					if (shouldClose && tempPresentation != null)
						ClosePresentation(ref tempPresentation);
				}
			}
			
			_fileCheckCache[cacheKey] = (false, DateTime.Now, TotalSlides);
			return false;
		}

		// JPG가 PPT보다 최신이거나 같으면 이미 빌드됨
		Console.WriteLine($"[DEBUG] Comparing times - JPG: {jpgFileInfo.LastWriteTimeUtc} ({jpgFileInfo.LastWriteTimeUtc.Ticks}), PPT: {pptFileInfo.LastWriteTimeUtc} ({pptFileInfo.LastWriteTimeUtc.Ticks})");
		if (jpgFileInfo.LastWriteTimeUtc >= pptFileInfo.LastWriteTimeUtc)
		{
			Console.WriteLine($"JPG is newer than or equal to PPT - already built (PPT: {pptFileInfo.LastWriteTimeUtc}, JPG: {jpgFileInfo.LastWriteTimeUtc})");
			
			// 슬라이드 수를 가져오기 위해 프레젠테이션 열기 (필요한 경우)
			if (TotalSlides == 0 || presentation == null)
			{
				_Presentation tempPresentation = presentation;
				bool shouldClose = false;
				try
				{
					if (tempPresentation == null)
					{
						tempPresentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
						shouldClose = true;
					}
					TotalSlides = tempPresentation.Slides.Count;
					Console.WriteLine($"Presentation opened for slide count: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error opening presentation to get slide count: {ex.Message}");
					_fileCheckCache[cacheKey] = (false, DateTime.Now, TotalSlides);
					return false;
				}
				finally
				{
					if (shouldClose && tempPresentation != null)
						ClosePresentation(ref tempPresentation);
				}
			}
			
			_fileCheckCache[cacheKey] = (true, DateTime.Now, TotalSlides);
			Console.WriteLine($"[Cache Store] Already built: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
			return true;
		}
		else
		{
			// PPT가 JPG보다 최신이면 재빌드 필요
			Console.WriteLine($"PPT is newer than JPG - rebuild required (PPT: {pptFileInfo.LastWriteTimeUtc}, JPG: {jpgFileInfo.LastWriteTimeUtc})");
			
			// 슬라이드 수를 가져오기 위해 프레젠테이션 열기 (필요한 경우)
			if (TotalSlides == 0 || presentation == null)
			{
				_Presentation tempPresentation = presentation;
				bool shouldClose = false;
				try
				{
					if (tempPresentation == null)
					{
						tempPresentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
						shouldClose = true;
					}
					TotalSlides = tempPresentation.Slides.Count;
					Console.WriteLine($"Presentation opened for slide count: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error opening presentation to get slide count: {ex.Message}");
				}
				finally
				{
					if (shouldClose && tempPresentation != null)
						ClosePresentation(ref tempPresentation);
				}
			}
			
			_fileCheckCache[cacheKey] = (false, DateTime.Now, TotalSlides);
			return false;
		}
	}



		private static string GetMD5(string path)
		{
			using (var fs = File.OpenRead(path))
			using (var md5 = new MD5CryptoServiceProvider())
				return string.Join("", md5.ComputeHash(fs).ToArray().Select(i => i.ToString("X2")));
		}

		/// <summary>
		/// 비동기 버전의 BuildScreenPreDumps - UI 블로킹 방지
		/// </summary>
		public bool BuildScreenOutDumps(string FilePath, string FilePrefix, ref int TotalSlides, int MAX_VERSES, int DB_MAXSLIDES, ref int[] SongVerses, ref int[,] Slide, string[] SequenceSymbol)
		{
			bool result = true;
			try
			{
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

				// ✅ 먼저 캐시 체크 (프레젠테이션을 열기 전에)
			Console.WriteLine($"[DEBUG BuildScreenOutDumps] Calling IsBuildedFileCheck...");
			bool isBuilt = IsBuildedFileCheck(FilePath, FilePrefix, ref TotalSlides);
			Console.WriteLine($"[DEBUG BuildScreenOutDumps] IsBuildedFileCheck returned: {isBuilt}");
			if (isBuilt)
			{
				Console.WriteLine($"Output preview images already exist (cache hit) for: {FilePrefix}");
					
					// 캐시 히트: Verse 정보 파싱을 위해서만 프레젠테이션 열기
					presentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoTrue, MsoTriState.msoFalse);
					Console.WriteLine(FilePath);
					if (presentation != null)
					{
						ParseVersesFromPresentation(presentation, TotalSlides, ref SongVerses, ref Slide, SequenceSymbol);
					}
					return result;
				}

				// 캐시 미스: 이미지를 새로 생성해야 함
				presentation = prePowerPointApp.Presentations.Open(FilePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
				Console.WriteLine(FilePath);
				if ((presentation == null) & (TotalSlides > 0))
				{
					return result;
				}
				TotalSlides = presentation.Slides.Count;
				String strPreFileName = "";
				String strOutFileName = "";

				Console.WriteLine($"Building {TotalSlides} output preview images to: {FilePrefix}");
				for (int i = 1; i <= TotalSlides; i++)
				{
					strOutFileName = FilePrefix + Convert.ToString(i) + ".jpg";
					presentation.Slides[i].Export(strOutFileName, "JPG", EXPORT_WIDTH, EXPORT_HEIGHT);
					Console.WriteLine($"Exported slide {i} to: {strOutFileName}");
					//Console.WriteLine("SlideID:{0}, SlideNumber: {1} ", Pres.Slides.Item(l).SlideID, Pres.Slides.Item(l).SlideNumber);
					strPreFileName = strOutFileName.Replace("~OUTPPPreview$", "~PREPPPreview$");

					// 파일 복사 재시도 (파일이 다른 프로세스에 의해 잠겨있을 수 있음)
					int retryCount = 0;
					bool copySucceeded = false;
					while (retryCount < 3 && !copySucceeded)
					{
						try
						{
							File.Copy(strOutFileName, strPreFileName, true);
							Console.WriteLine($"Copied to preview: {strPreFileName}");
							copySucceeded = true;
						}
						catch (IOException ex)
						{
							retryCount++;
							if (retryCount < 3)
							{
								Console.WriteLine($"File copy retry {retryCount}/3: {ex.Message}");
								System.Threading.Thread.Sleep(100);
							}
							else
							{
								Console.WriteLine($"File copy failed after 3 attempts: {ex.Message}");
								// 복사 실패시에도 계속 진행 (다음 슬라이드 처리)
							}
						}
					}
				}
				Console.WriteLine($"Completed building {TotalSlides} output preview images");

			// ✅ 이미지 생성 완료 후 캐시 업데이트
			UpdateImageBuildCache(FilePath, FilePrefix, TotalSlides);

			// Verse 정보 파싱
				ParseVersesFromPresentation(presentation, TotalSlides, ref SongVerses, ref Slide, SequenceSymbol);

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


// ✅ 이미지 생성 완료 후 캐시 업데이트 헬퍼 메서드
private void UpdateImageBuildCache(string FilePath, string FilePrefix, int TotalSlides)
{
	var pptFileInfo = new FileInfo(FilePath);
	string cacheKey = FilePath + "|" + FilePrefix + "|" + pptFileInfo.LastWriteTimeUtc.Ticks;
	_fileCheckCache[cacheKey] = (true, DateTime.Now, TotalSlides);
	Console.WriteLine($"[Cache Store] Images built successfully: {Path.GetFileName(FilePath)}, Slides: {TotalSlides}");
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
					presentation.Slides[k].Export(FilePrefix + Convert.ToString(k) + ".jpg", "JPG", EXPORT_WIDTH, EXPORT_HEIGHT);
				}

				// Verse 정보 파싱
				ParseVersesFromPresentation(presentation, TotalSlides, ref SongVerses, ref Slide, SequenceSymbol);

				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in BuildScreenDumps: {ex.Message}");
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
				presentation.Slides[1].Export(ScreenDumpFullPath[1, num], "JPG", EXPORT_WIDTH, EXPORT_HEIGHT);

				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in BuildOneFirstScreenDump: {ex.Message}");
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

	/// <summary>
	/// BuildScreenPreDumpsAsync의 결과를 담는 클래스
	/// </summary>
	public class BuildScreenDumpsResult
	{
		public bool Success { get; set; }
		public int TotalSlides { get; set; }
		public int[] SongVerses { get; set; }
		public int[,] Slide { get; set; }
	}
}

