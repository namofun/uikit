using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Rewrite;

namespace Microsoft.AspNetCore.Routing
{
    public class PreparationRule : IRewriteRule
    {
        private static readonly PathString _lib = new PathString("/lib/");
        private static readonly PathString _images = new PathString("/images/");

        public void ApplyRule(RewriteContext context)
        {
            var feature = context.HttpContext.Features.Get<IUrlRewritingFeature>();
            if (feature != null)
            {
                context.Result = RuleResult.SkipRemainingRules;
                return;
            }
            else
            {
                feature = new UrlRewritingFeature(context.HttpContext.Request);
                context.HttpContext.Features.Set(feature);
            }

            if (feature.Path.StartsWithSegments(_lib) || feature.Path.StartsWithSegments(_images))
            {
                context.HttpContext.Response.StatusCode = 404;
                context.Result = RuleResult.EndResponse;
            }
        }

        public RuleResult ApplyUrl(ActionContext context, ref string path)
        {
            if (path.StartsWith(_lib.Value) || path.StartsWith(_images.Value))
                return RuleResult.SkipRemainingRules;
            return RuleResult.ContinueRules;
        }
    }
}
