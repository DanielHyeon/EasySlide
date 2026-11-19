# 메인 폼 이벤트 대조표

WinForms 메인 디자이너의 클릭·더블클릭·키 입력·마우스 버튼 해제·선택 변경·드롭다운 선택 이벤트를 이름별로 묶었다. WPF 열에는 주 실행 경로 또는 대응하는 XAML 컨트롤을 기재했다. 같은 컨트롤에서 발생하는 모든 마우스·키 조합을 실기기로 재현한 수치는 아니다.

| WinForms 컨트롤 | WinForms 이벤트 및 처리기 | WPF 대응 경로 |
|---|---|---|
| BibleText | MouseUp → BibleText_MouseUp | MainWindow.xaml / BiblePassageBox / Text={Binding Bible.PassageText, Mode=OneWay} |
| BibleUserLookup | KeyDown → BibleUserLookup_KeyDown | MainWindow.xaml / BibleUserLookup / Text={Binding Bible.TypedReference, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}, KeyDown=BibleReferenceBox_KeyDown |
| Bibles_Go | MouseUp → Bibles_Btn_MouseUp | MainWindow.xaml / Bibles_Go / Click=JumpBibleReference_Click |
| BookLookup | SelectedIndexChanged → BookLookup_SelectedIndexChanged | MainWindow.xaml / BookLookup / ItemsSource={Binding Bible.Books}, SelectedItem={Binding Bible.SelectedBook, Mode=TwoWay} |
| CMenuBible_AddShow | Click → CMenuBible_AddShow_Click | MainWindow.xaml / CMenuBible_AddShow / Click=AddBibleVerse_Click |
| CMenuBible_Copy | Click → CMenuBible_Copy_Click | MainWindow.xaml / CMenuBible_Copy / Click=CopyBiblePassage_Click |
| CMenuBible_CopyInfoScreen | Click → CMenuBible_CopyInfoScreen_Click | MainWindow.xaml / CMenuBible_CopyInfoScreen / Click=CopyBibleToInfoScreen_Click |
| CMenuBible_SelectAll | Click → CMenuBible_SelectAll_Click | MainWindow.xaml / CMenuBible_SelectAll / Click=SelectAllBiblePassage_Click |
| CMenuBible_UnselectAll | Click → CMenuBible_UnselectAll_Click | MainWindow.xaml / CMenuBible_UnselectAll / Click=UnselectAllBiblePassage_Click |
| CMenuFiles_AddShow | Click → CMenuFiles_AddShow_Click | MainWindow.xaml / CMenuInfoScreenFiles_AddShow / Click=CMenuInfoScreenFiles_AddShow_Click |
| CMenuFiles_Copy | Click → CMenuFiles_Copy_Click | MainWindow.xaml / CMenuInfoScreenFiles_Copy / Click=CMenuInfoScreenFiles_Copy_Click |
| CMenuFiles_Edit | Click → CMenuFiles_Edit_Click | MainWindow.xaml / CMenuInfoScreenFiles_Edit / Click=CMenuInfoScreenFiles_Edit_Click |
| CMenuFiles_Refresh | Click → CMenuFiles_Refresh_Click | MainWindow.xaml / CMenuInfoScreenFiles_Refresh / Click=CMenuInfoScreenFiles_Refresh_Click |
| CMenuFiles_SelectAll | Click → CMenuFiles_SelectAll_Click | MainWindow.xaml / CMenuInfoScreenFiles_SelectAll / Click=CMenuInfoScreenFiles_SelectAll_Click |
| CMenuFiles_UnselectAll | Click → CMenuFiles_UnselectAll_Click | MainWindow.xaml / CMenuInfoScreenFiles_UnselectAll / Click=CMenuInfoScreenFiles_UnselectAll_Click |
| CMenuImages_AddDefault | Click → CMenuImages_AddDefault_Click | MainWindow.xaml / CMenuImages_AddDefault / Command={Binding PlacementTarget.DataContext.ApplyAsBackgroundCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}} |
| CMenuImages_AddItem | Click → CMenuImages_AddItem_Click | MainWindow.xaml / CMenuImages_AddItem / Command={Binding PlacementTarget.DataContext.ApplyToItemBackgroundCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}} |
| CMenuImages_Refresh | Click → CMenuImages_Refresh_Click | MainWindow.xaml / CMenuImages_Refresh / Command={Binding PlacementTarget.DataContext.LoadCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}} |
| CMenuPraiseB_Clear | Click → CMenuPraiseB_Clear_Click | MainWindow.xaml / CMenuPraiseB_Clear / Click=CMenuPraiseB_Clear_Click |
| CMenuPraiseB_Edit | Click → CMenuPraiseB_Edit_Click | MainWindow.xaml / CMenuPraiseB_Edit / Click=CMenuPraiseB_Edit_Click |
| CMenuPraiseB_SelectAll | Click → CMenuPraiseB_SelectAll_Click | MainWindow.xaml / CMenuPraiseB_SelectAll / Click=CMenuPraiseB_SelectAll_Click |
| CMenuPraiseB_UnselectAll | Click → CMenuPraiseB_UnselectAll_Click | MainWindow.xaml / CMenuPraiseB_UnselectAll / Click=CMenuPraiseB_UnselectAll_Click |
| CMenuSongs_AddShow | Click → CMenuSongs_AddShow_Click | MainWindow.xaml / CMenuSongs_AddShow / Click=CMenuSongs_AddShow_Click |
| CMenuSongs_Copy | Click → CMenuSongs_Copy_Click | MainWindow.xaml / CMenuSongs_Copy / Click=SongCopy_Click |
| CMenuSongs_Edit | Click → CMenuSongs_Edit_Click | MainWindow.xaml / CMenuSongs_Edit / Click=CMenuSongs_Edit_Click |
| CMenuSongs_Refresh | Click → CMenuSongs_Refresh_Click | MainWindow.xaml / CMenuSongs_Refresh / Click=CMenuSongs_Refresh_Click |
| CMenuSongs_SelectAll | Click → CMenuSongs_SelectAll_Click | MainWindow.xaml / CMenuSongs_SelectAll / Click=CMenuSongs_SelectAll_Click |
| CMenuSongs_UnselectAll | Click → CMenuSongs_UnselectAll_Click | MainWindow.xaml / CMenuSongs_UnselectAll / Click=CMenuSongs_UnselectAll_Click |
| CMenuWorship_AddUsages | Click → CMenuWorship_AddUsages_Click | WorshipListPanel.xaml / CMenuWorship_AddUsages / Click=CMenuWorship_AddUsages_Click |
| CMenuWorship_Clear | Click → CMenuWorship_Clear_Click | WorshipListPanel.xaml / CMenuWorship_Clear / Command={Binding PlacementTarget.DataContext.ClearWorshipListCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}} |
| CMenuWorship_Edit | Click → CMenuWorship_Edit_Click | WorshipListPanel.xaml / CMenuWorship_Edit / Click=CMenuWorship_Edit_Click |
| CMenuWorship_Play | Click → CMenuWorship_Play_Click | WorshipListPanel.xaml / CMenuWorship_Play / Command={Binding PlacementTarget.DataContext.PlaySelectedWorshipMediaCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}} |
| CMenuWorship_PlayOnOutput | Click → CMenuWorship_PlayOnOutput_Click | WorshipListPanel.xaml / CMenuWorship_PlayOnOutput / Command={Binding PlacementTarget.DataContext.PlaySelectedWorshipMediaOnOutputCommand, RelativeSource={RelativeSource AncestorType=ContextMenu}} |
| CMenuWorship_SelectAll | Click → CMenuWorship_SelectAll_Click | WorshipListPanel.xaml / CMenuWorship_SelectAll / Click=CMenuWorship_SelectAll_Click |
| CMenuWorship_UnselectAll | Click → CMenuWorship_UnselectAll_Click | WorshipListPanel.xaml / CMenuWorship_UnselectAll / Click=CMenuWorship_UnselectAll_Click |
| DefApplyDefaultsBtn | Click → DefApplyDefaultsBtn_Click | ApplyGlobalFormatToAllCommand |
| Def_AssignMedia | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_AssignMediaPanel |
| Def_BackColour | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_BackColour |
| Def_Head | DropDownItemClicked → Def_Head_DropDownItemClicked | MainWindow.xaml / Def_Head / IsChecked={Binding ActiveLyricsTitleHeading, Mode=OneWay}, Command={Binding ToggleLyricsTitleHeadingCommand} |
| Def_HeadAlign | DropDownItemClicked → Def_HeadAlign_DropDownItemClicked | MainWindow.xaml / Def_HeadAlign |
| Def_ImageMode | DropDownItemClicked → Def_ImageMode_DropDownItemClicked | MainWindow.xaml / Def_ImageMode |
| Def_Interlace | MouseUp → Def_Items_MouseUp | ToggleInterlaceCommand |
| Def_LoadTemplate | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_LoadTemplate / Click=LoadSessionTemplate_Click |
| Def_NoImage | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_NoImage / Command={Binding ClearOutputBackgroundImageCommand} |
| Def_Notations | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_Notations / IsChecked={Binding ActiveLyricsNotations, Mode=OneWay}, Command={Binding ToggleLyricsNotationsCommand} |
| Def_Outline | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_Outline / IsChecked={Binding ActiveLyricsOutline, Mode=OneWay}, Command={Binding ToggleLyricsOutlineCommand} |
| Def_PanelAsR1 | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelAsR1 / IsChecked={Binding ActivePanelTextColorAsRegion1, Mode=OneWay}, Command={Binding TogglePanelTextColorAsRegion1Command} |
| Def_PanelBackColour | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelBackColour |
| Def_PanelCopyright | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelCopyright / IsChecked={Binding ActiveLyricsCopyright, Mode=OneWay}, Command={Binding ToggleLyricsCopyrightCommand} |
| Def_PanelFontBold | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelFontBold / IsChecked={Binding ActivePanelFontBold, Mode=OneWay}, Command={Binding TogglePanelFontBoldCommand} |
| Def_PanelFontItalics | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelFontItalics / IsChecked={Binding ActivePanelFontItalic, Mode=OneWay}, Command={Binding TogglePanelFontItalicCommand} |
| Def_PanelFontList | SelectedIndexChanged → Def_PanelFontList_SelectedIndexChanged | MainWindow.xaml / Def_PanelFontList / ItemsSource={Binding LyricsFontFamilyOptions}, Text={Binding PanelFontFamily, Mode=TwoWay, UpdateSourceTrigger=LostFocus} |
| Def_PanelFontOutline | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelFontOutline / IsChecked={Binding ActivePanelFontOutline, Mode=OneWay}, Command={Binding TogglePanelFontOutlineCommand} |
| Def_PanelFontShadow | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelFontShadow / IsChecked={Binding ActivePanelFontShadow, Mode=OneWay}, Command={Binding TogglePanelFontShadowCommand} |
| Def_PanelFontUnderline | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelFontUnderline / IsChecked={Binding ActivePanelFontUnderline, Mode=OneWay}, Command={Binding TogglePanelFontUnderlineCommand} |
| Def_PanelHeight | MouseUp → Def_PanelHeight_MouseUp | MainWindow.xaml / Def_PanelHeight / Text={Binding PanelHeightPercent, Mode=TwoWay, UpdateSourceTrigger=LostFocus, ValidatesOnExceptions=True} |
| Def_PanelPrevNext | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelPrevNext / IsChecked={Binding ActiveLyricsNextItem, Mode=OneWay}, Command={Binding ToggleLyricsNextItemCommand} |
| Def_PanelShow | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelShow / IsChecked={Binding ActiveLyricsDisplayPanel, Mode=OneWay}, Command={Binding ToggleLyricsDisplayPanelCommand} |
| Def_PanelSlides | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelSlides / IsChecked={Binding ActiveLyricsPositionIndicator, Mode=OneWay}, Command={Binding ToggleLyricsPositionIndicatorCommand} |
| Def_PanelSong | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelSong / IsChecked={Binding ActiveLyricsItemNumber, Mode=OneWay}, Command={Binding ToggleLyricsItemNumberCommand} |
| Def_PanelTextColour | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelTextColour |
| Def_PanelTitle | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelTitle / IsChecked={Binding ActiveLyricsTitleOnPanel, Mode=OneWay}, Command={Binding ToggleLyricsTitleOnPanelCommand} |
| Def_PanelTransparent | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_PanelTransparent / IsChecked={Binding ActiveLyricsPanelTransparent, Mode=OneWay}, Command={Binding TogglePanelTransparentCommand} |
| Def_R1Align | DropDownItemClicked → Def_R1Align_DropDownItemClicked | MainWindow.xaml / Def_R1Align |
| Def_R1Colour | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_R1Colour |
| Def_R2Align | DropDownItemClicked → Def_R2Align_DropDownItemClicked | MainWindow.xaml / Def_R2Align / ItemsSource={Binding LyricsRegion2AlignmentPresets} |
| Def_R2Colour | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_R2Colour / ItemsSource={Binding LyricsTextColor2Presets} |
| Def_Region | DropDownItemClicked → Def_Region_DropDownItemClicked | MainWindow.xaml / Def_Region |
| Def_SaveTemplate | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_SaveTemplate / Click=SaveSessionTemplate_Click |
| Def_Shadow | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_Shadow / IsChecked={Binding ActiveLyricsShadow, Mode=OneWay}, Command={Binding ToggleLyricsShadowCommand} |
| Def_ToZero | MouseUp → Def_Items_MouseUp | MainWindow.xaml / Def_ToZero / Command={Binding TransposeLiveResetCommand} |
| Def_TransItem | SelectedIndexChanged → Def_TransSelectedIndexChanged | MainWindow.xaml / Def_TransItem / ItemsSource={Binding LegacyTransitionOptions} |
| Def_TransSlides | SelectedIndexChanged → Def_TransSelectedIndexChanged | MainWindow.xaml / Def_TransSlides / ItemsSource={Binding LegacyTransitionOptions} |
| Def_VAlign | DropDownItemClicked → Def_VAlign_DropDownItemClicked | MainWindow.xaml / Def_VAlign |
| Folders_WordCount | Click → Folders_WordCount_Click | MainWindow.xaml / Folders_WordCount / IsChecked={Binding Library.IsWordCountSortEnabled, Mode=TwoWay} |
| Image_Import | Click → Image_Import_Click | MainWindow.xaml / Image_Import / Click=ImageImport_Click |
| Image_OpenFolder | Click → Image_OpenFolder_Click | MainWindow.xaml / Image_OpenFolder / Click=OpenSourceFolder_Click |
| ImagesFolder | SelectedIndexChanged → ImagesFolder_SelectedIndexChanged | MainWindow.xaml / ImagesFolder / ItemsSource={Binding FolderGroups}, SelectedItem={Binding SelectedFolder, Mode=TwoWay} |
| Ind_AssignMedia | MouseUp → Ind_Items_MouseUp | MainWindow.xaml / Ind_AssignMedia / Click=SelectPreviewItemMedia_Click |
| Ind_BackColour | MouseUp → Ind_Items_MouseUp | MainWindow.xaml / Ind_BackColour |
| Ind_BottomUpDown | MouseUp → Ind_MarginUpDown_MouseUp | MainWindow.xaml / Ind_BottomUpDown / Text={Binding SelectedItemBottomMarginInput, Mode=TwoWay, UpdateSourceTrigger=LostFocus, ValidatesOnExceptions=True} |
| Ind_CapoDown | MouseUp → Ind_Items_MouseUp | ItemDisplayOptionsWindow / FormatData 71 |
| Ind_CapoUp | MouseUp → Ind_Items_MouseUp | ItemDisplayOptionsWindow / FormatData 71 |
| Ind_Head | DropDownItemClicked → Ind_Head_DropDownItemClicked | ItemDisplayOptionsWindow / FormatData 21 |
| Ind_HeadAlign | DropDownItemClicked → Ind_HeadAlign_DropDownItemClicked | ItemDisplayOptionsWindow / FormatData 23 |
| Ind_HideDisplayPanel | MouseUp → Ind_Items_MouseUp | ItemDisplayOptionsWindow / FormatData 22 |
| Ind_ImageMode | DropDownItemClicked → Ind_ImageMode_DropDownItemClicked | MainWindow.xaml / Ind_ImageMode |
| Ind_Interlace | MouseUp → Ind_Items_MouseUp | ItemDisplayOptionsWindow / FormatData 22 |
| Ind_LeftUpDown | MouseUp → Ind_MarginUpDown_MouseUp | MainWindow.xaml / Ind_LeftUpDown / Text={Binding SelectedItemLeftMarginInput, Mode=TwoWay, UpdateSourceTrigger=LostFocus, ValidatesOnExceptions=True} |
| Ind_LoadTemplate | MouseUp → Ind_Items_MouseUp | MainWindow.xaml / Ind_LoadTemplate / Click=LoadPreviewItemTemplate_Click |
| Ind_NoImage | MouseUp → Ind_Items_MouseUp | MainWindow.xaml / Ind_NoImage / Command={Binding SetSelectedItemBackgroundImageCommand} |
| Ind_Notations | MouseUp → Ind_Items_MouseUp | ItemDisplayOptionsWindow / FormatData 22 |
| Ind_Outline | MouseUp → Ind_Items_MouseUp | ItemDisplayOptionsWindow / FormatData 22 |
| Ind_R1Align | DropDownItemClicked → Ind_R1Align_DropDownItemClicked | ItemDisplayOptionsWindow / FormatData 31 |
| Ind_R1Bold | MouseUp → Ind_RegionsFormat_MouseUp | ItemDisplayOptionsWindow / FormatData 41 |
| Ind_R1Colour | MouseUp → Ind_RegionsFormat_MouseUp | ItemDisplayOptionsWindow / FormatData 29 |
| Ind_R1Italics | DropDownItemClicked → Ind_R1Italics_DropDownItemClicked | ItemDisplayOptionsWindow / FormatData 41 |
| Ind_R1Underline | MouseUp → Ind_RegionsFormat_MouseUp | ItemDisplayOptionsWindow / FormatData 41 |
| Ind_R2Align | DropDownItemClicked → Ind_R2Align_DropDownItemClicked | ItemDisplayOptionsWindow / FormatData 32 |
| Ind_R2Bold | MouseUp → Ind_RegionsFormat_MouseUp | ItemDisplayOptionsWindow / FormatData 41 |
| Ind_R2Colour | MouseUp → Ind_RegionsFormat_MouseUp | ItemDisplayOptionsWindow / FormatData 30 |
| Ind_R2Italics | DropDownItemClicked → Ind_R2Italics_DropDownItemClicked | ItemDisplayOptionsWindow / FormatData 41 |
| Ind_R2Underline | MouseUp → Ind_RegionsFormat_MouseUp | ItemDisplayOptionsWindow / FormatData 41 |
| Ind_Reg1FontsList | SelectedIndexChanged → Ind_FontsList_SelectedIndexChanged | ItemDisplayOptionsWindow / FormatData 43 |
| Ind_Reg1SizeUpDown | MouseUp → Ind_FontSizeUpDown_MouseUp | ItemDisplayOptionsWindow / FormatData 47 |
| Ind_Reg1TopUpDown | MouseUp → Ind_MarginUpDown_MouseUp | ItemDisplayOptionsWindow / FormatData 45 |
| Ind_Reg2FontsList | SelectedIndexChanged → Ind_FontsList_SelectedIndexChanged | ItemDisplayOptionsWindow / FormatData 44 |
| Ind_Reg2SizeUpDown | MouseUp → Ind_FontSizeUpDown_MouseUp | ItemDisplayOptionsWindow / FormatData 48 |
| Ind_Reg2TopUpDown | MouseUp → Ind_MarginUpDown_MouseUp | ItemDisplayOptionsWindow / FormatData 46 |
| Ind_Region | DropDownItemClicked → Ind_Region_DropDownItemClicked | ItemDisplayOptionsWindow / FormatData 25 |
| Ind_RightUpDown | MouseUp → Ind_MarginUpDown_MouseUp | MainWindow.xaml / Ind_RightUpDown / Text={Binding SelectedItemRightMarginInput, Mode=TwoWay, UpdateSourceTrigger=LostFocus, ValidatesOnExceptions=True} |
| Ind_SaveTemplate | MouseUp → Ind_Items_MouseUp | MainWindow.xaml / Ind_SaveTemplate / Click=SavePreviewItemTemplate_Click |
| Ind_Shadow | MouseUp → Ind_Items_MouseUp | ItemDisplayOptionsWindow / FormatData 22 |
| Ind_TransItem | SelectedIndexChanged → Ind_TransSelectedIndexChanged | MainWindow.xaml / Ind_TransItem / ItemsSource={Binding LegacyTransitionOptions} |
| Ind_TransSlides | SelectedIndexChanged → Ind_TransSelectedIndexChanged | MainWindow.xaml / Ind_TransSlides / ItemsSource={Binding LegacyTransitionOptions} |
| Ind_VAlign | DropDownItemClicked → Ind_VAlign_DropDownItemClicked | MainWindow.xaml / Ind_VAlign |
| Ind_checkBox | Click → Ind_checkBox_Click | MainWindow.xaml / Ind_checkBox / Command={Binding ToggleUseIndividualFormattingCommand}, IsChecked={Binding SelectedItem.UseIndividualFormatting, Mode=OneWay} |
| IndcbPreviewNotes | Click → IndcbPreviewNotes_Click | MainWindow.xaml / IndcbPreviewNotes / Click=OpenSessionNotes_Click |
| IndradioButtonFormat | Click → IndradioButtonTextFormatInfo_Click | MainWindow.xaml / IndradioButtonFormat / Command={Binding ShowPreviewFormatModeCommand}, IsChecked={Binding IsPreviewFormatMode, Mode=OneWay} |
| IndradioButtonInfo | Click → IndradioButtonTextFormatInfo_Click | MainWindow.xaml / IndradioButtonInfo / Command={Binding ShowPreviewInfoModeCommand}, IsChecked={Binding IsPreviewInfoMode, Mode=OneWay} |
| IndradioButtonText | Click → IndradioButtonTextFormatInfo_Click | MainWindow.xaml / IndradioButtonText / Command={Binding ShowPreviewTextModeCommand}, IsChecked={Binding IsPreviewTextMode, Mode=OneWay} |
| InfoScreenFolder | SelectedIndexChanged → InfoScreenFolder_SelectedIndexChanged | MainWindow.xaml / InfoScreenFolder / ItemsSource={Binding Folders}, SelectedItem={Binding SelectedFolder, Mode=TwoWay} |
| InfoScreenList | MouseUp → InfoScreenList_MouseUp | InlineInfoScreenList |
| InfoScreen_Copy | Click → InfoScreen_EditBtns_Click | CMenuInfoScreenFiles_Copy_Click |
| InfoScreen_Delete | Click → InfoScreen_EditBtns_Click | CMenuInfoScreenFiles_Delete_Click |
| InfoScreen_Edit | Click → InfoScreen_EditBtns_Click | CMenuInfoScreenFiles_Edit_Click |
| InfoScreen_Import | Click → InfoScreen_Import_Click | MainWindow.xaml / InfoScreen_Import / Click=ImportSourceFiles_Click |
| InfoScreen_Move | Click → InfoScreen_EditBtns_Click | MoveInfoScreensAsync |
| InfoScreen_New | Click → InfoScreen_EditBtns_Click | Main_SourceEdit_Click / OpenNoticeScreen |
| InfoScreen_OpenFolder | Click → InfoScreen_OpenFolder_Click | MainWindow.xaml / InfoScreen_OpenFolder / Click=OpenSourceFolder_Click |
| Main_Alerts | Click → Main_Alerts_Click | MainWindow.xaml / Main_Alerts / Click=Main_Alerts_Click |
| Main_Chinese | Click → Main_Chinese_Click | MainWindow.xaml / Main_Chinese / Command={Binding ToggleOutputChineseCommand} |
| Main_Copy | Click → Main_EditBtns_Click | MainWindow.xaml / Main_Copy / Click=Main_SourceEdit_Click |
| Main_Delete | Click → Main_EditBtns_Click | MainWindow.xaml / Main_Delete / Click=Main_SourceEdit_Click |
| Main_Edit | Click → Main_EditBtns_Click | MainWindow.xaml / Main_Edit / Click=Main_SourceEdit_Click |
| Main_Find | Click → Main_Find_Click | MainWindow.xaml / Main_Find / Click=Main_Find_Click |
| Main_JumpA | Click → Main_Jump_Click | MainWindow.xaml / Main_JumpA / Click=FolderJump_Click |
| Main_JumpB | Click → Main_Jump_Click | MainWindow.xaml / Main_JumpB / Click=FolderJump_Click |
| Main_JumpC | Click → Main_Jump_Click | MainWindow.xaml / Main_JumpC / Click=FolderJump_Click |
| Main_Media | Click → Main_Media_Click | MainWindow.xaml / Main_Media / Click=Main_Media_Click |
| Main_Move | Click → Main_EditBtns_Click | MainWindow.xaml / Main_Move / Click=Main_SourceEdit_Click |
| Main_New | Click → Main_EditBtns_Click | MainWindow.xaml / Main_New / Click=Main_SourceEdit_Click |
| Main_NoRotate | Click → Main_NoRotate_Click | MainWindow.xaml / Main_NoRotate / IsChecked={Binding IsNoRotateChecked, Mode=OneWay}, Command={Binding ToggleAutoRotateCommand} |
| Main_Options | Click → Main_Options_Click | MainWindow.xaml / Main_Options / Click=OpenSettings_Click |
| Main_Refresh | Click → Main_Refresh_Click | MainWindow.xaml / Main_Refresh / Click=Main_Refresh_Click |
| Main_RotateStyle | DropDownItemClicked → Main_RotateStyle_DropDownItemClicked | MainWindow.xaml / Main_RotateStyle / ItemsSource={Binding AutoRotateModeOptions} |
| MediaFolder | SelectedIndexChanged → MediaFolder_SelectedIndexChanged | MainWindow.xaml / MediaFolder / ItemsSource={Binding FolderGroups}, SelectedItem={Binding SelectedFolder, Mode=TwoWay} |
| MediaList | MouseUp → MediaList_MouseUp | MediaLibraryWindow.xaml / MediaList / ItemsSource={Binding MediaFiles}, SelectedItem={Binding SelectedFile, Mode=TwoWay}, MouseDoubleClick=MediaList_MouseDoubleClick |
| Media_Import | Click → Media_Import_Click | MainWindow.xaml / Media_Import / Click=ImportSourceFiles_Click |
| Media_OpenFolder | Click → Media_OpenFolder_Click | MainWindow.xaml / Media_OpenFolder / Click=OpenSourceFolder_Click |
| Menu_About | Click → Menu_About_Click | OpenAbout_Click |
| Menu_AddSong | Click → Menu_AddSong_Click | Main_SourceEdit_Click / Main_New |
| Menu_AddToUsages | Click → Menu_AddToUsages_Click | AddListUsages_Click |
| Menu_BlackScreen | Click → Menu_BlackScreen_Click | {Binding ToggleOutputBlackCommand} |
| Menu_ClearAllFormatting | Click → Menu_ClearAllFormatting_Click | ClearDatabaseFormats_Click |
| Menu_ClearRegistrySettings | Click → Menu_ClearRegistrySettings_Click | ResetAllSettings_Click |
| Menu_ClearScreen | Click → Menu_ClearScreen_Click | {Binding ToggleOutputClearCommand} |
| Menu_Compact | Click → Menu_Compact_Click | {Binding Library.CompactDatabaseCommand} |
| Menu_Contents | Click → Menu_Contents_Click | HelpContents_Click |
| Menu_CopySong | Click → Menu_CopySong_Click | Main_SourceEdit_Click / Main_Copy |
| Menu_DeleteSong | Click → Menu_DeleteSong_Click | Main_SourceEdit_Click / Main_Delete |
| Menu_EasiSlidesFolder | Click → Menu_EasiSlidesFolder_Click | OpenEasiSlidesFolder_Click |
| Menu_EditSong | Click → Menu_EditSong_Click | Main_SourceEdit_Click / Main_Edit |
| Menu_Empty | Click → Menu_Empty_Click | EmptyDeleted_Click |
| Menu_Exit | Click → Menu_Exit_Click | Exit_Click |
| Menu_Export | Click → Menu_Export_Click | OpenExport_Click |
| Menu_Find | Click → Menu_Find_Click | Main_Find_Click |
| Menu_GoLiveWithPreview | Click → Menu_PreviewGoLiveNext_Click | {Binding SendToOutputAndNextCommand} |
| Menu_HelpWeb | Click → Menu_HelpWeb_Click | WebHelp_Click |
| Menu_Import | Click → Menu_Import_Click | OpenImportExport_Click |
| Menu_ImportFolder | Click → Menu_ImportFolder_Click | OpenFolderImport_Click |
| Menu_ListingOfSelectedFolder | Click → Menu_ListingOfSelectedFolder_Click | ExportFolderListing_Click |
| Menu_MoveSong | Click → Menu_MoveSong_Click | Main_SourceEdit_Click / Main_Move |
| Menu_Options | Click → Menu_Options_Click | OpenSettings_Click |
| Menu_PraiseBookTemplates | Click → Menu_PraiseBooks_Click | OpenPraiseBookIndex_Click |
| Menu_PreviewNotations | Click → Menu_PreviewNotations_Click | {Binding PreviewAreaShowNotations, Mode=TwoWay} |
| Menu_ReArrangeSongFolders | Click → Menu_ReArrangeSongFolders_Click | FolderEditor_Click |
| Menu_Recover | Click → Menu_Recover_Click | SongRecover_Click |
| Menu_Refresh | Click → Menu_Refresh_Click | Main_Refresh_Click |
| Menu_RefreshOutput | Click → Menu_RefreshOutput_Click | {Binding RefreshOutputCommand} |
| Menu_Register | Click → Menu_Register_Click | OpenRegistration_Click |
| Menu_RestartCurrentItem | Click → Menu_RestartCurrentItem_Click | {Binding RestartCurrentItemCommand} |
| Menu_SelectAll | Click → Menu_SelectAll_Click | CMenuSongs_SelectAll_Click |
| Menu_SmartMerge | Click → Menu_SmartMerge_Click | SongMerge_Click |
| Menu_StartShow | Click → Menu_StartShow_Click | {Binding ToggleOutputLiveCommand} |
| Menu_StatusBar | Click → Menu_StatusBar_Click | ShowStatusMenu |
| Menu_UseSongNumbering | Click → Menu_useSongNumbering_Click | {Binding Library.UseSongNumbering, Mode=TwoWay} |
| Menu_ViewUsages | Click → Menu_ViewUsages_Click | OpenUsageReport_Click |
| Menu_WorshipSessions | Click → Menu_WorshipLists_Click | OpenManageWorshipLists_Click |
| OutputBtnItemDown | Click → OutputBtnUpDown_Click | MainWindow.xaml / OutputBtnItemDown / Command={Binding NextOutputItemCommand} |
| OutputBtnItemUp | Click → OutputBtnUpDown_Click | MainWindow.xaml / OutputBtnItemUp / Command={Binding PreviousOutputItemCommand} |
| OutputBtnJumpToNonRotate | Click → OutputBtnJumpToNonRotate_Click | MainWindow.xaml / OutputBtnJumpToNonRotate / Command={Binding JumpToNextNonRotateOutputItemCommand} |
| OutputBtnLMClear | Click → OutputBtnLMClear_Click | MainWindow.xaml / OutputBtnLMClear / Command={Binding ClearLiveMessageCommand} |
| OutputBtnLMSend | Click → OutputBtnLMSend_Click | MainWindow.xaml / OutputBtnLMSend / Command={Binding SendLiveMessageCommand} |
| OutputBtnMedia | Click → OutputBtnMedia_Click | MainWindow.xaml / OutputBtnMediaInline / Command={Binding PlayOutputMediaCommand}, Click=FocusOutputArea_Click |
| OutputBtnRefAlert | Click → OutputBtnRefAlert_Click | MainWindow.xaml / OutputBtnRefAlert / Command={Binding ToggleOutputReferenceAlertCommand}, IsChecked={Binding IsOutputReferenceAlertActive, Mode=OneWay} |
| OutputBtnSlideDown | Click → OutputBtnUpDown_Click | MainWindow.xaml / OutputBtnSlideDown / Command={Binding NextOutputSlideCommand} |
| OutputBtnSlideUp | Click → OutputBtnUpDown_Click | MainWindow.xaml / OutputBtnSlideUp / Command={Binding PreviousOutputSlideCommand} |
| OutputBtnVerse1 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerse1 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerse2 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerse2 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerse3 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerse3 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerse4 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerse4 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerse5 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerse5 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerse6 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerse6 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerse7 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerse7 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerse8 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerse8 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerse9 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerse9 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerseBridge | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerseBridge / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerseBridge2 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerseBridge2 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerseChorus | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerseChorus / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerseChorus2 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerseChorus2 / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVerseEnding | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVerseEnding / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVersePreChorus | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVersePreChorus / Command={Binding JumpToOutputLyricsSectionCommand} |
| OutputBtnVersePreChorus2 | Click → OutputBtnVerse_Click | MainWindow.xaml / OutputBtnVersePreChorus2 / Command={Binding JumpToOutputLyricsSectionCommand} |
| PB_Add | MouseUp → PB_Btn_MouseUp | MainWindow.xaml / PB_Add / Click=InlinePraiseBookAddSelected_Click |
| PB_Delete | MouseUp → PB_Btn_MouseUp | MainWindow.xaml / PB_Delete / Click=InlinePraiseBookDeleteSelected_Click |
| PB_Html | MouseUp → PB_Btn_MouseUp | MainWindow.xaml / PB_Html / Click=InlinePraiseBookExportHtml_Click |
| PB_Manage | MouseUp → PB_Btn_MouseUp | MainWindow.xaml / PB_Manage / Click=OpenPraiseBookIndex_Click |
| PB_Word | MouseUp → PB_Btn_MouseUp | MainWindow.xaml / PB_Word / Click=InlinePraiseBookExportRtf_Click |
| PB_WordCount | MouseUp → PB_Btn_MouseUp | MainWindow.xaml / PB_WordCount / Click=PB_WordCount_Click, IsChecked={Binding IsWordCountSortEnabled, Mode=OneWay} |
| PP_Import | Click → PP_Import_Click | MainWindow.xaml / PP_Import / Click=ImportSourceFiles_Click |
| PP_ListType | DropDownItemClicked → PP_Style_DropDownItemClicked | MainWindow.xaml / PP_ListType |
| PP_OpenFolder | Click → PP_Btn_Click | MainWindow.xaml / PP_OpenFolder / Click=OpenSourceFolder_Click |
| PowerpointFolder | SelectedIndexChanged → PowerpointFolder_SelectedIndexChanged | MainWindow.xaml / PowerpointFolder / ItemsSource={Binding FolderGroups}, SelectedItem={Binding SelectedFolder, Mode=TwoWay} |
| PowerpointList | MouseUp → PowerpointList_MouseUp | MainWindow.xaml / InlinePowerPointList / ItemsSource={Binding Presentations}, SelectedItem={Binding SelectedFile, Mode=TwoWay}, SelectionChanged=InlinePowerPointList_SelectionChanged |
| Powerpoint_Copy | Click → Powerpoint_EditBtns_Click | CMenuPowerPointFiles_Copy_Click |
| Powerpoint_Delete | Click → Powerpoint_EditBtns_Click | CMenuPowerPointFiles_Delete_Click |
| Powerpoint_Edit | Click → Powerpoint_EditBtns_Click | CMenuPowerPointFiles_Edit_Click |
| Powerpoint_Move | Click → Powerpoint_EditBtns_Click | Main_SourceEdit_Click / ExternalFileOperationKind.Move |
| PraiseBook | SelectedIndexChanged → PraiseBook_SelectedIndexChanged | MainWindow.xaml / PraiseBookTab |
| PraiseBookItems | MouseUp → PraiseBookItems_MouseUp | MainWindow.xaml / PraiseBookItems / ItemsSource={Binding Entries}, MouseDoubleClick=InlinePraiseBookItems_MouseDoubleClick, KeyDown=PraiseBookItems_KeyDown |
| PreviewBtnItemDown | Click → PreviewBtnUpDown_Click | MainWindow.xaml / PreviewBtnItemDown / Command={Binding NextPreviewItemCommand} |
| PreviewBtnItemUp | Click → PreviewBtnUpDown_Click | MainWindow.xaml / PreviewBtnItemUp / Command={Binding PreviousPreviewItemCommand} |
| PreviewBtnSlideDown | Click → PreviewBtnUpDown_Click | MainWindow.xaml / PreviewBtnSlideDown / Command={Binding NextSlideCommand} |
| PreviewBtnSlideUp | Click → PreviewBtnUpDown_Click | MainWindow.xaml / PreviewBtnSlideUp / Command={Binding PreviousSlideCommand} |
| PreviewBtnVerse1 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerse1 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerse2 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerse2 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerse3 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerse3 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerse4 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerse4 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerse5 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerse5 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerse6 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerse6 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerse7 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerse7 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerse8 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerse8 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerse9 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerse9 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerseBridge | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerseBridge / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerseBridge2 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerseBridge2 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerseChorus | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerseChorus / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerseChorus2 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerseChorus2 / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVerseEnding | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVerseEnding / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVersePreChorus | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVersePreChorus / Command={Binding JumpToLyricsSectionCommand} |
| PreviewBtnVersePreChorus2 | Click → PreviewBtnVerse_Click | MainWindow.xaml / PreviewBtnVersePreChorus2 / Command={Binding JumpToLyricsSectionCommand} |
| SongFolder | SelectedIndexChanged → SongFolder_SelectedIndexChanged | MainWindow.xaml / ClassicSongFolderCombo / ItemsSource={Binding Library.Folders}, SelectedItem={Binding Library.SelectedFolder, Mode=TwoWay} |
| SongsList | MouseUp → SongsList_MouseUp | MainWindow.xaml / LibrarySongList / SelectionChanged=LibrarySongList_SelectionChanged, ItemsSource={Binding Library.Songs}, SelectedItem={Binding Library.SelectedSong, Mode=TwoWay} |
| TabBibleVersions | Click → TabBibleVersions_Click | MainWindow.xaml / TabBibleVersions / ItemsSource={Binding Bible.Versions}, SelectedItem={Binding Bible.SelectedVersion, Mode=TwoWay} |
| WL_Add | MouseUp → WL_Btn_MouseUp | WorshipListPanel.xaml / WL_Add / Click=WL_Add_Click |
| WL_Delete | MouseUp → WL_Btn_MouseUp | WorshipListPanel.xaml / WL_Delete / Command={Binding RemoveSelectedItemCommand} |
| WL_Down | MouseUp → WL_Btn_MouseUp | WorshipListPanel.xaml / WL_Down / Command={Binding MoveSelectedItemDownCommand} |
| WL_Manage | MouseUp → WL_Btn_MouseUp | WorshipListPanel.xaml / WL_Manage / Click=WL_Manage_Click |
| WL_Notes | MouseUp → WL_Btn_MouseUp | WorshipListPanel.xaml / WL_Notes / Click=WL_Notes_Click |
| WL_Open | MouseUp → WL_Btn_MouseUp | WorshipListPanel.xaml / WL_Open / Click=WL_Open_Click |
| WL_Up | MouseUp → WL_Btn_MouseUp | WorshipListPanel.xaml / WL_Up / Command={Binding MoveSelectedItemUpCommand} |
| WL_Word | MouseUp → WL_Btn_MouseUp | WorshipListPanel.xaml / WL_Word / Click=WL_Word_Click |
| WorshipListItems | DoubleClick → WorshipListItems_DoubleClick<br>MouseUp → WorshipList_MouseUp | WorshipListPanel.xaml / QueueList / ItemsSource={Binding Queue}, SelectedItem={Binding SelectedItem, Mode=TwoWay}, KeyDown=QueueList_KeyDown |
| btnToLive | Click → btnToLive_Click | MainWindow.xaml / btnToLive / Command={Binding PreviewToLiveCommand} |
| btnToOutput | Click → btnToOutput_Click | MainWindow.xaml / btnToOutput / Command={Binding CopyPreviewToOutputCommand} |
| btnToOutputMoveNext | Click → btnToOutputMoveNext_Click | MainWindow.xaml / btnToOutputMoveNext / Command={Binding CopyPreviewToOutputAndNextCommand}, Click=FocusOutputArea_Click |
| cbGoLive | Click → cbGoLive_Click | MainWindow.xaml / cbGoLive / Command={Binding ToggleOutputLiveCommand}, IsChecked={Binding IsOutputLiveActive, Mode=OneWay} |
| cbOutputBlack | Click → cbOutputBlack_Click | MainWindow.xaml / cbOutputBlack / Command={Binding ToggleOutputBlackCommand}, IsChecked={Binding IsOutputBlackActive, Mode=OneWay} |
| cbOutputClear | Click → cbOutputClear_Click | MainWindow.xaml / cbOutputClear / Command={Binding ToggleOutputClearCommand}, IsChecked={Binding IsOutputClearActive, Mode=OneWay} |
| flowLayoutOutputLyrics | Click → flowLayoutOutputLyrics_Click | MainWindow.xaml / flowLayoutOutputLyrics / ItemsSource={Binding OutputLyricsPages} |
| flowLayoutPreviewLyrics | Click → flowLayoutPreviewLyrics_Click | MainWindow.xaml / flowLayoutPreviewLyrics / ItemsSource={Binding PreviewLyricsPages} |
| tabControlLists | SelectedIndexChanged → tabControlLists_SelectedIndexChanged | MainWindow.xaml / LeftListTabs / SelectionChanged=LeftListTabs_SelectionChanged |
| tabControlSource | SelectedIndexChanged → tabControlSource_SelectedIndexChanged | MainWindow.xaml / LeftBrowserTabs / SelectionChanged=LeftBrowserTabs_SelectionChanged |
