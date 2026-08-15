using System;

namespace Api.ActivityLogModule
{
    public class ActivityLog : IEquatable<ActivityLog>
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string Activity { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }
        public string? IpAddress { get; set; }

        public ActivityLog() { }

        public ActivityLog(int logId, int userId, string activity, DateTime activityDate, string? ipAddress)
        {
            LogId = logId;
            UserId = userId;
            Activity = activity;
            ActivityDate = activityDate;
            IpAddress = ipAddress;
        }

        public bool Equals(ActivityLog? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (LogId > 0 || other.LogId > 0) return LogId == other.LogId;
            return string.Equals(ToString(), other.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj) => Equals(obj as ActivityLog);

        public override int GetHashCode()
        {
            if (LogId > 0) return LogId.GetHashCode();
            return StringComparer.OrdinalIgnoreCase.GetHashCode(ToString());
        }

        public override string ToString() =>
            $"ActivityLog[LogId={LogId}, UserId={UserId}, Activity={Activity}, ActivityDate={ActivityDate:O}]";
    }
}
