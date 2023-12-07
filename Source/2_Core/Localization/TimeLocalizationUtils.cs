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
                BLLanguage.Polish => PL_SecondsAgo(n, compact),
                BLLanguage.Chinese => CN_SecondsAgo(n, compact),
                BLLanguage.Korean => KR_SecondsAgo(n, compact),
                BLLanguage.French => FR_SecondsAgo(n, compact),
                BLLanguage.German => GER_SecondsAgo(n, compact),
                BLLanguage.Spanish => ES_SecondsAgo(n, compact),
                BLLanguage.Italian => IT_SecondsAgo(n, compact),
                _ => EN_SecondsAgo(n, compact)
            };
        }

        private static string MinutesAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_MinutesAgo(n, compact),
                BLLanguage.Japanese => JP_MinutesAgo(n, compact),
                BLLanguage.Russian => RU_MinutesAgo(n, compact),
                BLLanguage.Polish => PL_MinutesAgo(n, compact),
                BLLanguage.Chinese => CN_MinutesAgo(n, compact),
                BLLanguage.Korean => KR_MinutesAgo(n, compact),
                BLLanguage.French => FR_MinutesAgo(n, compact),
                BLLanguage.German => GER_MinutesAgo(n, compact),
                BLLanguage.Spanish => ES_MinutesAgo(n, compact),
                BLLanguage.Italian => IT_MinutesAgo(n, compact),
                _ => EN_MinutesAgo(n, compact)
            };
        }

        private static string HoursAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_HoursAgo(n, compact),
                BLLanguage.Japanese => JP_HoursAgo(n, compact),
                BLLanguage.Russian => RU_HoursAgo(n, compact),
                BLLanguage.Polish => PL_HoursAgo(n, compact),
                BLLanguage.Chinese => CN_HoursAgo(n, compact),
                BLLanguage.Korean => KR_HoursAgo(n, compact),
                BLLanguage.French => FR_HoursAgo(n, compact),
                BLLanguage.German => GER_HoursAgo(n, compact),
                BLLanguage.Spanish => ES_HoursAgo(n, compact),
                BLLanguage.Italian => IT_HoursAgo(n, compact),
                _ => EN_HoursAgo(n, compact)
            };
        }

        private static string DaysAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_DaysAgo(n, compact),
                BLLanguage.Japanese => JP_DaysAgo(n, compact),
                BLLanguage.Russian => RU_DaysAgo(n, compact),
                BLLanguage.Polish => PL_DaysAgo(n, compact),
                BLLanguage.Chinese => CN_DaysAgo(n, compact),
                BLLanguage.Korean => KR_DaysAgo(n, compact),
                BLLanguage.French => FR_DaysAgo(n, compact),
                BLLanguage.German => GER_DaysAgo(n, compact),
                BLLanguage.Spanish => ES_DaysAgo(n, compact),
                BLLanguage.Italian => IT_DaysAgo(n, compact),
                _ => EN_DaysAgo(n, compact)
            };
        }

        private static string MonthsAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_MonthsAgo(n, compact),
                BLLanguage.Japanese => JP_MonthsAgo(n, compact),
                BLLanguage.Russian => RU_MonthsAgo(n, compact),
                BLLanguage.Polish => PL_MonthsAgo(n, compact),
                BLLanguage.Chinese => CN_MonthsAgo(n, compact),
                BLLanguage.Korean => KR_MonthsAgo(n, compact),
                BLLanguage.French => FR_MonthsAgo(n, compact),
                BLLanguage.German => GER_MonthsAgo(n, compact),
                BLLanguage.Spanish => ES_MonthsAgo(n, compact),
                BLLanguage.Italian => IT_MonthsAgo(n, compact),
                _ => EN_MonthsAgo(n, compact)
            };
        }

        private static string YearsAgo(int n, bool compact) {
            return BLLocalization.GetCurrentLanguage() switch {
                BLLanguage.English => EN_YearsAgo(n, compact),
                BLLanguage.Japanese => JP_YearsAgo(n, compact),
                BLLanguage.Russian => RU_YearsAgo(n, compact),
                BLLanguage.Polish => PL_YearsAgo(n, compact),
                BLLanguage.Chinese => CN_YearsAgo(n, compact),
                BLLanguage.Korean => KR_YearsAgo(n, compact),
                BLLanguage.French => FR_YearsAgo(n, compact),
                BLLanguage.German => GER_YearsAgo(n, compact),
                BLLanguage.Spanish => ES_YearsAgo(n, compact),
                BLLanguage.Italian => IT_YearsAgo(n, compact),
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
            return $"{n}秒前";
        }

        private static string JP_MinutesAgo(int n, bool compact) {
            return $"{n}分前";
        }

        private static string JP_HoursAgo(int n, bool compact) {
            return $"{n}時間前";
        }

        private static string JP_DaysAgo(int n, bool compact) {
            return $"{n}日前";
        }

        private static string JP_MonthsAgo(int n, bool compact) {
            return $"{n}ヶ月前";
        }

        private static string JP_YearsAgo(int n, bool compact) {
            return $"{n}年前";
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

        #region Polish

        private static string PL_SecondsAgo(int n, bool compact) {
            if (n is 1) return compact ? $"{n} sekunda" : $"{n} sekunda temu";

            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} sekundy" : $"{n} sekundy temu";
            }

            return compact ? $"{n} sekund" : $"{n} sekund temu";
        }

        private static string PL_MinutesAgo(int n, bool compact) {
            if (n is 1) return compact ? $"{n} minuta" : $"{n} minuta temu";

            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} minuty" : $"{n} minuty temu";
            }

            return compact ? $"{n} minut" : $"{n} minut temu";
        }

        private static string PL_HoursAgo(int n, bool compact) {
            if (n is 1) return compact ? $"{n} godzina" : $"{n} godzina temu";

            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} godziny" : $"{n} godziny temu";
            }

            return compact ? $"{n} godzin" : $"{n} godzin temu";
        }

        private static string PL_DaysAgo(int n, bool compact) {
            if (n is 1) return compact ? $"{n} dzień" : $"{n} dzień temu";

            return compact ? $"{n} dni" : $"{n} dni temu";
        }

        private static string PL_MonthsAgo(int n, bool compact) {
            if (n is 1) return compact ? $"{n} miesiąc" : $"{n} miesiąc temu";

            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} miesiące" : $"{n} miesiące temu";
            }

            return compact ? $"{n} miesięcy" : $"{n} miesięcy temu";
        }

        private static string PL_YearsAgo(int n, bool compact) {
            if (n is 1) return compact ? $"{n} rok" : $"{n} rok temu";

            if (n is < 10 or > 20) {
                var lastDigit = n % 10;
                if (lastDigit is 2 or 3 or 4) return compact ? $"{n} lata" : $"{n} lata temu";
            }

            return compact ? $"{n} lat" : $"{n} lat temu";
        }

        #endregion

        #region Chinese

        private static string CN_SecondsAgo(int n, bool compact) {
            return $"{n} 秒前";
        }

        private static string CN_MinutesAgo(int n, bool compact) {
            return $"{n} 分钟前";
        }

        private static string CN_HoursAgo(int n, bool compact) {
            return $"{n} 小时前";
        }

        private static string CN_DaysAgo(int n, bool compact) {
            return $"{n} 天前";
        }

        private static string CN_MonthsAgo(int n, bool compact) {
            return $"{n} 个月前";
        }

        private static string CN_YearsAgo(int n, bool compact) {
            return $"{n} 年前";
        }

        #endregion

        #region Korean

        private static string KR_SecondsAgo(int n, bool compact) {
            return $"{n} 초 전";
        }

        private static string KR_MinutesAgo(int n, bool compact) {
            return $"{n} 분 전";
        }

        private static string KR_HoursAgo(int n, bool compact) {
            return $"{n} 시간 전";
        }

        private static string KR_DaysAgo(int n, bool compact) {
            return $"{n} 일 전";
        }

        private static string KR_MonthsAgo(int n, bool compact) {
            return $"{n} 개월 전";
        }

        private static string KR_YearsAgo(int n, bool compact) {
            return $"{n} 년 전";
        }

        #endregion

        #region French

        private static string FR_SecondsAgo(int n, bool compact) {
            return n switch {
                1 => "il y a 1 seconde",
                _ => $"il y a {n} secondes"
            };
        }

        private static string FR_MinutesAgo(int n, bool compact) {
            return n switch {
                1 => "il y a 1 minute",
                _ => $"il y a {n} minutes"
            };
        }

        private static string FR_HoursAgo(int n, bool compact) {
            return n switch {
                1 => "il y a 1 heure",
                _ => $"il y a {n} heures"
            };
        }

        private static string FR_DaysAgo(int n, bool compact) {
            return n switch {
                1 => "il y a 1 jour",
                _ => $"il y a {n} jours"
            };
        }

        private static string FR_MonthsAgo(int n, bool compact) {
            return $"il y a {n} mois";
        }

        private static string FR_YearsAgo(int n, bool compact) {
            return n switch {
                1 => "il y a 1 an",
                _ => $"il y a {n} ans"
            };
        }

        #endregion

        #region German

        private static string GER_SecondsAgo(int n, bool compact) {
            return n switch {
                1 => "vor 1 Sekunde",
                _ => $"vor {n} Sekunden"
            };
        }

        private static string GER_MinutesAgo(int n, bool compact) {
            return n switch {
                1 => "vor 1 Minute",
                _ => $"vor {n} Minuten"
            };
        }

        private static string GER_HoursAgo(int n, bool compact) {
            return n switch {
                1 => "vor 1 Stunde",
                _ => $"vor {n} Stunden"
            };
        }

        private static string GER_DaysAgo(int n, bool compact) {
            return n switch {
                1 => "vor 1 Tag",
                _ => $"vor {n} Tagen"
            };
        }

        private static string GER_MonthsAgo(int n, bool compact) {
            return n switch {
                1 => "vor 1 Monat",
                _ => $"vor {n} Monaten"
            };
        }

        private static string GER_YearsAgo(int n, bool compact) {
            return n switch {
                1 => "vor 1 Jahr",
                _ => $"vor {n} Jahren"
            };
        }

        #endregion

        #region Spanish

        private static string ES_SecondsAgo(int n, bool compact) {
            return n switch {
                1 => "hace 1 segundo",
                _ => $"hace {n} segundos"
            };
        }

        private static string ES_MinutesAgo(int n, bool compact) {
            return n switch {
                1 => "hace 1 minuto",
                _ => $"hace {n} minutos"
            };
        }

        private static string ES_HoursAgo(int n, bool compact) {
            return n switch {
                1 => "hace 1 hora",
                _ => $"hace {n} horas"
            };
        }

        private static string ES_DaysAgo(int n, bool compact) {
            return n switch {
                1 => "hace 1 día",
                _ => $"hace {n} días"
            };
        }

        private static string ES_MonthsAgo(int n, bool compact) {
            return n switch {
                1 => "hace 1 mes",
                _ => $"hace {n} meses"
            };
        }

        private static string ES_YearsAgo(int n, bool compact) {
            return n switch {
                1 => "hace 1 año",
                _ => $"hace {n} años"
            };
        }

        #endregion

        #region Italian

        private static string IT_SecondsAgo(int n, bool compact) {
            return n switch {
                1 => "1 secondo fa",
                _ => $"{n} secondi fa"
            };
        }

        private static string IT_MinutesAgo(int n, bool compact) {
            return n switch {
                1 => "1 minuto fa",
                _ => $"{n} minuti fa"
            };
        }

        private static string IT_HoursAgo(int n, bool compact) {
            return n switch {
                1 => "1 ora fa",
                _ => $"{n} ore fa"
            };
        }

        private static string IT_DaysAgo(int n, bool compact) {
            return n switch {
                1 => "1 giorno fa",
                _ => $"{n} giorni fa"
            };
        }

        private static string IT_MonthsAgo(int n, bool compact) {
            return n switch {
                1 => "1 mese fa",
                _ => $"{n} mesi fa"
            };
        }

        private static string IT_YearsAgo(int n, bool compact) {
            return n switch {
                1 => "1 anno fa",
                _ => $"{n} anni fa"
            };
        }

        #endregion
    }
}