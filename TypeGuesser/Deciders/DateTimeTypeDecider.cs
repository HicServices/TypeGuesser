using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TypeGuesser.Deciders;

/// <summary>
/// Guesses whether strings are <see cref="DateTime"/> and handles parsing approved strings according to the <see cref="Culture"/>
/// </summary>
public class DateTimeTypeDecider : DecideTypesForStrings<DateTime>
{
    private readonly TimeSpanTypeDecider _timeSpanTypeDecider;
    private readonly DecimalTypeDecider _decimalChecker;

    /// <summary>
    /// Array of all supported DateTime formats in which the Month appears before the Day e.g. e.g. "MMM-dd-yy" ("Sep-16-19")
    /// </summary>
    public static string[] DateFormatsMD;

    /// <summary>
    /// Array of all supported DateTime formats in which the Day appears before the Month e.g. "dd-MMM-yy" ("16-Sep-19")
    /// </summary>
    public static string[] DateFormatsDM;

    /// <summary>
    /// Array of all supported Time formats e.g. "h:mm:ss tt" ("9:34:39 AM")
    /// </summary>
    public static string[] TimeFormats;

    private string[] _dateFormatToUse;
    private CultureInfo _culture;
        
    /// <summary>
    /// Setting this to false will prevent <see cref="GuessDateFormat(IEnumerable{string})"/> changing the <see cref="Culture"/> e.g. when
    /// inserting date times
    /// </summary>
    public static bool AllowCultureGuessing = true;
        
    /// <summary>
    /// Explicitly sets the culture to use for processing date times.  This suppresses <see cref="GuessDateFormat(IEnumerable{string})"/>.
    /// Set to null to restore the current environment culture (and re enable guessing).
    /// 
    /// </summary>
    public override CultureInfo Culture { get => _culture;
        set
        {
            value ??= CultureInfo.CurrentCulture;

            _dateFormatToUse = value.DateTimeFormat.ShortDatePattern.IndexOf('M') > value.DateTimeFormat.ShortDatePattern.IndexOf('d') ? DateFormatsDM : DateFormatsMD;

            _culture = value; 
        } }

    static DateTimeTypeDecider()
    {
        var dateFormatsMd = new List<string>();
        var dateFormatsDm = new List<string>();
        var timeFormats = new List<string>();

        //all dates on their own
        foreach (var y in YearFormats)
        foreach (var m in MonthFormats)
        foreach (var d in DayFormats)
        foreach (var dateSeparator in DateSeparators)
        {
            dateFormatsMd.Add(string.Join(dateSeparator, m, d, y));
            dateFormatsMd.Add(string.Join(dateSeparator, y, m, d));

            dateFormatsDm.Add(string.Join(dateSeparator, d, m, y));
            dateFormatsMd.Add(string.Join(dateSeparator, y, m, d));
        }

        //then all the times
        foreach (var timeSeparator in TimeSeparators)
        foreach (var suffix in Suffixes)
        foreach (var h in HourFormats)
        foreach (var m in MinuteFormats)
        {
            timeFormats.Add(string.Join(timeSeparator, h, m));
            timeFormats.Add($"{string.Join(timeSeparator, h, m)} {suffix}");

            foreach (var s in SecondFormats)
            {
                timeFormats.Add(string.Join(timeSeparator, h, m, s));
                timeFormats.Add($"{string.Join(timeSeparator, h, m, s)} {suffix}");
            }
        }
        DateFormatsDM = dateFormatsDm.ToArray();
        DateFormatsMD = dateFormatsMd.ToArray();
        TimeFormats = timeFormats.ToArray();
    }

    private static readonly string[] YearFormats = {
        "yy",
        "yyy",
        "yyyy",
        "yyyyy"
    };

    private static readonly string[] MonthFormats = {
        "M",
        "MM",
        "MMM",
        "MMMM"
    };

    private static readonly string[] DayFormats = {
        "dd",
        "ddd",
        "dddd"
    };

    private static readonly string[] DateSeparators = {
        "\\\\",
        "/",
        "-",
        "."
    };

    private static readonly string[] HourFormats = {
        "h",
        "hh",
        "H",
        "HH"
    };

    private static readonly string[] MinuteFormats = {
        "m",
        "mm"
    };

    private static readonly string[] SecondFormats = {
        "s",
        "ss"
    };

    private static readonly string[] Suffixes = {
        "tt"
    };

    private static readonly string[] TimeSeparators = {
        ":"
    };

    /// <summary>
    /// Creates a new instance for detecting/parsing <see cref="DateTime"/> strings according to the <paramref name="cultureInfo"/>
    /// </summary>
    /// <param name="cultureInfo"></param>
    public DateTimeTypeDecider(CultureInfo cultureInfo) : base(cultureInfo,TypeCompatibilityGroup.Exclusive, typeof(DateTime))
    {
        _timeSpanTypeDecider = new TimeSpanTypeDecider(cultureInfo);
        _decimalChecker = new DecimalTypeDecider(cultureInfo);
    }

