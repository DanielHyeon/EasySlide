using Easislides.Module;
using Easislides.Util;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Easislides
{
    /// <summary>
    /// 키보드 액션 처리 핸들러
    /// FrmMain의 키보드 이벤트 로직을 캡슐화
    /// </summary>
    public class KeyboardActionHandler
    {
        private readonly FrmMain _form;

        public KeyboardActionHandler(FrmMain form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
        }

        /// <summary>
        /// PowerPoint 썸네일 마우스 클릭 처리
        /// </summary>
        public void HandlePowerPointThumbnailClick(Control control, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            control.Focus();

            int slideNumber = DataUtil.ObjToInt(control.Tag) + 1;
            Debug.WriteLine($"PowerPointImage_Click name={control.Name} slide={slideNumber} focus={_form.ActiveControl?.Name}");

            // 슬라이드 변경
            SongSettings targetItem;
            if (control.Name == "PP_Preview")
            {
                targetItem = gf.PreviewItem;
            }
            else // PP_Output
            {
                targetItem = gf.OutputItem;
            }

            if (targetItem.CurSlide != slideNumber)
            {
                targetItem.CurSlide = slideNumber;
                _form.MoveToSlide(targetItem, KeyDirection.Refresh);
            }
        }

        /// <summary>
        /// PowerPoint 썸네일 더블클릭 처리 (애니메이션/비디오 재생)
        /// </summary>
        public void HandlePowerPointThumbnailDoubleClick(Control control)
        {
            int slideNumber = DataUtil.ObjToInt(control.Tag) + 1;
            Debug.WriteLine($"PowerPointImage_DoubleClick name={control.Name} slide={slideNumber}");

            // 슬라이드 번호 설정
            if (control.Name == "PP_Preview")
            {
                gf.PreviewItem.CurSlide = slideNumber;
            }
            else
            {
                gf.OutputItem.CurSlide = slideNumber;
            }

            // PowerPoint 애니메이션/비디오 트리거 (FrmMain을 통해 접근)
            _form.PlayPowerPointAnimation(slideNumber);
        }

        /// <summary>
        /// PowerPoint 썸네일 컨테이너(FlowLayoutPanel) 키보드 입력 처리
        /// </summary>
        public void HandlePowerPointContainerKeyUp(string panelName, Keys keyCode, bool shiftKey)
        {
            Debug.WriteLine($"PowerPointContainer_KeyUp panel={panelName} key={keyCode} shift={shiftKey}");

            // Preview와 Output 구분
            SongSettings targetItem = panelName == "flowLayoutPreviewPowerPoint"
                ? gf.PreviewItem
                : gf.OutputItem;

            // 키 재매핑
            gf.ReMapKeyBoard(ref keyCode);

            // 네비게이션 키 처리
            if (KeyboardMapping.IsNavigationKey(keyCode))
            {
                HandleNavigationKey(targetItem, keyCode);
                return;
            }

            // 찬양 구절 점프 키 처리
            if (KeyboardMapping.IsVerseJumpKey(keyCode, shiftKey))
            {
                HandleVerseJumpKey(targetItem, keyCode, shiftKey);
                return;
            }

            // 숫자 키 처리 (C 키 포함)
            if (KeyboardMapping.IsNumericKey(keyCode) || keyCode == Keys.C)
            {
                HandleNumericKey(targetItem, keyCode);
                return;
            }

            // 기능 키 처리
            if (KeyboardMapping.IsFunctionKey(keyCode))
            {
                HandleFunctionKey(targetItem, keyCode);
            }
        }

        /// <summary>
        /// 네비게이션 키 처리 (방향키, Home, End, PageUp, PageDown 등)
        /// </summary>
        public void HandleNavigationKey(SongSettings item, Keys key)
        {
            switch (key)
            {
                case Keys.Home:
                    _form.ManualMoveToItem(item, KeyDirection.FirstOne);
                    break;
                case Keys.Prior:  // Page Up
                    _form.ManualMoveToItem(item, KeyDirection.PrevOne);
                    break;
                case Keys.Next:  // Page Down
                    _form.ManualMoveToItem(item, KeyDirection.NextOne);
                    break;
                case Keys.End:
                    _form.ManualMoveToItem(item, KeyDirection.LastOne);
                    break;
                case Keys.Tab:
                    _form.ManualMoveToItem(item, KeyDirection.NextOne);
                    break;
                case Keys.Up:
                    if (!gf.GlobalHookKey_CtrlArrow && !gf.GlobalHookKey_Arrow)
                    {
                        _form.MoveToSlide(item, KeyDirection.PrevOne);
                    }
                    break;
                case Keys.Left:
                    _form.MoveToSlide(item, KeyDirection.FirstOne);
                    break;
                case Keys.Right:
                    _form.MoveToSlide(item, KeyDirection.LastOne);
                    break;
                case Keys.Down:
                    if (!gf.GlobalHookKey_CtrlArrow && !gf.GlobalHookKey_Arrow)
                    {
                        _form.MoveToSlide(item, KeyDirection.NextOne);
                    }
                    break;
                case Keys.Space:
                    _form.MoveToSlide(item, KeyDirection.SpaceOne);
                    break;
            }
        }

        /// <summary>
        /// 찬양 구절 점프 키 처리 (B, W, P, Q, E, T)
        /// </summary>
        public void HandleVerseJumpKey(SongSettings item, Keys key, bool shiftKey)
        {
            int verseType = GetVerseTypeFromKey(key, shiftKey);
            if (verseType > 0)
            {
                _form.JumpToVerseType(item, verseType);
            }
        }

        /// <summary>
        /// 숫자 키 처리 (0-9, NumPad 0-9)
        /// </summary>
        public void HandleNumericKey(SongSettings item, Keys key)
        {
            Keys normalizedKey = KeyboardMapping.NormalizeNumericKey(key);

            // C 키도 0으로 처리
            if (key == Keys.C)
            {
                normalizedKey = Keys.D0;
            }

            if (normalizedKey >= Keys.D0 && normalizedKey <= Keys.D9)
            {
                int verseIndex = normalizedKey - Keys.D0;

                if (item.SongVerses[verseIndex] > 0)
                {
                    item.CurSlide = item.SongVerses[verseIndex];
                    _form.MoveToSlide(item, KeyDirection.Refresh);
                }
            }
        }

        /// <summary>
        /// 기능 키 처리 (G, Z, A, J, M)
        /// </summary>
        public void HandleFunctionKey(SongSettings item, Keys key)
        {
            switch (key)
            {
                case Keys.G:
                    ToggleGapOption();
                    break;
                case Keys.Z:
                    if (item.OutputStyleScreen)
                    {
                        _form.QueryShowActive();
                    }
                    break;
                case Keys.A:
                    _form.SetRotateState(!gf.AutoRotateOn);
                    break;
                case Keys.J:
                    _form.GotoNextNonRotateItem(item);
                    break;
                case Keys.M:
                    if (item.OutputStyleScreen)
                    {
                        _form.RemoteControlLiveShow(FrmMain.LiveShowAction.Remote_MediaItemPausePlay);
                    }
                    break;
            }
        }

        /// <summary>
        /// 전역 기능 키 처리 (F7-F10)
        /// </summary>
        public void HandleGlobalFunctionKey(Keys key)
        {
            switch (key)
            {
                case Keys.F7 when gf.GlobalHookKey_F7:
                    if (gf.PreviewItem.ItemID != "")
                    {
                        _form.CopyPreviewToOutput();
                        _form.BeginInvoke(new Action(() => { _form.LiveBlack(gf.ShowLiveBlack = false); }));
                    }
                    break;
                case Keys.F8 when gf.GlobalHookKey_F8:
                    if (gf.PreviewItem.ItemID != "")
                    {
                        _form.CopyPreviewToOutput();
                    }
                    break;
                case Keys.F9 when gf.GlobalHookKey_F9:
                    _form.LiveBlack(!gf.ShowLiveBlack);
                    break;
                case Keys.F10 when gf.GlobalHookKey_F10:
                    _form.LiveBlack(!gf.ShowLiveBlack);
                    break;
            }
        }

        /// <summary>
        /// 전역 화살표 키 처리
        /// </summary>
        public void HandleGlobalArrowKey(Keys key, bool controlPressed)
        {
            bool shouldHandle = gf.GlobalHookKey_Arrow || (controlPressed && gf.GlobalHookKey_CtrlArrow);

            if (!shouldHandle)
                return;

            switch (key)
            {
                case Keys.Up:
                    _form.MoveToSlide(gf.OutputItem, KeyDirection.PrevOne);
                    break;
                case Keys.Down:
                    _form.MoveToSlide(gf.OutputItem, KeyDirection.NextOne);
                    break;
            }
        }

        /// <summary>
        /// 키와 Shift 상태로부터 구절 타입 결정
        /// </summary>
        private int GetVerseTypeFromKey(Keys key, bool shiftKey)
        {
            return key switch
            {
                Keys.B when shiftKey => KeyboardMapping.VerseTypes.Worship,
                Keys.B => KeyboardMapping.VerseTypes.Bridge,
                Keys.W when shiftKey => KeyboardMapping.VerseTypes.Worship,
                Keys.P when shiftKey => KeyboardMapping.VerseTypes.PreChorus2,
                Keys.P => KeyboardMapping.VerseTypes.PreChorus,
                Keys.Q when shiftKey => KeyboardMapping.VerseTypes.PreChorus2,
                Keys.E => KeyboardMapping.VerseTypes.Ending,
                Keys.T when shiftKey => KeyboardMapping.VerseTypes.Tag,
                _ => 0
            };
        }

        /// <summary>
        /// Gap 옵션 토글
        /// </summary>
        private void ToggleGapOption()
        {
            if (gf.GapItemOption == GapType.None)
            {
                gf.GapItemOption = gf.AltGapItemOption;
                gf.AltGapItemOption = GapType.None;
            }
            else
            {
                gf.AltGapItemOption = gf.GapItemOption;
                gf.GapItemOption = GapType.None;
            }
            _form.ShowStatusBarSummary();
        }
    }
}
