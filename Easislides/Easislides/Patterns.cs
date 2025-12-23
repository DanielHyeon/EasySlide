using System.Drawing;
using System.Drawing.Drawing2D;

namespace Easislides
{
	internal class Patterns
	{
		public enum Styles
		{
			DiagForCentre,
			DiagBackCentre,
			DiagForTopDown,
			DiagBackTopDown,
			ElliTop,
			ElliBottom,
			VertCentre,
			VertTopDown,
			HoriCentre,
			HoriTopDown,
			RectCentre,
			Plain
		}

		private int Width;

		private int Height;

		public int MaxStyleIndex = 11;

		public void Clear(ref Graphics g, Color InColour)
		{
			g.Clear(InColour);
		}

		public void Fill(ref Graphics g, Color Colour1, Color Colour2, int InStyle, int InWidth, int InHeight, ref string BackgroundID)
		{
			Width = InWidth;
			Height = InHeight;
			Color color = Colour1;
			if (Colour2 == Colour1)
			{
				Colour1 = Lighter(Colour1);
				Colour2 = Darker(Colour1);
			}
			BackgroundID = Colour1.ToString() + Colour2.ToString() + InStyle;
			try
			{
				switch (InStyle)
				{
				case 0:
					FillDiagonalCentre(ref g, Colour1, Colour2, LinearGradientMode.ForwardDiagonal);
					break;
				case 2:
					FillDiagonalTopDown(ref g, Colour1, Colour2, LinearGradientMode.ForwardDiagonal);
					break;
				case 1:
					FillDiagonalCentre(ref g, Colour1, Colour2, LinearGradientMode.BackwardDiagonal);
					break;
				case 3:
					FillDiagonalTopDown(ref g, Colour1, Colour2, LinearGradientMode.BackwardDiagonal);
					break;
				case 6:
					FillHortVertCentre(ref g, Colour1, Colour2, LinearGradientMode.Vertical);
					break;
				case 4:
					FillEllipse(ref g, Colour1, Colour2, AtBottom: true);
					break;
				case 5:
					FillEllipse(ref g, Colour1, Colour2, AtBottom: false);
					break;
				case 7:
					FillHortVertTopDown(ref g, Colour1, Colour2, LinearGradientMode.Vertical);
					break;
				case 8:
					FillHortVertCentre(ref g, Colour1, Colour2, LinearGradientMode.Horizontal);
					break;
				case 9:
					FillHortVertTopDown(ref g, Colour1, Colour2, LinearGradientMode.Horizontal);
					break;
				case 10:
					FillRect(ref g, Colour1, Colour2);
					break;
				default:
					g.Clear(color);
					break;
				}
			}
			catch
			{
				g.Clear(color);
			}
		}

		public void FillDiagonalCentre(ref Graphics g, Color Colour1, Color Colour2, LinearGradientMode GradientMode)
		{
			Rectangle rect = new Rectangle(0, 0, Width, Height);
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, Colour1, Colour2, GradientMode);
			ColorBlend colorBlend = new ColorBlend(4);
			colorBlend.Colors[0] = Colour2;
			colorBlend.Colors[1] = Colour1;
			colorBlend.Colors[2] = Colour1;
			colorBlend.Colors[3] = Colour2;
			colorBlend.Positions[0] = 0f;
			colorBlend.Positions[1] = 0.5f;
			colorBlend.Positions[2] = 0.55f;
			colorBlend.Positions[3] = 1f;
			linearGradientBrush.InterpolationColors = colorBlend;
			g.FillRectangle(linearGradientBrush, rect);
		}

