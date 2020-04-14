using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;


namespace Lib.Captcha
{
	public class RecaptchaService
	{
		public enum RecaptchaStrType
		{
			NumbersOnly,
			LettersOnly,
			LowerLettersOnly,
			CapitalLettersOnly,
			NumbersAndLowerLetters,
			NumbersAndCapitalLetters,
			MixAll
		}
		public enum RecaptchaComplexity
		{
			No,
			LOW,
			MEDIUM,
			HIGH
		}

		#region Constructor and private properties
		//TODO: Add extra fonts 
		string[] supportedFonts = { "Verdana", "Times New Roman", "Arial" };
		private int H { get; set; }
		private int W { get; set; }

		private int rotateMin { get; set; }
		private int rotateMAX { get; set; }

		private RecaptchaStrType StrType { get; set; }
		private RecaptchaComplexity Complexity { get; set; }
		private HatchStyle[] hStyle { get; set; }
		private FontStyle[] fStyle { get; set; }

		public RecaptchaService() : this(250, 70, RecaptchaStrType.NumbersOnly, RecaptchaComplexity.No)
		{
			//Defaults

		}

		public RecaptchaService(int cWidth, int cHigh, RecaptchaStrType type, RecaptchaComplexity level)
		{
			W = cWidth;
			H = cHigh;
			StrType = type;
			Complexity = level;
			setCoplexityStyle();
		}
		#endregion

		//<---------------------------Public Methods--------------------------->
		public Dictionary<string, string> GenerateCaptchaBase64(int length)
		{
			Dictionary<string, string> captcha = new Dictionary<string, string>();

			string genStr = GetRandomString(length);

			byte[] imageBytes = CreateCaptchaByteArray(genStr);

			captcha.Add(genStr, Convert.ToBase64String(imageBytes));

			return captcha;
		}



		//<---------------------------Drawing Methods--------------------------->
		#region Drawing
		private byte[] CreateCaptchaByteArray(string str)
		{
			using (var capmap = new Bitmap(W, H, PixelFormat.Format24bppRgb))
			{
				using (var g = Graphics.FromImage(capmap))
				{
					g.TextRenderingHint = TextRenderingHint.AntiAlias;
					var drArea = new RectangleF(0, 0, W, H);
					//Get the background brush
					Brush bgBrush = WritingBrush(GetRondomColor(150, 200), Color.White);
					//fill background

					g.FillRectangle(bgBrush, drArea);

					Matrix m = new Matrix();
					Random r = new Random();
					for (int i = 0; i <= str.Length - 1; i++)
					{
						m.Reset();
						int x = W / (str.Length + 1) * i;
						int y = H / 2;
						//Rotate text Random
						m.RotateAt(r.Next(rotateMin, rotateMAX), new PointF(x, y));
						g.Transform = m;
						//write the letters
						float fs = GetFontSize(str.Length);
						FontStyle fStl = fStyle[r.Next(fStyle.Length - 1)];
						Font f = new Font(supportedFonts[r.Next(supportedFonts.Length - 1)], fs, fStl, GraphicsUnit.Pixel);
						int maxY = H - Convert.ToInt32(fs);
						if (maxY < 0) maxY = 0;
						float fy = r.Next(0, maxY);
						g.DrawString(str.Substring(i, 1), f, new SolidBrush(GetRondomColor(10, 120)), x, fy);

						g.ResetTransform();
					}
					if (Complexity > RecaptchaComplexity.LOW)
						DrawRandomLine(g);
					byte[] imageBytes = null;
					using (var stream = new MemoryStream())
					{
						capmap.Save(stream, ImageFormat.Bmp);
						imageBytes = stream.ToArray();
					}

					return imageBytes;
				}
			}
		}
		private void DrawRandomLine(Graphics g)
		{
			Random r = new Random();
			Pen linePen = new Pen(new SolidBrush(GetRondomColor(240, 255, 100)), r.Next(1, 4));
			for (int i = 0; i < r.Next(3, 5); i++)
			{
				Point startPoint = new Point(r.Next(0, W), r.Next(0, H));
				Point endPoint = new Point(r.Next(0, W), r.Next(0, H));
				linePen.Color = GetRondomColor(50, 250, 120);
				g.DrawLine(linePen, startPoint, endPoint);
				if (Complexity == RecaptchaComplexity.HIGH)
				{
					Point bezierPoint1 = new Point(r.Next(0, W), r.Next(0, H));
					Point bezierPoint2 = new Point(r.Next(0, W), r.Next(0, H));

					g.DrawBezier(linePen, startPoint, bezierPoint1, bezierPoint2, endPoint);
				}
				else
				{

				}
			}
		}
		#endregion

