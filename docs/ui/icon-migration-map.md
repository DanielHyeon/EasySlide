# 아이콘 자산 마이그레이션 매핑 표 (현행 → Fluent UI System Icons)

> 본 표는 계획서 [§11.B](../ui-ux-modernization-plan.md) 의 상세본. 추출/사용 가이드는 [icon-pipeline.md](icon-pipeline.md).
>
> **사용법**: Sprint 1 Day 1~2에 본 표를 따라 일괄 추출. `옵션 A` 열에 값이 있으면 WPF UI `SymbolIcon` 그대로 사용. 없으면 옵션 B(SVG 변환).

## 표 보는 법

| 열 | 의미 |
|---|---|
| **현행** | `Easislides/Resources/` 또는 `EasislideImages/`의 raster 파일명 |
| **EasiDS 키** | `Theme/Icons.xaml`에 정의할 통일된 키 (`Icon.도메인.역할`) |
| **옵션 A (WPF UI Symbol)** | `<Ui:SymbolIcon Symbol="..." />` 즉시 사용 |
| **옵션 B (Fluent SVG)** | WPF UI Symbol에 없으면 이 SVG 다운로드 → 변환 |
| **상태** | 폐기 / 사용자 콘텐츠로 분리 / 매핑 결정 |

## 1. 송출 제어 (라이브)

| 현행 | EasiDS 키 | 옵션 A (WPF UI Symbol) | 옵션 B (Fluent SVG) | 상태 |
|---|---|---|---|---|
| `BlackScreen.png` + `-Pressed.png` + `-Red.png` | `Icon.Live.Black` | `EyeOff24` | `ic_fluent_eye_off_24_regular` | 3상태 → 1자산, 색 토큰으로 상태 표현 |
| `BlueScreen-Pressed.png` + `-Red.png` | `Icon.Live.Hide` | `Pause24` | `ic_fluent_pause_24_regular` | (또는 `Stop24`) |
| `hideText.png` | `Icon.Live.HideText` | `TextHidden24` 또는 `EyeOff24` | `ic_fluent_text_field_24_regular` (overlay 추가) | 검토 필요 |
| `btnLive.png` | `Icon.Live.Toggle` | `VideoClip24` | `ic_fluent_video_clip_24_regular` | |
| `btnToOutput.png` | `Icon.Live.SendToOutput` | `ArrowRight24` | `ic_fluent_arrow_right_24_regular` | |
| `btnToOutputMove.png` | `Icon.Live.MoveToOutput` | `ArrowMove24` 또는 `ArrowRightDoubled24` | `ic_fluent_arrow_move_24_regular` | |
| `LiveCam.png` | `Icon.Live.Camera` | `VideoClip24` | `ic_fluent_video_clip_24_regular` | |
| `camcorder.png` + `-Red.png` | `Icon.Live.Camcorder` | `Camera24` | `ic_fluent_camera_24_regular` | 빨강 상태는 `Brush.Live.Active` 토큰 |
| `Send.png` | `Icon.Live.Send` | `Send24` | `ic_fluent_send_24_regular` | |
| `NumNewScreen.png` | `Icon.Live.NewScreen` | `WindowNew24` | `ic_fluent_window_new_24_regular` | |
| `HideDisplay.png` | `Icon.Live.HideDisplay` | `EyeOff24` | `ic_fluent_eye_off_24_regular` | |
| `Tick.png` | `Icon.Status.Check` | `Checkmark24` | `ic_fluent_checkmark_24_regular` | |

## 2. 콘텐츠 (성경·찬양·미디어·문서)

| 현행 | EasiDS 키 | 옵션 A | 옵션 B | 상태 |
|---|---|---|---|---|
| `Bible.png` + **`Bible - Hightlight.png`(오타)** | `Icon.Bible` | `Book24` | `ic_fluent_book_24_regular` | 오타 자산 폐기 |
| `Media.png` + `Media-highlight.png` | `Icon.Media.Play` | `PlayCircle24` | `ic_fluent_play_circle_24_regular` | |
| `notebook.png` + `-highlight.png` | `Icon.Notebook` | `Notebook24` | `ic_fluent_notebook_24_regular` | |
| `word.png` + `-highlight.png` | `Icon.Document.Word` | `DocumentText24` | `ic_fluent_document_text_24_regular` | (Word 브랜드 표시 필요 시 `DocumentEdit24` 대안) |
| `PPImg.png` + ` - Highlight.png` | `Icon.PowerPoint.Slide` | `SlideLayout24` | `ic_fluent_slide_layout_24_regular` | |
| `Html.png` | `Icon.Document.Html` | `Code24` | `ic_fluent_code_24_regular` | (`DocumentCode24` 대안) |

## 3. 파일·폴더·관리