		public void FillDiagonalTopDown(ref Graphics g, Color Colour1, Color Colour2, LinearGradientMode GradientMode)
		{
			Rectangle rect = new Rectangle(0, 0, Width, Height);
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, Colour1, Colour2, GradientMode);
			ColorBlend colorBlend = new ColorBlend(2);
			colorBlend.Colors[0] = Colour1;
			colorBlend.Colors[1] = Colour2;
			colorBlend.Positions[0] = 0f;
			colorBlend.Positions[1] = 1f;
			linearGradientBrush.InterpolationColors = colorBlend;
			g.FillRectangle(linearGradientBrush, rect);
		}

		public void FillHortVertCentre(ref Graphics g, Color Colour1, Color Colour2, LinearGradientMode GradientMode)
		{
			Rectangle rect = new Rectangle(0, 0, Width, Height);
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, Colour1, Colour2, GradientMode);
			ColorBlend colorBlend = new ColorBlend(3);
			colorBlend.Colors[0] = Colour2;
			colorBlend.Colors[1] = Colour1;
			colorBlend.Colors[2] = Colour2;
			colorBlend.Positions[0] = 0f;
			colorBlend.Positions[1] = ((GradientMode == LinearGradientMode.Horizontal) ? 0.55f : 0.5f);
			colorBlend.Positions[2] = 1f;
			linearGradientBrush.InterpolationColors = colorBlend;
			g.FillRectangle(linearGradientBrush, rect);
		}

		public void FillHortVertTopDown(ref Graphics g, Color Colour1, Color Colour2, LinearGradientMode GradientMode)
		{
			Rectangle rect = new Rectangle(0, 0, Width, Height);
			LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, Colour1, Colour2, GradientMode);
			ColorBlend colorBlend = new ColorBlend(2);
			colorBlend.Colors[0] = Colour1;
			colorBlend.Colors[1] = Colour2;
			colorBlend.Positions[0] = 0f;
			colorBlend.Positions[1] = 1f;
			linearGradientBrush.InterpolationColors = colorBlend;
			g.FillRectangle(linearGradientBrush, rect);
		}

		public void FillEllipse(ref Graphics g, Color Colour1, Color Colour2, bool AtBottom)
		{
			Rectangle rect = new Rectangle(0, 0, Width, Height);
			rect.Width = (int)((double)(float)Width * 2.1);
			rect.Height = Height * 2;
			rect.X = -(int)((float)(rect.Width - Width) / 1.9f);
			rect.Y = (AtBottom ? (-(int)((float)rect.Height / 13f)) : (-(int)((float)rect.Height / 2.5f)));
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddEllipse(rect);
			PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
			pathGradientBrush.SurroundColors = new Color[1]
			{
				Colour2
			};
			pathGradientBrush.CenterColor = Colour1;
			g.FillRectangle(pathGradientBrush, new Rectangle(0, 0, Width, Height));
		}

		public void FillRect(ref Graphics g, Color Colour1, Color Colour2)
		{
			Rectangle rect = new Rectangle(0, 0, Width, Height);
			GraphicsPath graphicsPath = new GraphicsPath();
			rect.Width = (int)((float)Width * 1.5f);
			rect.Height = (int)((float)Height * 1.5f);
			rect.X = -(int)((float)(rect.Width - Width) / 2f);
			rect.Y = -(int)((float)(rect.Height - Height) / 2f);
			graphicsPath.AddRectangle(rect);
			PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath);
			pathGradientBrush.SurroundColors = new Color[1]
			{
				Colour2
			};
			pathGradientBrush.CenterColor = Colour1;
			g.FillRectangle(pathGradientBrush, new Rectangle(0, 0, Width, Height));
		}

		public Color Lighter(Color InColor)
		{
			int num = InColor.R + 50;
			int num2 = InColor.G + 50;
			int num3 = InColor.B + 50;
			if (num > 255)
			{
				num = 255;
			}
			if (num2 > 255)
			{
				num2 = 255;
			}
			if (num3 > 255)
			{
				num3 = 255;
			}
			return Color.FromArgb(num, num2, num3);
		}

		public Color Darker(Color InColor)
		{
			int num = InColor.R - 50;
			int num2 = InColor.G - 50;
			int num3 = InColor.B - 50;
			if (num < 0)
			{
				num = 0;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (num3 < 0)
			{
				num3 = 0;
			}
			return Color.FromArgb(num, num2, num3);
		}
	}
}