		//<---------------------------Configuration Methods--------------------------->
		#region Configurations
		private void setCoplexityStyle()
		{
			rotateMin = 0;
			rotateMAX = 0;
			switch (Complexity)
			{
				case RecaptchaComplexity.LOW:
					hStyle = new HatchStyle[] { HatchStyle.Min, HatchStyle.Vertical, HatchStyle.Horizontal, HatchStyle.BackwardDiagonal };
					rotateMin = rotateMin - 5;
					rotateMAX = rotateMAX + 5;
					fStyle = new FontStyle[] { FontStyle.Regular, FontStyle.Bold, FontStyle.Italic };
					break;
				case RecaptchaComplexity.MEDIUM:
					hStyle = new HatchStyle[] { HatchStyle.Percent20, HatchStyle.Max, HatchStyle.LightHorizontal, HatchStyle.LightVertical, HatchStyle.ForwardDiagonal, HatchStyle.DashedHorizontal, HatchStyle.DashedVertical };
					rotateMin = rotateMin - 10;
					rotateMAX = rotateMAX + 10;
					fStyle = new FontStyle[] { FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Underline };
					break;
				case RecaptchaComplexity.HIGH:
					hStyle = new HatchStyle[] { HatchStyle.DarkDownwardDiagonal, HatchStyle.Weave, HatchStyle.SolidDiamond, HatchStyle.Percent30, HatchStyle.Plaid, HatchStyle.ZigZag };
					rotateMin = rotateMin - 15;
					rotateMAX = rotateMAX + 15;
					fStyle = new FontStyle[] { FontStyle.Regular, FontStyle.Bold, FontStyle.Strikeout, FontStyle.Italic, FontStyle.Underline };

					break;
				default:
					hStyle = new HatchStyle[] { HatchStyle.Percent05 };
					fStyle = new FontStyle[] { FontStyle.Regular, FontStyle.Bold };
					break;
			}
		}

		#endregion

		//<---------------------------Helper Methods--------------------------->
		#region Helpers
		//TODO:Manage Colors to more wide colors and relate background color to forefront
		private Color GetRondomColor(int min, int max, int opcity = -1)
		{
			Random r = new Random();
			Color f = Color.FromArgb((r.Next(min, max)), (r.Next(min, max)), (r.Next(min, max)));
			if (opcity != -1)
			{
				f = Color.FromArgb(r.Next(50, opcity), (r.Next(min, max)), (r.Next(min, max)), (r.Next(min, max)));
			}
			return f;
		}

		private int GetFontSize(int length)
		{
			//int[] fSize = { 15, 20, 25, 30 };//em
			Random r = new Random();
			var averageSize = W / length;
			return r.Next(averageSize - (length * 2), averageSize + (length * 2));
		}
		private Brush WritingBrush(Color foreColor, Color backColor)
		{
			Random r = new Random();
			Brush b = new HatchBrush(hStyle[r.Next(hStyle.Length - 1)],
						foreColor,
						backColor);
			return b;
		}
		private string GetRandomString(int length)
		{
			string SourceStr = string.Empty;
			string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			string nums = "0123456789";
			switch (StrType)
			{
				case RecaptchaStrType.LettersOnly:
					SourceStr = letters + letters.ToLower();
					break;
				case RecaptchaStrType.LowerLettersOnly:
					SourceStr = letters.ToLower();
					break;
				case RecaptchaStrType.CapitalLettersOnly:
					SourceStr = letters.ToUpper();
					break;
				case RecaptchaStrType.NumbersAndLowerLetters:
					SourceStr = nums.Remove(0, 2) + letters.ToLower();
					break;
				case RecaptchaStrType.NumbersAndCapitalLetters:
					SourceStr = nums.Remove(0, 2) + letters.ToUpper();
					break;
				case RecaptchaStrType.MixAll:
					SourceStr = nums.Remove(0, 2) + letters.ToUpper() + letters.ToLower();
					break;
				default:
					SourceStr = nums;
					break;
			}
			var r = new Random();
			string randomStr = string.Empty;
			for (int i = 0; i < length; i++)
			{
				randomStr += SourceStr[r.Next(SourceStr.Length)];
			}
			return randomStr;
		}
		#endregion
	}
}
