# WinForms와 WPF 운영 기능 대조

WinForms `FrmMain.Designer.cs`의 File, Edit, View, Output, Tools, Help 메뉴에서 구분선을 제외한 40개 명령을 추출했다. 같은 메뉴에 있는 WPF 이벤트, 명령 또는 상태 속성과 연결했다. `WinFormsMenuParityTests`는 WinForms 메뉴가 추가되거나 WPF 연결이 사라질 때 실패한다.

| 메뉴 | WinForms 항목 | WPF 실행 경로 |
|---|---|---|
| File | `Menu_WorshipSessions` | `OpenManageWorshipLists_Click` |
| File | `Menu_PraiseBookTemplates` | `OpenPraiseBookIndex_Click` |
| File | `Menu_ListingOfSelectedFolder` | `ExportFolderListing_Click` |
| File | `Menu_EditHistoryList` | `RecentEdits_Open` |
| File | `Menu_Exit` | `Exit_Click` |
| Edit | `Menu_AddSong` | `Main_SourceEdit_Click / Main_New` |
| Edit | `Menu_EditSong` | `Main_SourceEdit_Click / Main_Edit` |
| Edit | `Menu_CopySong` | `Main_SourceEdit_Click / Main_Copy` |
| Edit | `Menu_MoveSong` | `Main_SourceEdit_Click / Main_Move` |
| Edit | `Menu_DeleteSong` | `Main_SourceEdit_Click / Main_Delete` |
| Edit | `Menu_SelectAll` | `CMenuSongs_SelectAll_Click` |
| Edit | `Menu_Find` | `Main_Find_Click` |
| Edit | `Menu_UseSongNumbering` | `Library.UseSongNumbering` |
| Edit | `Menu_ReArrangeSongFolders` | `FolderEditor_Click` |
| View | `Menu_EasiSlidesFolder` | `OpenEasiSlidesFolder_Click` |
| View | `Menu_Options` | `OpenSettings_Click` |
| View | `Menu_Refresh` | `Main_Refresh_Click` |
| View | `Menu_PreviewNotations` | `PreviewAreaShowNotations` |
| View | `Menu_StatusBar` | `ShowStatusMenu` |
| Output | `Menu_StartShow` | `ToggleOutputLiveCommand` |
| Output | `Menu_GoLiveWithPreview` | `SendToOutputAndNextCommand` |
| Output | `Menu_RefreshOutput` | `RefreshOutputCommand` |
| Output | `Menu_BlackScreen` | `ToggleOutputBlackCommand` |
| Output | `Menu_ClearScreen` | `ToggleOutputClearCommand` |
| Output | `Menu_RestartCurrentItem` | `RestartCurrentItemCommand` |
| Tools | `Menu_Import` | `OpenImportExport_Click` |
| Tools | `Menu_ImportFolder` | `OpenFolderImport_Click` |
| Tools | `Menu_Export` | `OpenExport_Click` |
| Tools | `Menu_Recover` | `SongRecover_Click` |
| Tools | `Menu_Empty` | `EmptyDeleted_Click` |
| Tools | `Menu_AddToUsages` | `AddListUsages_Click` |
| Tools | `Menu_ViewUsages` | `OpenUsageReport_Click` |
| Tools | `Menu_SmartMerge` | `SongMerge_Click` |
| Tools | `Menu_Compact` | `Library.CompactDatabaseCommand` |
| Tools | `Menu_ClearAllFormatting` | `ClearDatabaseFormats_Click` |
| Tools | `Menu_ClearRegistrySettings` | `ResetAllSettings_Click` |
| Help | `Menu_Contents` | `HelpContents_Click` |
| Help | `Menu_HelpWeb` | `WebHelp_Click` |
| Help | `Menu_Register` | `OpenRegistration_Click` |
| Help | `Menu_About` | `OpenAbout_Click` |

## 메뉴 밖의 조작

WinForms 메인 디자이너에서 클릭, 더블클릭, 키 입력, 마우스 버튼 해제, 선택 변경, 드롭다운 선택 이벤트에 연결된 컨트롤 이름은 중복을 제외하면 273개였다. 항목별 이벤트와 WPF 실행 경로는 [컨트롤 대조표](controls.md)에 정리했다. 개별 항목의 소스와 메뉴 위치는 `FrmMain.Designer.cs`, 기존 작업별 대응표는 `docs/wpf-migration/inventory/frmmain-to-wpf-1to1-map.md`에 있다.