    /// <inheritdoc/>
    protected override IDecideTypesForStrings CloneImpl(CultureInfo overrideCulture)
    {
        return new DateTimeTypeDecider(overrideCulture);
    }

    /// <inheritdoc/>
    protected override object ParseImpl(string value)
    {
        // if user has specified a specific format that we are to use, use it
        if(Settings.ExplicitDateFormats != null)
            return DateTime.ParseExact(value,Settings.ExplicitDateFormats,_culture,DateTimeStyles.None);

        // otherwise parse a value using any of the valid culture formats
        if (!TryBruteParse(value, out var dt))
            throw new FormatException(string.Format(SR.DateTimeTypeDecider_ParseImpl_Could_not_parse___0___to_a_valid_DateTime, value));

        return dt;
    }

    /// <summary>
    /// Makes guess about whether to use MD or DM based on the <paramref name="samples"/>.
    /// Where no samples, or no matches or the same number of matches DM is used.
    /// the samples.
    /// 
    /// <para>If <see cref="Culture"/> has been set then this method is ignored.  If the static property <see cref="AllowCultureGuessing"/>
    /// is set then it is also ignored.</para>
    /// </summary>
    public void GuessDateFormat(IEnumerable<string> samples)
    {
        if(!AllowCultureGuessing)
            return;
            
        samples = samples.Where(s=>!string.IsNullOrWhiteSpace(s)).ToList();

        //if they are all valid anyway
        if(samples.All(s=>DateTime.TryParse(s,Culture,DateTimeStyles.None,out _)))
            return;
                        
        _dateFormatToUse = DateFormatsDM;
        var countDm = samples.Count(s=>TryBruteParse(s,out _));
        _dateFormatToUse = DateFormatsMD;
        var countMd = samples.Count(s=>TryBruteParse(s,out _));

        if(countDm >= countMd)
            _dateFormatToUse = DateFormatsDM;
    }

    /// <inheritdoc />
    public override bool IsAcceptableAsType(string candidateString, IDataTypeSize size)
    {
        return IsExplicitDate(candidateString) || base.IsAcceptableAsType(candidateString, size);
    }

    /// <inheritdoc/>
    protected override bool IsAcceptableAsTypeImpl(string candidateString, IDataTypeSize sizeRecord)
    {
        //if it's a float then it isn't a date is it! thanks C# for thinking 1.1 is the first of January
        if (_decimalChecker.IsAcceptableAsType(candidateString, sizeRecord))
            return false;

        //likewise if it is just the Time portion of the date then we have a column with mixed dates and times which SQL will not deal with well in the end database (e.g. it will set the
        //date portion of times to today's date which will be very confusing
        if (_timeSpanTypeDecider.IsAcceptableAsType(candidateString, sizeRecord))
            return false;

        try
        {
            return TryBruteParse(candidateString, out _);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private readonly char[] _space = { ' ' };

    private bool TryBruteParse(string s, out DateTime dt)
    {
        //if it's legit according to the current culture
        if(DateTime.TryParse(s,Culture,DateTimeStyles.None,out dt))
            return true;

        var split = s?.Split(_space, StringSplitOptions.RemoveEmptyEntries);

        //if there are no tokens
        if (split ==null || split.Length == 0)
        {
            dt = DateTime.MinValue;
            return false;
        }

        //if there is one token it is assumed either to be a date or a string
        if (split.Length == 1)
            if (TryGetTime(split[0], out dt))
                return true;
            else if (TryGetDate(split[0], out dt))
                return true;
            else
                return false;

        //if there are 2+ tokens then first token should be a date then the rest (concatenated) should be a time
        //e.g. "28/2/1993 5:36:27 AM" gets evaluated as "28/2/1993" and then "5:36:27 AM"

        if (TryGetDate(split[0], out dt) && TryGetTime(string.Join(" ", split.Skip(1)), out var time))
        {
            dt = new DateTime(dt.Year, dt.Month, dt.Day, time.Hour, time.Minute, time.Second, time.Millisecond);

            return true;
        }

        dt = DateTime.MinValue;
        return false;
    }

    private bool TryGetDate(string v, out DateTime date)
    {
        return DateTime.TryParseExact(v, _dateFormatToUse, Culture, DateTimeStyles.AllowInnerWhite, out date);
    }

    private bool TryGetTime(string v, out DateTime time)
    {
        return DateTime.TryParseExact(v, TimeFormats, Culture, DateTimeStyles.AllowInnerWhite, out time);
    }
}