| 현행 | EasiDS 키 | 옵션 A | 옵션 B | 상태 |
|---|---|---|---|---|
| `folder.png` | `Icon.Folder` | `Folder24` | `ic_fluent_folder_24_regular` | |
| `folderOpen.png` | `Icon.Folder.Open` | `FolderOpen24` | `ic_fluent_folder_open_24_regular` | |
| `Add.png` | `Icon.Action.Add` | `Add24` | `ic_fluent_add_24_regular` | |
| `DeleteFile.png` | `Icon.Action.Delete` | `Delete24` | `ic_fluent_delete_24_regular` | |
| `DeleteList.png` | `Icon.Action.DeleteList` | `Delete24` | `ic_fluent_delete_24_regular` | (DeleteFile과 동일 자산) |
| `Clear.png` | `Icon.Action.Clear` | `Eraser24` | `ic_fluent_eraser_24_regular` | |
| `EditFile.png` | `Icon.Action.Edit` | `Edit24` | `ic_fluent_edit_24_regular` | (EasislideImages) |
| `NewItem.png` | `Icon.Action.NewItem` | `DocumentAdd24` | `ic_fluent_document_add_24_regular` | (EasislideImages) |
| `CopyFile.png` | `Icon.Action.Copy` | `Copy24` | `ic_fluent_copy_24_regular` | (EasislideImages) |
| `MoveFile.png` | `Icon.Action.Move` | `ArrowMove24` | `ic_fluent_arrow_move_24_regular` | (EasislideImages) |
| `Move Up.png` | `Icon.Action.MoveUp` | `ArrowUp24` | `ic_fluent_arrow_up_24_regular` | (EasislideImages) |
| `Move Down.png` | `Icon.Action.MoveDown` | `ArrowDown24` | `ic_fluent_arrow_down_24_regular` | (EasislideImages) |
| `Refresh.png` | `Icon.Action.Refresh` | `ArrowSync24` 또는 `ArrowClockwise24` | `ic_fluent_arrow_sync_24_regular` | (EasislideImages) |
| `Find.png` | `Icon.Action.Find` | `Search24` | `ic_fluent_search_24_regular` | (EasislideImages) |
| `save.png` | `Icon.Action.Save` | `Save24` | `ic_fluent_save_24_regular` | (EasislideImages) |

## 4. 설정·시스템·도움말

| 현행 | EasiDS 키 | 옵션 A | 옵션 B | 상태 |
|---|---|---|---|---|
| `Option.png` / `options.png` | `Icon.Settings` | `Settings24` | `ic_fluent_settings_24_regular` | |
| `keyboard.png` | `Icon.Shortcuts` | `Keyboard24` | `ic_fluent_keyboard_24_regular` | |
| `Help.png` | `Icon.Help` | `QuestionCircle24` 또는 `Question24` | `ic_fluent_question_circle_24_regular` | |
| `ques.png` | `Icon.Question` | `Question24` | `ic_fluent_question_24_regular` | |
| `Info_Sym.png` + `highlight.png` | `Icon.Info` | `Info24` | `ic_fluent_info_24_regular` | |
| `Alert.png` | `Icon.Alert` | `Warning24` | `ic_fluent_warning_24_regular` | (EasislideImages) |
| `Template.png` | `Icon.Template` | `Layer24` 또는 `DocumentTextLink24` | `ic_fluent_layer_24_regular` | |
| `Contents.png` | `Icon.Contents` | `ListBar24` 또는 `TextBulletList24` | `ic_fluent_text_bullet_list_24_regular` | |
| `WishList.png` | `Icon.WorshipList` | `TaskListLtr24` | `ic_fluent_task_list_ltr_24_regular` | EasiSlides 도메인 핵심 |
| `EditSessionNote.png` | `Icon.SessionNote` | `Notepad24` 또는 `NoteEdit24` | `ic_fluent_notepad_24_regular` | |
| `PPTListType.png` | `Icon.Ppt.List` | `SlideMultiple24` | `ic_fluent_slide_multiple_24_regular` | |
| `PPTPreviewStyle.png` | `Icon.Ppt.Preview` | `SlideContent24` 또는 `SlideGrid24` | `ic_fluent_slide_content_24_regular` | |
| `MediaFile.png` | `Icon.Media.File` | `MusicNote124` 또는 `Video24` | `ic_fluent_video_24_regular` | (EasislideImages) |
| `NoRotate.png` | `Icon.NoRotate` | `LockClosed24` 또는 `ArrowRotateClockwiseOff24` | `ic_fluent_lock_closed_24_regular` | (EasislideImages) — 의미 검토 필요 |

## 5. 모니터·디스플레이

| 현행 | EasiDS 키 | 옵션 A | 옵션 B | 상태 |
|---|---|---|---|---|
| `singlescreen.png` | `Icon.Monitor.Single` | `Desktop24` | `ic_fluent_desktop_24_regular` | |
| `dualscreens.png` | `Icon.Monitor.Dual` | `DualScreen24` | `ic_fluent_dual_screen_24_regular` | |

## 6. 브랜드·로고 (자체 자산 유지 또는 별도 결정)

