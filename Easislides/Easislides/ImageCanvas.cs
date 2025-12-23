using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Easislides
{
	internal class ImageCanvas : Control
	{
		private Image _image;

		private int _PosLeft = 0;

		private int _PosTop = 0;

		private int _PosWidth = 0;

		private int _PosHeight = 0;

		private string _FileName = "";

		private float _ImageRatio = 1f;

		private bool _PowerPoint = false;

		private int _SlideNumber = 0;

		private bool _ImageDone = false;

		public float BorderWidthFactor = 0f;

		public int PPSlideNumbering = 0;

		public bool ExternalPP = false;

		public int CurSelectedSlide = 0;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image image
		{
			get
			{
				return _image;
			}
			set
			{
				_image = value;
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PosLeft
		{
			get
			{
				return _PosLeft;
			}
			set
			{
				_PosLeft = value;
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PosTop
		{
			get
			{
				return _PosTop;
			}
			set
			{
				_PosTop = value;
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PosWidth
		{
			get
			{
				return _PosWidth;
			}
			set
			{
				_PosWidth = value;
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PosHeight
		{
			get
			{
				return _PosHeight;
			}
			set
			{
				_PosHeight = value;
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FileName
		{
			get
			{
				return _FileName;
			}
			set
			{
				_FileName = value;
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float ImageRatio
		{
			get
			{
				return _ImageRatio;
			}
			set
			{
				_ImageRatio = value;
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool PowerPoint
		{
			get
			{
				return _PowerPoint;
			}
			set
			{
				_PowerPoint = value;
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SlideNumber
		{
			get
			{
				return _SlideNumber;
			}
			set
			{
				_SlideNumber = value;
			}
		}

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ImageDone
		{
			get
			{
				return _ImageDone;
			}
			set
			{
				_ImageDone = value;
			}
		}

		public void SetImageRatio(int InImageWidth, int InImageHeight)
		{
			ImageRatio = (float)InImageWidth / (float)InImageHeight;
		}

		public void ResizeCanvas_Old(int InCanvasWidth, int InCanvasHeight)
		{
            base.Width = InCanvasWidth;
            base.Height = InCanvasHeight;

			if (ImageDone && _image != null)
			{
				float num = (float)base.Width / (float)base.Height;
				ImageRatio = (float)_image.Width / (float)_image.Height;
				if (num < ImageRatio)
				{
					PosLeft = 0;
					PosWidth = base.Width;
					PosHeight = (int)((float)PosWidth / ImageRatio);
					PosTop = (base.Height - PosHeight) / 2;
				}
				else
				{
					PosHeight = base.Height;
					PosTop = 0;
					PosWidth = (int)((float)PosHeight * ImageRatio);
					PosLeft = (base.Width - PosWidth) / 2;
				}
				ImageDone = true;
			}
		}

        public void ResizeCanvas(int InCanvasWidth, int InCanvasHeight)
        {
            // ✅ 이미지가 `null`이면 오류 방지
            if (_image == null)
            {
                Console.WriteLine("Error: _image is null in ResizeCanvas.");
                return;
            }

            try
            {
                base.Width = InCanvasWidth;
                base.Height = InCanvasHeight;

                float canvasRatio = (float)base.Width / base.Height;
                float imageRatio = (float)_image.Width / _image.Height;  // ✅ 여기서 예외 발생 가능

                // ✅ 비율을 유지하면서 캔버스 크기에 맞게 자동 조정
                if (canvasRatio < imageRatio)
                {
                    PosWidth = base.Width;
                    PosHeight = (int)Math.Round(PosWidth / imageRatio);
                    PosLeft = 0;
                    PosTop = (base.Height - PosHeight) / 2;
                }
                else
                {
                    PosHeight = base.Height;
                    PosWidth = (int)Math.Round(PosHeight * imageRatio);
                    PosTop = 0;
                    PosLeft = (base.Width - PosWidth) / 2;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ResizeCanvas: {ex.Message}");
            }
        }



        public ImageCanvas()
		{
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			Cursor = Cursors.Hand;
			base.Visible = false;
			FileName = "";
			ImageRatio = 1f;
		}

		//protected override void OnPaint(PaintEventArgs e)
		//{
		//	if ((_image != null || !(FileName == "")) && base.Visible)
		//	{
		//		if (ImageDone)
		//		{
		//			Draw_image(e);
		//		}
		//		else if (Calc_Image())
		//		{
		//			Draw_image(e);
		//		}
		//	}
		//}

        protected override void OnPaint(PaintEventArgs e)
        {
            if ((_image == null || string.IsNullOrEmpty(FileName)) && !base.Visible)
                return; // 불필요한 렌더링 방지

            // 🔹 고해상도 렌더링을 위한 `Graphics` 설정
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (!ImageDone && !Calc_Image())
                return; // `Calc_Image()`가 실패하면 렌더링 중단

            Draw_image(e); // 최종적으로 이미지 렌더링
        }

        private void Draw_image(PaintEventArgs e)
		{
			e.Graphics.Clear(BackColor);
			e.Graphics.DrawImage(_image, new Rectangle(_PosLeft, _PosTop, _PosWidth, _PosHeight), new Rectangle(0, 0, _image.Width, _image.Height), GraphicsUnit.Pixel);
		}

		public void BuildNewImageThumbs_Old(int NewCanvasIndex, int NewWidth, int NewHeight, ref string[] NewImageArray, int NewTotalCount, string NewPrefix, int NewCurSelectedSlide, Color NewBackColour, ToolTip NewToolTip, bool NewExternalPP)
		{
			try
			{
				int pPSlideNumbering = NewCanvasIndex + 1;
                string extension = "";
				string text = "";
				string text2 = NewImageArray[NewCanvasIndex];
				base.Width = NewWidth;
                base.Height = NewHeight;

				ExternalPP = NewExternalPP;
				CurSelectedSlide = NewCurSelectedSlide;
				if (PowerPoint)
				{
					text = PPSlideNumbering.ToString();
					SlideNumber = PPSlideNumbering;
					goto IL_0108;
				}
				text = gf.GetDisplayNameOnly(ref NewImageArray[NewCanvasIndex], UpdateByRef: false, KeepExt: true);
				SlideNumber = 0;
                //if (DataUtil.Right(text2, 4) == ".ppt")
                extension = Path.GetExtension(text2).ToLower();
                if (extension == ".ppt"|| extension == ".pptx")
                {
					text2 = gf.ExtPPrefix + gf.ExtPPrefix_Num + "\\" + Path.GetFileNameWithoutExtension(text2) + ".jpg";
				}
				if (!(FileName == text2) || ExternalPP)
				{
					goto IL_0108;
				}
				ResizeCanvas(base.Width, base.Height);
				goto end_IL_0001;
				IL_0108:
				NewToolTip.SetToolTip(this, text);
				PPSlideNumbering = pPSlideNumbering;
				BackColor = NewBackColour;
				BorderWidthFactor = 0.04f;
				ImageDone = false;
				FileName = text2;
				base.Visible = true;
				Invalidate();
				end_IL_0001:;
			}
			catch
			{
			}
		}

        public void BuildNewImageThumbs(int NewCanvasIndex, int NewWidth, int NewHeight, ref string[] NewImageArray,
                                int NewTotalCount, string NewPrefix, int NewCurSelectedSlide, Color NewBackColour,
                                ToolTip NewToolTip, bool NewExternalPP)
        {
            try
            {
                int pPSlideNumbering = NewCanvasIndex + 1;
                string extension = Path.GetExtension(NewImageArray[NewCanvasIndex]).ToLower();
                string text2 = NewImageArray[NewCanvasIndex];

                base.Width = NewWidth;  
                base.Height = NewHeight;

                ExternalPP = NewExternalPP;
                CurSelectedSlide = NewCurSelectedSlide;

                if (PowerPoint)
                {
                    SlideNumber = PPSlideNumbering;
                }
                else
                {
                    SlideNumber = 0;
                    if (extension == ".ppt" || extension == ".pptx")
                    {
                        text2 = $"{gf.ExtPPrefix}{gf.ExtPPrefix_Num}\\{Path.GetFileNameWithoutExtension(text2)}.jpg";
                    }
                }

                // ✅ 파일명이 변경되었거나 외부 PowerPoint 파일이면 새 이미지 적용
                if (FileName != text2 || ExternalPP)
                {
                    FileName = text2;
                    ImageDone = false;
                    BackColor = NewBackColour;
                    BorderWidthFactor = 0.04f;
                    base.Visible = true;

                    // ✅ 툴팁 설정
                    NewToolTip.SetToolTip(this, SlideNumber > 0 ? SlideNumber.ToString() : text2);

                    PPSlideNumbering = pPSlideNumbering;

                    // ✅ 고해상도 캔버스로 리사이징
                    ResizeCanvas(base.Width, base.Height);

                    Invalidate(); // 다시 그리기 요청
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BuildNewImageThumbs: {ex.Message}");
            }
        }

        /// <summary>
        /// daniel 왼쪽 페이지 숫자 위치를 오른쪽 끝으로 이동
        /// </summary>
        /// <returns></returns>
        public bool Calc_Image_Old()
        {
            int NewImageWidth = 0;
            int NewImageHeight = 0;
            int StoredImageWidth = 0;
            int StoredImageHeight = 0;
            int posWidth = PosWidth;
            int posHeight = PosHeight;
            try
            {
                if (ExternalPP)
                {
                    gf.ExternalPPT.BuildOneFirstScreenDump(PPSlideNumbering);
                }
                Image image = Image.FromFile(FileName);
                SetImageRatio(image.Width, image.Height);
                gf.CalcImageToFit(ImageRatio, base.Width, base.Height, ref NewImageWidth, ref NewImageHeight, ComputeStoredImage: true, ref StoredImageWidth, ref StoredImageHeight);
                if ((NewImageWidth <= 0) | (NewImageHeight <= 0))
                {
                    return false;
                }
                Image image2 = new Bitmap(StoredImageWidth, StoredImageHeight, PixelFormat.Format24bppRgb);
                Graphics graphics = Graphics.FromImage(image2);
                Rectangle rect = new Rectangle(0, 0, StoredImageWidth, StoredImageHeight);
                if (PowerPoint)
                {
                    Font font = new Font("Microsoft Sans Serif", (float)StoredImageWidth / 12.5f);
                    int num = (int)((StoredImageWidth >= StoredImageHeight) ? ((float)StoredImageWidth * BorderWidthFactor) : ((float)StoredImageHeight * BorderWidthFactor));

                    Rectangle rect2 = new Rectangle(num, num, StoredImageWidth - num * 2, StoredImageHeight - num * 2);
                    graphics.DrawImage(image, rect2);

                    string text = PPSlideNumbering.ToString();

                    SizeF sizeF = graphics.MeasureString(text, font);

                    PointF pointF;

					switch (PPSlideNumbering)
                    {
                        case < 10:
                            pointF = new PointF(StoredImageWidth - (num * 2 + 5), num);
                            break;
                        default:
                            pointF = new PointF(StoredImageWidth - (num * 2 + 12), num);
                            break;
                    }

					Font font1 = new Font("Microsoft Sans Serif", (float)StoredImageWidth / 16f);
					SizeF sizeF1 = graphics.MeasureString(text, font1);

					graphics.FillRectangle(new SolidBrush(gf.BlackScreenColour), pointF.X-3, pointF.Y + 1f, sizeF1.Width - 2, sizeF1.Height - 3);
                    graphics.FillRectangle(new SolidBrush(Color.Yellow), pointF.X-4, pointF.Y, sizeF1.Width - 2, sizeF1.Height - 3);
                    graphics.DrawString(text, font1, new SolidBrush(gf.BlackScreenColour), pointF.X-5, pointF.Y);

                    if (CurSelectedSlide == PPSlideNumbering)
                    {
                        graphics.DrawRectangle(new Pen(new SolidBrush(Color.Red), (int)font.Size), rect);
                    }
                    else
                    {
                        graphics.DrawRectangle(new Pen(new SolidBrush(base.Parent.BackColor), (int)font.Size), rect);
                    }
                }
                else if (ExternalPP)
                {
                    Font font = new Font("Microsoft Sans Serif", (float)StoredImageWidth / 12.5f);
                    int num = (int)((StoredImageWidth >= StoredImageHeight) ? ((float)StoredImageWidth * BorderWidthFactor) : ((float)StoredImageHeight * BorderWidthFactor));
                    Rectangle rect2 = new Rectangle(num, num, StoredImageWidth - num * 2, StoredImageHeight - num * 2);
                    graphics.DrawImage(image, rect2);
                    if (CurSelectedSlide == PPSlideNumbering)
                    {
                        graphics.DrawRectangle(new Pen(new SolidBrush(Color.Red), (int)font.Size), rect);
                    }
                    else
                    {
                        graphics.DrawRectangle(new Pen(new SolidBrush(BackColor), (int)font.Size), rect);
                    }
                }
                else
                {
                    graphics.DrawImage(image, rect);
                }
                this.image = image2;
                ImageDone = true;
                ResizeCanvas(base.Width, base.Height);
                //image.Dispose();
                //image2.Dispose();
                //graphics.Dispose();
            }
            catch
            {
                ImageDone = false;
                return false;
            }
            return true;
        }

        public bool Calc_Image()
        {
            int NewImageWidth = 0, NewImageHeight = 0;
            int StoredImageWidth = 0, StoredImageHeight = 0;

            try
            {
                if (ExternalPP)
                {
                    gf.ExternalPPT.BuildOneFirstScreenDump(PPSlideNumbering);
                }

                // 🔹 `Image.FromFile()`을 안전하게 호출하여 손상된 이미지 방지
                using (Image image = Image.FromFile(FileName))
                {
                    SetImageRatio(image.Width, image.Height);
                    gf.CalcImageToFit(ImageRatio, base.Width, base.Height, ref NewImageWidth, ref NewImageHeight, true, ref StoredImageWidth, ref StoredImageHeight);

                    if (NewImageWidth <= 0 || NewImageHeight <= 0)
                    {
                        return false;
                    }

                    // ✅ 해상도 2배 증가하여 FHD~4K 지원
                    StoredImageWidth *= 2;
                    StoredImageHeight *= 2;

                    using (Bitmap image2 = new Bitmap(StoredImageWidth, StoredImageHeight, PixelFormat.Format24bppRgb))
                    using (Graphics graphics = Graphics.FromImage(image2))
                    {
                        // 🔹 고품질 그래픽 설정
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        Rectangle rect = new Rectangle(0, 0, StoredImageWidth, StoredImageHeight);

                        if (PowerPoint || ExternalPP)
                        {
                            float fontSize = StoredImageWidth / 12.5f;
                            using (Font font = new Font("Microsoft Sans Serif", fontSize))
                            {
                                int margin = (StoredImageWidth >= StoredImageHeight) ? (int)(StoredImageWidth * BorderWidthFactor) : (int)(StoredImageHeight * BorderWidthFactor);
                                Rectangle imageRect = new Rectangle(margin, margin, StoredImageWidth - 2 * margin, StoredImageHeight - 2 * margin);

                                graphics.DrawImage(image, imageRect);

                                string text = PPSlideNumbering.ToString();
                                PointF pointF = new PointF(StoredImageWidth - (PPSlideNumbering < 10 ? margin * 2 + 5 : margin * 2 + 12), margin);

                                // ✅ 텍스트 배경을 그리기 전에 텍스트 크기 계산 (2자리 수 이상일 때 위치 및 폰트 크기 보정)
                                using (Font smallFont = new Font("Microsoft Sans Serif", StoredImageWidth / 24f))
                                {
                                    SizeF textSize = graphics.MeasureString(text, smallFont);

                                    // ✅ 텍스트 배경을 올바른 위치에 적용
                                    graphics.FillRectangle(Brushes.Black, pointF.X - 3, pointF.Y + 1, textSize.Width - 2, textSize.Height - 3);
                                    graphics.FillRectangle(Brushes.Yellow, pointF.X - 4, pointF.Y, textSize.Width - 2, textSize.Height - 3);
                                    graphics.DrawString(text, smallFont, Brushes.Black, pointF.X - 5, pointF.Y);
                                }

                                using (Pen pen = new Pen(CurSelectedSlide == PPSlideNumbering ? Color.Red : base.Parent.BackColor, font.Size))
                                {
                                    graphics.DrawRectangle(pen, rect);
                                }
                            }
                        }
                        else
                        {
                            graphics.DrawImage(image, rect);
                        }

                        // ✅ 최종 고해상도 이미지 저장
                        this.image = (Image)image2.Clone();
                    }
                }

                ImageDone = true;
                ResizeCanvas(base.Width, base.Height);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Calc_Image: {ex.Message}");
                ImageDone = false;
                return false;
            }
            return true;
        }
    }
}
