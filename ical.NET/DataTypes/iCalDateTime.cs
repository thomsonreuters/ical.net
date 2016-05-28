using System;
using System.IO;
using Ical.Net.Interfaces;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// The iCalendar equivalent of the .NET <see cref="DateTime"/> class.
    /// <remarks>
    /// In addition to the features of the <see cref="DateTime"/> class, the <see cref="CalDateTime"/>
    /// class handles time zone differences, and integrates seamlessly into the iCalendar framework.
    /// </remarks>
    /// </summary>
    [Serializable]
    public sealed class CalDateTime : EncodableDataType, IDateTime
    {
        public static CalDateTime Now => new CalDateTime(DateTime.Now);

        public static CalDateTime Today => new CalDateTime(DateTime.Today);

        private DateTime _value;
        private bool _hasDate;
        private bool _hasTime;
        private bool _isUniversalTime;

        public CalDateTime() {}

        public CalDateTime(IDateTime value)
        {
            Initialize(value.Value, value.TzId, null);
        }

        public CalDateTime(DateTime value) : this(value, null) {}

        public CalDateTime(DateTime value, string tzId)
        {
            Initialize(value, tzId, null);
        }

        public CalDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            Initialize(year, month, day, hour, minute, second, null, null);
            HasTime = true;
        }

        public CalDateTime(int year, int month, int day, int hour, int minute, int second, string tzId)
        {
            Initialize(year, month, day, hour, minute, second, tzId, null);
            HasTime = true;
        }

        public CalDateTime(int year, int month, int day, int hour, int minute, int second, string tzId, ICalendar cal)
        {
            Initialize(year, month, day, hour, minute, second, tzId, cal);
            HasTime = true;
        }

        public CalDateTime(int year, int month, int day) : this(year, month, day, 0, 0, 0) {}
        public CalDateTime(int year, int month, int day, string tzId) : this(year, month, day, 0, 0, 0, tzId) {}

        public CalDateTime(string value)
        {
            var serializer = new DateTimeSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        private void Initialize(int year, int month, int day, int hour, int minute, int second, string tzId, ICalendar cal)
        {
            Initialize(CoerceDateTime(year, month, day, hour, minute, second, DateTimeKind.Local), tzId, cal);
        }

        private void Initialize(DateTime value, string tzId, ICalendar cal)
        {
            if (value.Kind == DateTimeKind.Utc)
            {
                IsUniversalTime = true;
            }

            // Convert all incoming values to UTC.
            Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            HasDate = true;
            HasTime = (value.Second == 0 && value.Minute == 0 && value.Hour == 0) ? false : true;
            TzId = tzId;
            AssociatedObject = cal;
        }

        private DateTime CoerceDateTime(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
        {
            var dt = DateTime.MinValue;

            // NOTE: determine if a date/time value exceeds the representable date/time values in .NET.
            // If so, let's automatically adjust the date/time to compensate.
            // FIXME: should we have a parsing setting that will throw an exception
            // instead of automatically adjusting the date/time value to the
            // closest representable date/time?
            try
            {
                if (year > 9999)
                {
                    dt = DateTime.MaxValue;
                }
                else if (year > 0)
                {
                    dt = new DateTime(year, month, day, hour, minute, second, kind);
                }
            }
            catch {}

            return dt;
        }

        public override ICalendarObject AssociatedObject
        {
            get { return base.AssociatedObject; }
            set
            {
                if (!Equals(AssociatedObject, value))
                {
                    base.AssociatedObject = value;
                }
            }
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var dt = obj as IDateTime;
            if (dt != null)
            {
                _value = dt.Value;
                _isUniversalTime = dt.IsUniversalTime;
                _hasDate = dt.HasDate;
                _hasTime = dt.HasTime;

                AssociateWith(dt);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is IDateTime)
            {
                AssociateWith((IDateTime) obj);
                return ((IDateTime) obj).AsUtc.Equals(AsUtc);
            }
            if (obj is DateTime)
            {
                var dt = (CalDateTime) obj;
                AssociateWith(dt);
                return Equals(dt.AsUtc, AsUtc);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return ToString(null, null);
        }

        public static bool operator <(CalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
            {
                return left.AsUtc < right.AsUtc;
            }
            return left.AsUtc.Date < right.AsUtc.Date;
        }

        public static bool operator >(CalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
            {
                return left.AsUtc > right.AsUtc;
            }
            return left.AsUtc.Date > right.AsUtc.Date;
        }

        public static bool operator <=(CalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
            {
                return left.AsUtc <= right.AsUtc;
            }
            return left.AsUtc.Date <= right.AsUtc.Date;
        }

        public static bool operator >=(CalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
            {
                return left.AsUtc >= right.AsUtc;
            }
            return left.AsUtc.Date >= right.AsUtc.Date;
        }

        public static bool operator ==(CalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
            {
                return left.AsUtc.Equals(right.AsUtc);
            }
            return left.AsUtc.Date.Equals(right.AsUtc.Date);
        }

        public static bool operator !=(CalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);

            if (left.HasTime || right.HasTime)
            {
                return !left.AsUtc.Equals(right.AsUtc);
            }
            return !left.AsUtc.Date.Equals(right.AsUtc.Date);
        }

        public static TimeSpan operator -(CalDateTime left, IDateTime right)
        {
            left.AssociateWith(right);
            return left.AsUtc - right.AsUtc;
        }

        public static IDateTime operator -(CalDateTime left, TimeSpan right)
        {
            var copy = left.Copy<IDateTime>();
            copy.Value -= right;
            return copy;
        }

        public static IDateTime operator +(CalDateTime left, TimeSpan right)
        {
            var copy = left.Copy<IDateTime>();
            copy.Value += right;
            return copy;
        }

        public static implicit operator CalDateTime(DateTime left)
        {
            return new CalDateTime(left);
        }

        /// <summary>
        /// Converts the date/time to this computer's local date/time.
        /// </summary>
        public DateTime AsSystemLocal
        {
            get
            {
                if (!HasTime)
                {
                    return DateTime.SpecifyKind(Value.Date, DateTimeKind.Local);
                }
                if (IsUniversalTime)
                {
                    return Value.ToLocalTime();
                }
                return AsUtc.ToLocalTime();
            }
        }

        private DateTime _utc;

        /// <summary>
        /// Converts the date/time to UTC (Coordinated Universal Time)
        /// </summary>
        public DateTime AsUtc
        {
            get
            {
                if (IsUniversalTime)
                {
                    _utc = DateTime.SpecifyKind(_value, DateTimeKind.Utc);
                    return _utc;
                }
                if (!string.IsNullOrWhiteSpace(TzId))
                {
                    var newUtc = DateUtil.ToZonedDateTimeLeniently(Value, TzId);
                    _utc = newUtc.ToDateTimeUtc();
                    return _utc;
                }
                _utc = DateTime.SpecifyKind(Value, DateTimeKind.Local).ToUniversalTime();

                // Fallback to the OS-conversion
                return _utc;
            }
        }

        public bool IsUniversalTime
        {
            get { return _isUniversalTime; }
            set { _isUniversalTime = value; }
        }

        public string TimeZoneName => TzId;

        public DateTime Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public bool HasDate
        {
            get { return _hasDate; }
            set { _hasDate = value; }
        }

        public bool HasTime
        {
            get { return _hasTime; }
            set { _hasTime = value; }
        }

        private string _tzId = string.Empty;
        public string TzId
        {
            get
            {
                if (IsUniversalTime)
                {
                    return "UTC";
                }
                return !string.IsNullOrWhiteSpace(_tzId)
                    ? _tzId
                    : Parameters.Get("TZID");
            }
            set
            {
                if (!Equals(TzId, value))
                {
                    Parameters.Set("TZID", value);
                    _tzId = value;
                }
            }
        }

        public int Year => Value.Year;

        public int Month => Value.Month;

        public int Day => Value.Day;

        public int Hour => Value.Hour;

        public int Minute => Value.Minute;

        public int Second => Value.Second;

        public int Millisecond => Value.Millisecond;

        public long Ticks => Value.Ticks;

        public DayOfWeek DayOfWeek => Value.DayOfWeek;

        public int DayOfYear => Value.DayOfYear;

        public DateTime Date => Value.Date;

        public TimeSpan TimeOfDay => Value.TimeOfDay;

        public IDateTime ToTimeZone(string newTimeZone)
        {
            if (string.IsNullOrWhiteSpace(newTimeZone))
            {
                throw new ArgumentException("You must provide a valid TZID to the ToTimeZone() method", "newTimeZone");
            }
            if (Calendar == null)
            {
                throw new Exception("The iCalDateTime object must have an iCalendar associated with it in order to use TimeZones.");
            }

            var newDt = string.IsNullOrWhiteSpace(TzId)
                ? DateUtil.ToZonedDateTimeLeniently(Value, newTimeZone).ToDateTimeUtc()
                : DateUtil.FromTimeZoneToTimeZone(Value, TzId, newTimeZone).ToDateTimeUtc();

            return new CalDateTime(newDt, newTimeZone);
        }

        public IDateTime SetTimeZone(ITimeZone tz)
        {
            if (tz != null)
            {
                TzId = tz.TzId;
            }
            return this;
        }

        public IDateTime Add(TimeSpan ts)
        {
            return this + ts;
        }

        public IDateTime Subtract(TimeSpan ts)
        {
            return this - ts;
        }

        public TimeSpan Subtract(IDateTime dt)
        {
            return this - dt;
        }

        public IDateTime AddYears(int years)
        {
            var dt = Copy<IDateTime>();
            dt.Value = Value.AddYears(years);
            return dt;
        }

        public IDateTime AddMonths(int months)
        {
            var dt = Copy<IDateTime>();
            dt.Value = Value.AddMonths(months);
            return dt;
        }

        public IDateTime AddDays(int days)
        {
            var dt = Copy<IDateTime>();
            dt.Value = Value.AddDays(days);
            return dt;
        }

        public IDateTime AddHours(int hours)
        {
            var dt = Copy<IDateTime>();
            if (!dt.HasTime && hours % 24 > 0)
            {
                dt.HasTime = true;
            }
            dt.Value = Value.AddHours(hours);
            return dt;
        }

        public IDateTime AddMinutes(int minutes)
        {
            var dt = Copy<IDateTime>();
            if (!dt.HasTime && minutes % 1440 > 0)
            {
                dt.HasTime = true;
            }
            dt.Value = Value.AddMinutes(minutes);
            return dt;
        }

        public IDateTime AddSeconds(int seconds)
        {
            var dt = Copy<IDateTime>();
            if (!dt.HasTime && seconds % 86400 > 0)
            {
                dt.HasTime = true;
            }
            dt.Value = Value.AddSeconds(seconds);
            return dt;
        }

        public IDateTime AddMilliseconds(int milliseconds)
        {
            var dt = Copy<IDateTime>();
            if (!dt.HasTime && milliseconds % 86400000 > 0)
            {
                dt.HasTime = true;
            }
            dt.Value = Value.AddMilliseconds(milliseconds);
            return dt;
        }

        public IDateTime AddTicks(long ticks)
        {
            var dt = Copy<IDateTime>();
            dt.HasTime = true;
            dt.Value = Value.AddTicks(ticks);
            return dt;
        }

        public bool LessThan(IDateTime dt)
        {
            return this < dt;
        }

        public bool GreaterThan(IDateTime dt)
        {
            return this > dt;
        }

        public bool LessThanOrEqual(IDateTime dt)
        {
            return this <= dt;
        }

        public bool GreaterThanOrEqual(IDateTime dt)
        {
            return this >= dt;
        }

        public void AssociateWith(IDateTime dt)
        {
            if (AssociatedObject == null && dt.AssociatedObject != null)
            {
                AssociatedObject = dt.AssociatedObject;
            }
            else if (AssociatedObject != null && dt.AssociatedObject == null)
            {
                dt.AssociatedObject = AssociatedObject;
            }
        }

        public int CompareTo(IDateTime dt)
        {
            if (Equals(dt))
            {
                return 0;
            }
            if (this < dt)
            {
                return -1;
            }
            if (this > dt)
            {
                return 1;
            }
            throw new Exception("An error occurred while comparing two IDateTime values.");
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var tz = TimeZoneName;
            if (!string.IsNullOrEmpty(tz))
            {
                tz = " " + tz;
            }

            if (format != null)
            {
                return Value.ToString(format, formatProvider) + tz;
            }
            if (HasTime && HasDate)
            {
                return Value + tz;
            }
            if (HasTime)
            {
                return Value.TimeOfDay + tz;
            }
            return Value.ToShortDateString() + tz;
        }
    }
}