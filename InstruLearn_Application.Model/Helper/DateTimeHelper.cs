using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;

namespace InstruLearn_Application.Model.Helper
{
    public class DateTimeHelper
    {
        public static string GetDayName(int dayOfWeek)
        {
            return dayOfWeek switch
            {
                0 => "Sunday",
                1 => "Monday",
                2 => "Tuesday",
                3 => "Wednesday",
                4 => "Thursday",
                5 => "Friday",
                6 => "Saturday",
                _ => "Unknown"
            };
        }

        public static DateTime CalculateTimeEnd(DateTime startDate, int numberOfSessions, List<string> learningDays)
        {
            DateTime currentDate = startDate;
            int sessionsCompleted = 0;

            HashSet<int> validDays = new HashSet<int>();

            foreach (string day in learningDays)
            {
                if (System.Enum.TryParse(day, true, out DayOfWeeks parsedDay))
                {
                    validDays.Add((int)parsedDay);
                }
            }

            if (validDays.Count == 0)
            {
                throw new ArgumentException("Invalid learning days provided.");
            }

            while (sessionsCompleted < numberOfSessions)
            {
                if (validDays.Contains((int)currentDate.DayOfWeek))
                {
                    sessionsCompleted++;
                }

                if (sessionsCompleted < numberOfSessions)
                {
                    currentDate = currentDate.AddDays(1);
                }
            }

            return currentDate;
        }
    }
}
