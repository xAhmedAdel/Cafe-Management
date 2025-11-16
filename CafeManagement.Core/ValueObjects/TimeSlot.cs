namespace CafeManagement.Core.ValueObjects;

public readonly record struct TimeSlot(DateTime StartTime, DateTime EndTime)
{
    public TimeSpan Duration => EndTime - StartTime;
    public int DurationMinutes => (int)Math.Round(Duration.TotalMinutes);

    public bool Overlaps(TimeSlot other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    public bool Contains(DateTime time)
    {
        return time >= StartTime && time < EndTime;
    }

    public static TimeSlot FromMinutes(DateTime startTime, int minutes)
    {
        return new TimeSlot(startTime, startTime.AddMinutes(minutes));
    }
}