| 운영 작업 | 대조 결과 |
|---|---|
| 곡·정보 화면·PowerPoint 편집 | 메뉴와 상단 도구 모음이 같은 라우팅을 사용한다. 복사·이동·삭제는 활성 소스에서 선택한 항목을 대상으로 한다. |
| 곡 선택과 송출 준비 | 곡을 미리 볼 때 예배 순서의 선택을 보존한다. 예배 순서에 추가한 곡은 원래 절 순서와 개별 서식을 사용한다. |
| 회중 안내·보호자 호출·가사 모니터 안내 | 별도 알림 창에서 조작한다. 회중 안내와 보호자 호출은 개별 배너와 별도 최근 기록 파일을 사용한다. 최근 20건까지 WinForms 파일 형식으로 저장한다. |
| 미리보기 코드와 번체·간체 | 미리보기 코드 표시와 송출 코드 표시를 따로 제어한다. 번체·간체 전환은 표시 문자열에 적용한다. |
| 사도신경 등 외부 텍스트 | 빈 줄로 문단을 나눈 후 문단 단위 맞춤 또는 자동 넘김 설정에 따라 송출한다. |
| 미디어 | 항목별 파일, 제목 검색, 카메라 번호, 음량, 좌우 균형, 음소거, 반복, 와이드 및 출력 모니터를 읽는다. 파일 시간·종료·오류 이벤트를 재생 상태에 반영한다. |
| 개별 표시 설정 | 주 언어·보조 언어의 글꼴, 크기, 글자색, 정렬, 강조, 세로 위치와 코드 조옮김을 편집한다. 개별 서식 변경 시 카메라·미디어 옵션과 추가 서식 필드를 보존한다. |
| 소스 폴더와 하단 정보 패널 | 정보 화면의 하위 폴더를 선택하고 파일을 가져온다. PowerPoint·미디어·이미지 원본 폴더를 열 수 있다. 정보 패널의 글꼴과 높이를 지정한다. |
| 세션 기본 서식과 정보 화면 | 세션 .est를 열고 저장한다. 전체에 기본 서식을 적용할 때 정보 화면을 제외한다. .esi를 편집할 때 다른 언어의 서식과 미지원 FormatData 키를 보존한다. |
| 데이터 관리 | 폴더 목록 내보내기, 사용 기록 등록·조회, 삭제 폴더 비우기와 개별 서식 초기화를 각각 연결했다. DB 변경은 기존 백업·트랜잭션 경로를 사용한다. |
| 화면 위치 및 단축키 | 자동·사용자 지정 송출 배치에서 4:3과 16:9를 처리한다. F5를 현재 항목 재시작에 연결하고 사용자가 지정한 단축키를 처리한다. |

## WinForms 창 38종

메인 폼 디렉터리의 독립적인 `Frm*.cs` 형식을 다음과 같이 분류했다. 여러 WinForms 대화상자를 하나의 WPF 창 또는 탭에서 제공하는 경우가 있다.

| WinForms | WPF에서의 위치 |
|---|---|
| FrmMain, FrmBackground, FrmLaunchShow, FrmLyricsScreen, FrmSingleMonitorAlert | MainWindow, OutputWindow, PreviewWindow, 가사 모니터, 송출 위치 및 안전 확인 |
| FrmEditItem, FrmCopy, FrmMove, FrmRecoverDeleted, FrmSmartMerge, FrmRearrangeFolderPositions | 곡 편집·복사·이동·복구·병합 및 폴더 편집 창 |
| FrmBibleRename, FrmEditBibleItem | 성경 버전 관리 및 성경 항목 편집 |
| FrmImport, FrmImportFolder, FrmImportAccessHelper, FrmExport, FrmGenerateDoc, FrmGenerateHtml | ImportExportWindow와 찬양집 탭의 RTF·HTML 내보내기 |
| FrmFind, FrmLookupTitles, FrmUsages | SearchUsageWindow의 곡·제목·사용 현황 탭 |
| FrmManageItemLists, FrmEditNotes, FrmInfoScreen, FrmShowAlert | 예배 순서·찬양집 관리, 세션 메모, 정보 화면, 송출 알림 창 |
| FrmMediaPlayerControl, FrmLaunchMediaPlayer, FrmCopyMoveExternal | 미디어 지정과 미리 듣기, 송출 재생기, 외부 파일 복사·이동 |
| FrmOptions, FrmGetWorkingFolder, FrmSplashScreen, FrmSplashScreenOld | SettingsWindow, 작업 폴더 선택과 시작 화면. 이전 스플래시 디자인은 별도의 운영 명령이 없다. |
| FrmHelp, FrmAbout, FrmRegister, FrmPopupText, FrmUpdateFileName | 도움말·정보·등록 창 및 작업별 텍스트/이름 입력 대화상자 |

## 검증 범위

실제 WPF 창을 격리된 설정과 테스트 데이터로 열어 File 메뉴, Alerts 창 재사용, 송출 창의 두 배너와 검은 화면 전환을 확인했다. 송출 창의 실제 WPF 레이아웃에서 긴 한글 48줄 문단의 표시 경계와 마지막 줄을 검사했다. 미디어 설정 창에서 다섯 원본 모드와 다섯 카메라 번호를 확인했다. 실제 송출 창에서 3초 무음 WAV의 위치 증가와 종료 상태를 확인했다. 실제 PowerPoint로 4:3과 16:9 문서를 렌더링해 미리보기·송출 양쪽에서 640×480 및 640×360 썸네일 세 장씩 확인했다. 개별 표시 설정 창에서 두 언어 탭과 보조 언어의 저장된 글꼴을 확인했다. SQLite 정리와 레거시 파일 읽기·쓰기는 임시 파일을 사용한다. 카메라 신호, 물리 모니터 재연결, PowerPoint 프레젠터 리모컨, 프레젠테이션에 포함된 동영상은 장치·Office 버전·파일 조합에 따라 현장 검증이 필요하다. 3초짜리 192×108 AV1 MP4는 WPF 기본 재생 경로에서 0xC00D11B1을 재현했다. DirectShow 대체 경로를 적용한 뒤 같은 파일이 3초 동안 재생되고 정상 종료했다. 실제 예배 순서의 Play Media on Output Monitor 메뉴에 연결된 명령으로도 같은 파일을 재생했다. 테스트용 가상 디스플레이에 배치한 WPF 송출 창에서 192×108 영상 프레임, 픽셀 값 범위 0~255, 재생 위치의 진행과 정상 종료를 확인했다. 등록된 영상 디코더를 사용하는 대체 경로는 파일 미리보기와 송출 재생 양쪽에 연결된다. 카메라도 DirectShow를 사용한다.

격리된 Release 전체 테스트 2,822개가 통과했다. WPF 2,567개, WinForms 235개, 분석기 20개로 실패·건너뜀은 0개였다. OpenSpec 변경은 엄격 검증을 통과했다.
