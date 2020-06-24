namespace Microsoft.AspNetCore.Mvc.Routing
{
    /// <summary>
    /// The HttpContext Feature to tell status code page re-executor skip the process.
    /// </summary>
    internal interface IClaimedNoStatusCodePageFeature
    {
    }

    /// <summary>
    /// The HttpContext Feature to tell status code page re-executor skip the process.
    /// </summary>
    internal class ClaimedNoStatusCodePageFeature : IClaimedNoStatusCodePageFeature
    {
    }
}
