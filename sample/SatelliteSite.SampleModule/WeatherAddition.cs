using MediatR;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.IdentityModule.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.SampleModule
{
    public class WeatherAddition : IAdditionalRole
    {
        public string Category { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public bool EnableUrl { get; set; } = true;

        public string GetUrl(object urlHelper)
        {
            if (!EnableUrl) return null;
            var url = urlHelper as IUrlHelper;
            return url.Action("GetOne", "Weather", new { area = "Api", id = Guid.NewGuid().ToString() });
        }
    }

    public class WeatherAdditionProvider : INotificationHandler<UserDetailModel>
    {
        public Task Handle(UserDetailModel notification, CancellationToken cancellationToken)
        {
            notification.AddMore(new WeatherAddition
            {
                Category = "Weather of",
                Text = "aaa",
                Title = "AAA",
                EnableUrl = false,
            });

            notification.AddMore(new WeatherAddition
            {
                Category = "Weather of",
                Text = "bbb",
                Title = "BBB"
            });

            notification.AddMore(new WeatherAddition
            {
                Category = "Weathers",
                Text = "aaa",
                Title = "AAA"
            });

            notification.AddMore(new WeatherAddition
            {
                Category = "Weathers",
                Text = "bbb",
                Title = "BBB",
                EnableUrl = false
            });

            return Task.CompletedTask;
        }
    }
}
