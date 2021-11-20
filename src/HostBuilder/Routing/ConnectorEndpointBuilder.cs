using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Microsoft.AspNetCore.Routing
{
    internal sealed class ConnectorEndpointBuilder : BaseEndpointBuilder
    {
        public const string NotSupportMessage =
            "For mapping APIs and controllers, " +
            "please do RegisterEndpoints() in module class.";

        public Type ModuleType { get; }

        public ConnectorEndpointBuilder(
            IEndpointRouteBuilder builder,
            string areaName,
            AbstractConnector connector)
            : base(builder, areaName, connector.Module.Conventions)
        {
            ModuleType = connector.Module.GetType();
        }

        public override IEndpointConventionBuilder MapApiDocument(string name, string title, string description, string version)
        {
            throw new NotSupportedException(NotSupportMessage);
        }

        public override ControllerActionEndpointConventionBuilder MapControllers()
        {
            throw new NotSupportedException(NotSupportMessage);
        }

        protected override ModuleEndpointDataSource GetOrCreateDataSource()
        {
            var edsType = typeof(ModuleEndpointDataSource<>).MakeGenericType(ModuleType);
            var dataSource = default(ModuleEndpointDataSource);

            foreach (var eds in DataSources)
            {
                if (eds.GetType() == edsType)
                {
                    dataSource = (ModuleEndpointDataSource)eds;
                }
            }

            if (dataSource == null)
            {
                dataSource = (ModuleEndpointDataSource)ServiceProvider.GetService(edsType)!;
                DataSources.Add(dataSource);
            }

            return dataSource;
        }
    }
}