| 현행 | EasiDS 키 | 결정 |
|---|---|---|
| `ES Icon 32 Blue.png` + `Highlight.png` | `Icon.Brand.AppIcon` | **자체 자산 유지** — Fluent 아이콘으로 대체 불가 (앱 브랜드 식별자). Sprint 1에 SVG 리디자인 검토. |
| `ES.Icon.ico` / `$this.Icon.ico` | `Icon.Brand.Window` | **자체 자산 유지** — Windows 작업표시줄·트레이용 .ico. Sprint 1에 다중 사이즈 .ico 재생성. |
| `logo2023.png` / `logo2023.gif` | `Icon.Brand.LogoMark` | **자체 자산 유지** — 마케팅·About 다이얼로그용. PNG→SVG 변환 검토. |
| `favicon-Highlight.ico` (EasislideImages) | — | EasislideImages 기타 자산은 폐기 후보. 미사용 확인 후 제거. |

## 7. 폐기 (자산 아님 — 사용자 콘텐츠 또는 미사용)

| 현행 | 처리 |
|---|---|
| `주일예배 썸네일.png` / `수요예배 썸네일.png` | **사용자 콘텐츠로 분리** — `%AppData%\EasislidesNext\UserAssets\Thumbs\`로 마이그레이션 (§10.5.1 AssetMigrator) |
| `panel1.BackgroundImage.gif` / `panel1.BackgroundImage.png` | UI 배경 raster — 폐기, 토큰 색만 사용 |
| `no-image.png` | `Icon.Placeholder.NoImage` — `ImageMultiple24` 또는 `ImageOff24`로 대체 |
| `Image1.bmp` | 미사용 추정 — 그렙으로 참조 확인 후 폐기 |
| `pic-bestfit.png` | UI 옵션 인디케이터 — `ResizeImage24` 또는 자체 작성 |
| `Cherch.png` / `Easislides\Cherch.png` | 의미 불명 — 코드 참조 확인 후 폐기 |
| `Chinese.png` / `A.png` / `B.png` / `C.png` (EasislideImages) | 다국어/탭 라벨 — 텍스트로 대체 검토 |
| `Bibles_Go.png` | `Icon.Bible.Go` → `Play24` 또는 `ArrowRight24` |
| `genRTF.png` (EasislideImages) | `Icon.Export.Rtf` → `DocumentArrowDown24` |

## 8. 처리 카운터 (검증용)

| 카테고리 | 항목 수 | 비고 |
|---|---|---|
| §1 송출 제어 | 12 | 모두 옵션 A 가능 |
| §2 콘텐츠 | 6 | 모두 옵션 A 가능 |
| §3 파일·관리 | 14 | 모두 옵션 A 가능 |
| §4 설정·도움말 | 14 | 의미 검토 필요 항목 일부 (Template, NoRotate) |
| §5 모니터 | 2 | 옵션 A 가능 |
| §6 브랜드 (유지) | 4 | Fluent 대체 불가 — 자체 SVG 리디자인 |
| §7 폐기 | 9 | AssetMigrator 또는 단순 삭제 |
| **합계** | **61** | (송출 제어 §1의 3상태 자산은 묶음 1개로 계산) |

## 9. 적용 후 디렉터리 상태 (목표)

**삭제 대상**:
- `Easislides/Resources/*.png` (전부)
- `Easislides/Resources/Image1.bmp`
- `EasislideImages/*.png` 및 `EasislideImages/Original/*.png` (브랜드 자산 제외)

**유지 (자체 자산)**:
- `Easislides.Wpf/Assets/Brand/AppIcon.svg` ← 신규, ES Icon 32 Blue 재작업
- `Easislides.Wpf/Assets/Brand/AppIcon.ico` ← 다중 사이즈 .ico
- `Easislides.Wpf/Assets/Brand/Logo.svg` ← logo2023 재작업
- `Easislides.Wpf/Theme/Icons.xaml` ← Fluent 아이콘 매핑 키 정의

**금지** (Q6 100% 폐기 원칙):
- 어떤 형태로든 PNG/BMP 형식의 UI 아이콘
- `-Highlight` / `-Pressed` / `-Red` 상태별 별파일

## 10. 검증 체크리스트 (Sprint 1 마무리)

- [ ] 본 표의 모든 행에 `Icon.*` 키 할당됨
- [ ] 옵션 A 컬럼의 모든 Symbol이 WPF UI 라이브러리에 실제 존재하는지 확인 (코드에서 `Wpf.Ui.Controls.SymbolRegular`/`SymbolFilled` enum 검사)
- [ ] 옵션 B로 폴백한 항목은 SVG 다운로드 + 변환 + `Assets/Icons/` 등록
- [ ] `Easislides/Resources/`, `EasislideImages/` 폴더 전체 삭제 (Q6)
- [ ] 사용자 콘텐츠(`주일예배 썸네일.png` 등)는 `AssetMigrator`로 이동 (§10.5.1)
- [ ] 모든 폼에서 더 이상 Legacy raster를 참조하지 않음 (`grep -r "\.png" Easislides.Wpf/` 결과 0건, Assets/Brand 제외)
