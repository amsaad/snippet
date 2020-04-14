using Amsaad.Models;
using Amsaad.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Amsaad.Controllers
{
	public class HomeController : Controller
	{

		public ActionResult cView()
		{
			return View();
		}
    
    [HttpPost]
		public ActionResult _CAPTCHA()
		{
			//ViewBag.captchaSource = "~/Services/captchaHandler.ashx?id=" + @Guid.NewGuid().ToString();
			return PartialView();
		}
		
	}
}
