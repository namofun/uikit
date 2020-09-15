#nullable disable
using System.IO;
using System.Linq;

namespace System
{
    /// <summary>
    /// Identify the version of this assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class GitVersionAttribute : Attribute
    {
        /// <summary>
        /// Gets the version of this assembly.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the branch of this assembly.
        /// </summary>
        public string Branch { get; }

        /// <summary>
        /// Sets the version of this assembly.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="branch">The branch.</param>
        public GitVersionAttribute(string version, string branch)
        {
            Version = version;
            Branch = branch;
        }

        /// <summary>
        /// The code for MSBuild scripts.
        /// </summary>
        /// <param name="Folder">The input parameter for <c>/some/root/solution/.git</c>.</param>
        /// <param name="Branch">The branch id.</param>
        /// <param name="CommitId">The commit id.</param>
        private static void MSBuildFunction(string Folder, out string Branch, out string CommitId)
        {
            Branch = "unknown";
            CommitId = "UNKNOWN";
            if (Directory.Exists(Folder))
            {
                var head = Path.Combine(Folder, "logs", "HEAD");
                if (File.Exists(head))
                {
                    var lines = File.ReadAllText(head).Trim();
                    var line = lines.Split('\n').LastOrDefault()?.Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        CommitId = line.Split(' ')[1];
                    }
                }

                head = Path.Combine(Folder, "HEAD");
                var packed_ref = Path.Combine(Folder, "packed-refs");
                if (File.Exists(head))
                {
                    var lines = File.ReadAllText(head).Trim();
                    var line = lines.Split('\n').FirstOrDefault()?.Trim() ?? "";
                    const string starts = "ref: refs/heads/";
                    if (line.StartsWith(starts))
                    {
                        Branch = line.Substring(starts.Length);
                    }
                    else if (line.Length == 40 && File.Exists(packed_ref))
                    {
                        lines = File.ReadAllText(packed_ref).Trim();
                        var line2 = lines.Split('\n').Select(t => t.Trim()).Where(t => t.StartsWith(line)).FirstOrDefault();
                        if (line2.IndexOf("refs/remotes") == 41)
                        {
                            var line3 = line2.Substring(41).Split(new[] { '/' }, 4);
                            if (line3.Length == 4) Branch = line3[3];
                        }
                        else if (line2.IndexOf("refs/tags") == 41)
                        {
                            var line3 = line2.Substring(41).Split(new[] { '/' }, 3);
                            if (line3.Length == 3) Branch = line3[2];
                        }
                    }
                }
            }
        }
    }
}
