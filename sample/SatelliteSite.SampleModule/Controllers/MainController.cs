﻿using Microsoft.AspNetCore.Authentication.EasyAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.SampleModule.Services;
using System.Security.Claims;

namespace SatelliteSite.SampleModule.Controllers
{
    [Area("Sample")]
    [Route("[area]/[controller]/[action]")]
    [SupportStatusCodePage]
    public class MainController : ViewControllerBase
    {
        private ForecastService Service { get; }

        public MainController(ForecastService service)
        {
            Service = service;
        }

        [HttpGet]
        public IActionResult Home()
        {
            return View(Service.Forecast());
        }

        [HttpGet]
        public IActionResult CardSample1()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Markdown()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        [HttpPost]
        public IActionResult Claims(string roleName)
        {
            return Json(User.IsInRole(roleName));
        }

        [Authorize]
        [HttpGet]
        public IActionResult ClaimsV2()
        {
            return Json(new EasyAuthClientPrincipal((ClaimsIdentity)User.Identity));
        }

        [HttpGet]
        public IActionResult ThrowErrors()
        {
            throw new System.Exception("hello?\ndhewufghe\ndheuwfhe");
        }
    }
}
