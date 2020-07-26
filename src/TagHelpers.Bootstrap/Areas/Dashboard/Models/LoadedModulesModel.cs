using Microsoft.AspNetCore.Mvc.DataTables;

namespace SatelliteSite.Substrate.Dashboards
{
    public class LoadedModulesModel
    {
        [DtDisplay(0, "name", Sortable = true)]
        public string? AssemblyName { get; set; }

        [DtDisplay(1, "version", Sortable = true, DefaultAscending = "asc")]
        public string? Version { get; set; }

        [DtDisplay(2, "branch", "{Branch} ({Commit})")]
        [DtCoalesce("")]
        [DtTooltip("{CommitLong}")]
        public string? Branch { get; set; }

        [DtIgnore]
        public string? Commit => CommitLong?.Substring(0, 7);

        [DtIgnore]
        public string? CommitLong { get; set; }

        [DtDisplay(3, "pkey")]
        public string? PublicKey { get; set; }
    }
}
