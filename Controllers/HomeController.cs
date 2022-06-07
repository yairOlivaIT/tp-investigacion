using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProyectoFinal2.Models;
using Facebook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ProyectoFinal2.Controllers
{
    
    public class HomeController : Controller
    {
        string AppId = string.Empty;
        string AppSecret = string.Empty;
        
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            var configuration = GetConfiguration();
            AppId = configuration.GetSection("AppID").Value;
            AppSecret = configuration.GetSection("AppSecret").Value;
        }

        public IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        private Uri RedirectUri
        {
            get
            {
                var uriBuilder = new UriBuilder(Request.Headers["Referer"].ToString());
                uriBuilder.Query = null;
                uriBuilder.Fragment = null;
                uriBuilder.Path = Url.Action("FacebookCallback");
                return uriBuilder.Uri;
            }
        }

        public IActionResult Facebook()
        {
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = AppId,
                client_secret = AppSecret,
                redirect_uri = RedirectUri.AbsoluteUri,
                response_type = "code",
                scope = "email"
            });
            return Redirect(loginUrl.AbsoluteUri);
        }
        
        public IActionResult FacebookCallback(string code)
        {
            var fb = new FacebookClient();
            dynamic result = fb.Post("oauth/access_token", new
            {
                client_id = AppId,
                client_secret = AppSecret,
                redirect_uri = RedirectUri.AbsoluteUri,
                code = code
            });
            var accessToken = result.access_token;
            fb.AccessToken = accessToken;
            dynamic data = fb.Get("me?fields=link,first_name,last_name,currency,email,locale,timezone,verified,picture,age_range");
            TempData["email"] = data.email;
            TempData["picture"] = data.picture.data.url;
            TempData["name"] = data.first_name + " " + data.last_name;
            return RedirectToAction("Index","Home");
        }
        
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
