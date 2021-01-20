﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace SatelliteSite.IdentityModule
{
    internal class IdentityAdvancedConfigurator :
        IConfigureOptions<MvcOptions>,
        IConfigureOptions<IdentityOptions>,
        IControllerModelConvention
    {
        private readonly IdentityAdvancedOptions _options;

        public IdentityAdvancedConfigurator(IOptions<IdentityAdvancedOptions> options)
        {
            _options = options.Value;
        }

        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.Assembly != typeof(IdentityAdvancedOptions).Assembly)
            {
                return;
            }

            for (int i = 0; i < controller.Actions.Count; i++)
            {
                var attr = controller.Actions[i].Attributes.OfType<IdentityAdvancedFeatureAttribute>().SingleOrDefault();
                if (attr?.GetActivated(_options) == false)
                {
                    controller.Actions.RemoveAt(i--);
                }
            }
        }

        public void Configure(MvcOptions options)
        {
            options.Conventions.Add(this);
        }

        public void Configure(IdentityOptions options)
        {
            if (_options.ShortenedClaimName)
            {
                options.ClaimsIdentity.RoleClaimType = UserClaimsPrincipalExtensions.ClaimTypes_Role;
                options.ClaimsIdentity.UserIdClaimType = UserClaimsPrincipalExtensions.ClaimTypes_NameIdentifier;
                options.ClaimsIdentity.UserNameClaimType = UserClaimsPrincipalExtensions.ClaimTypes_Name;
            }
        }
    }
}