using System;

namespace BeatLeader {
    public static class TimeLocalizationUtils {
        #region GetRelativeTimeString

        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        public static string GetRelativeTimeStringLocalizedWithFont(TimeSpan timeSpan, bool compact) {
            var font = BLLocalization.GetLanguageFont();
            if (font != null) {
                return $"<font={font.name}>{GetRelativeTimeStringLocalized(timeSpan, compact)}</font>";
            }

            return GetRelativeTimeStringLocalized(timeSpan, compact);
        }

        public static string GetRelativeTimeStringLocalized(TimeSpan timeSpan, bool compact) {
            switch (timeSpan.TotalSeconds) {
                case < 0: return "-";
                case < 1 * Minute: return SecondsAgo(timeSpan.Seconds, compact);
                case < 2 * Minute: return MinutesAgo(1, compact);
                case < 1 * Hour: return MinutesAgo(timeSpan.Minutes, compact);
                case < 2 * Hour: return HoursAgo(1, compact);
                case < 24 * Hour: return HoursAgo(timeSpan.Hours, compact);
                case < 2 * Day: return DaysAgo(1, compact);
                case < 30 * Day: return DaysAgo(timeSpan.Days, compact);
                case < 12 * Month: {
                    var months = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 30));
                    return months <= 1 ? MonthsAgo(1, compact) : MonthsAgo(months, compact);
                }
                default: {
                    var years = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 365));
                    return years <= 1 ? YearsAgo(1, compact) : YearsAgo(years, compact);
                }
            }
        }

        private static string SecondsAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_SecondsAgo(n, compact),
                BLLanguage.Japanese => JP_SecondsAgo(n, compact),
                BLLanguage.Russian => RU_SecondsAgo(n, compact),
                BLLanguage.Chinese => CN_SecondsAgo(n, compact),
                BLLanguage.Korean => KR_SecondsAgo(n, compact),
                _ => EN_SecondsAgo(n, compact)
            };
        }

        private static string MinutesAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_MinutesAgo(n, compact),
                BLLanguage.Japanese => JP_MinutesAgo(n, compact),
                BLLanguage.Russian => RU_MinutesAgo(n, compact),
                BLLanguage.Chinese => CN_MinutesAgo(n, compact),
                BLLanguage.Korean => KR_MinutesAgo(n, compact),
                _ => EN_MinutesAgo(n, compact)
            };
        }

        private static string HoursAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_HoursAgo(n, compact),
                BLLanguage.Japanese => JP_HoursAgo(n, compact),
                BLLanguage.Russian => RU_HoursAgo(n, compact),
                BLLanguage.Chinese => CN_HoursAgo(n, compact),
                BLLanguage.Korean => KR_HoursAgo(n, compact),
                _ => EN_HoursAgo(n, compact)
            };
        }

        private static string DaysAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_DaysAgo(n, compact),
                BLLanguage.Japanese => JP_DaysAgo(n, compact),
                BLLanguage.Russian => RU_DaysAgo(n, compact),
                BLLanguage.Chinese => CN_DaysAgo(n, compact),
                BLLanguage.Korean => KR_DaysAgo(n, compact),
                _ => EN_DaysAgo(n, compact)
            };
        }

        private static string MonthsAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_MonthsAgo(n, compact),
                BLLanguage.Japanese => JP_MonthsAgo(n, compact),
                BLLanguage.Russian => RU_MonthsAgo(n, compact),
                BLLanguage.Chinese => CN_MonthsAgo(n, compact),
                BLLanguage.Korean => KR_MonthsAgo(n, compact),
                _ => EN_MonthsAgo(n, compact)
            };
        }

        private static string YearsAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_YearsAgo(n, compact),
                BLLanguage.Japanese => JP_YearsAgo(n, compact),
                BLLanguage.Russian => RU_YearsAgo(n, compact),
                BLLanguage.Chinese => CN_YearsAgo(n, compact),
                BLLanguage.Korean => KR_YearsAgo(n, compact),
                _ => EN_YearsAgo(n, compact)
            };
        }

        #endregion

        #region English

        private static string EN_SecondsAgo(int n, bool compact) {
            return n == 1 ? "1 second ago" : $"{n} seconds ago";
        }

        private static string EN_MinutesAgo(int n, bool compact) {
            return n == 1 ? "1 minute ago" : $"{n} minutes ago";
        }

        private static string EN_HoursAgo(int n, bool compact) {
            return n == 1 ? "1 hour ago" : $"{n} hours ago";
        }

        private static string EN_DaysAgo(int n, bool compact) {
            return n == 1 ? "yesterday" : $"{n} days ago";
        }

        private static string EN_MonthsAgo(int n, bool compact) {
            return n == 1 ? "1 month ago" : $"{n} months ago";
        }

        private static string EN_YearsAgo(int n, bool compact) {
            return n == 1 ? "1 year ago" : $"{n} years ago";
        }

        #endregion

        #region Japanese

        private static string JP_SecondsAgo(int n, bool compact) {
            return compact ? $"{n}秒" : $"{n}秒前";
        }

        private static string JP_MinutesAgo(int n, bool compact) {
            return compact ? $"{n}分" : $"{n}分前";
        }

        private static string JP_HoursAgo(int n, bool compact) {
            return compact ? $"{n}時間" : $"{n}時間前";
        }

        private static string JP_DaysAgo(int n, bool compact) {
            return compact ? $"{n}日" : $"{n}日前";
        }

        private static string JP_MonthsAgo(int n, bool compact) {
            return compact ? $"{n}ヶ月" : $"{n}ヶ月前";
        }

        private static string JP_YearsAgo(int n, bool compact) {
            return compact ? $"{n}年" : $"{n}年前";
        }

        #endregion

        #region Russian

        private static string RU_SecondsAgo(int n, bool compact) {
            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 1) return compact ? $"{n} секунда" : $"{n} секунду назад";
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} секунды" : $"{n} секунды назад";
            }

            return compact ? $"{n} секунд" : $"{n} секунд назад";
        }

        private static string RU_MinutesAgo(int n, bool compact) {
            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 1) return compact ? $"{n} минута" : $"{n} минуту назад";
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} минуты" : $"{n} минуты назад";
            }

            return compact ? $"{n} минут" : $"{n} минут назад";
        }

        private static string RU_HoursAgo(int n, bool compact) {
            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 1) return compact ? $"{n} час" : $"{n} час назад";
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} часа" : $"{n} часа назад";
            }

            return compact ? $"{n} часов" : $"{n} часов назад";
        }

        private static string RU_DaysAgo(int n, bool compact) {
            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 1) return compact ? $"{n} день" : $"{n} день назад";
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} дня" : $"{n} дня назад";
            }

            return compact ? $"{n} дней" : $"{n} дней назад";
        }

        private static string RU_MonthsAgo(int n, bool compact) {
            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 1) return compact ? $"{n} месяц" : $"{n} месяц назад";
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} месяца" : $"{n} месяца назад";
            }

            return compact ? $"{n} месяцев" : $"{n} месяцев назад";
        }

        private static string RU_YearsAgo(int n, bool compact) {
            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 1) return compact ? $"{n} год" : $"{n} год назад";
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} года" : $"{n} года назад";
            }

            return compact ? $"{n} лет" : $"{n} лет назад";
        }

        #endregion

        #region Chinese

        private static string CN_SecondsAgo(int n, bool compact) {
            return compact ? $"{n} 秒" : $"{n} 秒前";
        }

        private static string CN_MinutesAgo(int n, bool compact) {
            return compact ? $"{n} 分钟" : $"{n} 分钟前";
        }

        private static string CN_HoursAgo(int n, bool compact) {
            return compact ? $"{n} 小时" : $"{n} 小时前";
        }

        private static string CN_DaysAgo(int n, bool compact) {
            return compact ? $"{n} 天" : $"{n} 天前";
        }

        private static string CN_MonthsAgo(int n, bool compact) {
            return compact ? $"{n} 个月" : $"{n} 个月前";
        }

        private static string CN_YearsAgo(int n, bool compact) {
            return compact ? $"{n} 年" : $"{n} 年前";
        }

        #endregion

        #region Korean

        private static string KR_SecondsAgo(int n, bool compact) {
            return compact ? $"{n} 초" : $"{n} 초 전";
        }

        private static string KR_MinutesAgo(int n, bool compact) {
            return compact ? $"{n} 분" : $"{n} 분 전";
        }

        private static string KR_HoursAgo(int n, bool compact) {
            return compact ? $"{n} 시간" : $"{n} 시간 전";
        }

        private static string KR_DaysAgo(int n, bool compact) {
            return compact ? $"{n} 일" : $"{n} 일 전";
        }

        private static string KR_MonthsAgo(int n, bool compact) {
            return compact ? $"{n} 개월" : $"{n} 개월 전";
        }

        private static string KR_YearsAgo(int n, bool compact) {
            return compact ? $"{n} 년" : $"{n} 년 전";
        }

        #endregion
    }
}