﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text;

namespace System.ServiceModel.Syndication
{
    internal static class DateTimeHelper
    {
        internal const string Rfc3339DateTimeFormat = "yyyy-MM-ddTHH:mm:ssK";

        public static Func<string, string, string, DateTimeOffset> CreateRss20DateTimeParser()
        {
            return (dateTimeString, localName, ns) =>
            {
                DateTimeOffset dto;

                // First check if DateTimeOffset default parsing can parse the date
                if (DateTimeOffset.TryParse(dateTimeString, out dto))
                {
                    return dto;
                }

                // RSS specifies RFC822
                if (Rfc822DateTimeParser(dateTimeString, out dto))
                {
                    return dto;
                }

                // Event though RCS3339 is for Atom, someone might be using this for RSS
                if (Rfc3339DateTimeParser(dateTimeString, out dto))
                {
                    return dto;
                }

                // Unable to parse - using a default date;
                return new DateTimeOffset();
            };
        }

        public static Func<string, string, string, DateTimeOffset> CreateAtom10DateTimeParser()
        {
            return (dateTimeString, localName, ns) =>
            {
                dateTimeString = dateTimeString.Trim();
                if (dateTimeString.Length < 20)
                {
                    throw new FormatException(SR.ErrorParsingDateTime);
                }

                if (dateTimeString[19] == '.')
                {
                    // remove any fractional seconds, we choose to ignore them
                    int i = 20;
                    while (dateTimeString.Length > i && char.IsDigit(dateTimeString[i]))
                    {
                        ++i;
                    }

                    dateTimeString = dateTimeString.Substring(0, 19) + dateTimeString.Substring(i);
                }

                DateTimeOffset dto;
                if (DateTimeOffset.TryParseExact(dateTimeString, Rfc3339DateTimeFormat,
                    CultureInfo.InvariantCulture.DateTimeFormat,
                    DateTimeStyles.None, out dto))
                {
                    return dto;
                }

                throw new FormatException(SR.ErrorParsingDateTime);
            };
        }

        private static bool Rfc3339DateTimeParser(string dateTimeString, out DateTimeOffset dto)
        {
            // RFC3339 uses the W3C Profile of ISO 8601 so using the date time format string "O" will achieve this.
            return DateTimeOffset.TryParseExact(dateTimeString, "O", null as IFormatProvider, DateTimeStyles.AllowWhiteSpaces, out dto);
        }

        private static bool Rfc822DateTimeParser(string dateTimeString, out DateTimeOffset dto)
        {
            StringBuilder dateTimeStringBuilder = new StringBuilder(dateTimeString.Trim());
            if (dateTimeStringBuilder.Length < 18)
            {
                return false;
            }

            int timeZoneStartIndex;
            for (timeZoneStartIndex = dateTimeStringBuilder.Length - 1; dateTimeStringBuilder[timeZoneStartIndex] != ' '; timeZoneStartIndex--)
                ;
            timeZoneStartIndex++;

            int timeZoneLength = dateTimeStringBuilder.Length - timeZoneStartIndex;
            string timeZoneSuffix = dateTimeStringBuilder.ToString(timeZoneStartIndex, timeZoneLength);
            dateTimeStringBuilder.Remove(timeZoneStartIndex, timeZoneLength);
            bool isUtc;
            dateTimeStringBuilder.Append(NormalizeTimeZone(timeZoneSuffix, out isUtc));
            string wellFormattedString = dateTimeStringBuilder.ToString();

            DateTimeOffset theTime;
            string[] parseFormat =
            {
                "ddd, dd MMMM yyyy HH:mm:ss zzz",
                "dd MMMM yyyy HH:mm:ss zzz",
                "ddd, dd MMM yyyy HH:mm:ss zzz",
                "dd MMM yyyy HH:mm:ss zzz",

                "ddd, dd MMMM yyyy HH:mm zzz",
                "dd MMMM yyyy HH:mm zzz",
                "ddd, dd MMM yyyy HH:mm zzz",
                "dd MMM yyyy HH:mm zzz",

                // The original RFC822 spec listed 2 digit years. RFC1123 updated the format to include 4 digit years and states that you should use 4 digits.
                // Technically RSS2.0 specifies RFC822 but it's presumed that RFC1123 will be used as we're now past Y2K and everyone knows better. The 4 digit
                // formats are listed first for performance reasons as it's presumed they will be more likely to match first.
                "ddd, dd MMMM yy HH:mm:ss zzz",
                "dd MMMM yyyy HH:mm:ss zzz",
                "ddd, dd MMM yy HH:mm:ss zzz",
                "dd MMM yyyy HH:mm:ss zzz",

                "ddd, dd MMMM yy HH:mm zzz",
                "dd MMMM yyyy HH:mm zzz",
                "ddd, dd MMM yy HH:mm zzz",
                "dd MMM yyyy HH:mm zzz"
            };

            if (DateTimeOffset.TryParseExact(wellFormattedString, parseFormat,
                CultureInfo.InvariantCulture.DateTimeFormat,
                (isUtc ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None), out theTime))
            {
                dto = theTime;
                return true;
            }

            return false;
        }

        private static string NormalizeTimeZone(string rfc822TimeZone, out bool isUtc)
        {
            isUtc = false;
            // return a string in "-08:00" format
            if (rfc822TimeZone[0] == '+' || rfc822TimeZone[0] == '-')
            {
                // the time zone is supposed to be 4 digits but some feeds omit the initial 0
                StringBuilder result = new StringBuilder(rfc822TimeZone);
                if (result.Length == 4)
                {
                    // the timezone is +/-HMM. Convert to +/-HHMM
                    result.Insert(1, '0');
                }
                result.Insert(3, ':');
                return result.ToString();
            }
            switch (rfc822TimeZone)
            {
                case "UT":
                case "Z":
                    isUtc = true;
                    return "-00:00";
                case "GMT":
                    return "-00:00";
                case "A":
                    return "-01:00";
                case "B":
                    return "-02:00";
                case "C":
                    return "-03:00";
                case "D":
                case "EDT":
                    return "-04:00";
                case "E":
                case "EST":
                case "CDT":
                    return "-05:00";
                case "F":
                case "CST":
                case "MDT":
                    return "-06:00";
                case "G":
                case "MST":
                case "PDT":
                    return "-07:00";
                case "H":
                case "PST":
                    return "-08:00";
                case "I":
                    return "-09:00";
                case "K":
                    return "-10:00";
                case "L":
                    return "-11:00";
                case "M":
                    return "-12:00";
                case "N":
                    return "+01:00";
                case "O":
                    return "+02:00";
                case "P":
                    return "+03:00";
                case "Q":
                    return "+04:00";
                case "R":
                    return "+05:00";
                case "S":
                    return "+06:00";
                case "T":
                    return "+07:00";
                case "U":
                    return "+08:00";
                case "V":
                    return "+09:00";
                case "W":
                    return "+10:00";
                case "X":
                    return "+11:00";
                case "Y":
                    return "+12:00";
                default:
                    return "";
            }
        }

    }
}
