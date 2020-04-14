using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace Amsaad.Services
{
	/// <summary>
	/// Summary description for CaptchaHandler
	/// </summary>
	public class CaptchaHandler : IHttpHandler, IRequiresSessionState
	{

		public void ProcessRequest(HttpContext context)
		{
			Dictionary<string, string> captcha = GetCaptcha();
			string img64 = captcha.Values.ElementAt(0);
			context.Session["Captcha"] = captcha.Keys.ElementAt(0);
			context.Response.BinaryWrite(Convert.FromBase64String(img64));

	
		}

		private Dictionary<string, string> GetCaptcha()
		{
			Lib.Captcha.RecaptchaService srv = new Lib.Captcha.RecaptchaService(200, 70, Lib.Captcha.RecaptchaService.RecaptchaStrType.MixAll, Lib.Captcha.RecaptchaService.RecaptchaComplexity.HIGH);
			return srv.GenerateCaptchaBase64(5); 
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}