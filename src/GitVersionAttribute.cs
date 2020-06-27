﻿namespace System
{
    using System.IO;
    using System.Linq;

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
        private void MSBuildFunction(string Folder, out string Branch, out string CommitId)
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
                if (File.Exists(head))
                {
                    var lines = File.ReadAllText(head).Trim();
                    var line = lines.Split('\n').FirstOrDefault()?.Trim() ?? "";
                    const string starts = "ref: refs/heads/";
                    if (line.StartsWith(starts))
                    {
                        Branch = line.Substring(starts.Length);
                    }
                }
            }
        }
    }
}
