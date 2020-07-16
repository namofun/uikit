namespace SatelliteSite.Entities
{
    /// <summary>
    /// The event types for auditlogs.
    /// </summary>
    public enum AuditlogType
    {
        Judgehost = 0,
        TeamAffiliation = 1,
        Team = 2,
        User = 3,
        Contest = 4,
        Problem = 5,
        Testcase = 6,
        Language = 7,
        Scoreboard = 8,
        Clarification = 9,
        Submission = 10,
        Judging = 11,
        Executable = 12,
        Rejudging = 13,
        TeamCategory = 14,
        InternalError = 15,
        Configuration = 17,
        Attachment = 20,
    }
}
