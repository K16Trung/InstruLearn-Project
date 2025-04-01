using System;
using System.Collections.Generic;
using AutoMapper;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;

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

    public static DateTime CalculateTimeEnd(DateOnly startDay, TimeOnly timeStart, int numberOfSessions, List<string> learningDays)
    {
        DateTime currentDate = startDay.ToDateTime(timeStart);
        int sessionsCompleted = 0;

        // Convert string learning days to .NET `DayOfWeek` enum
        HashSet<DayOfWeek> validDays = new HashSet<DayOfWeek>();

        foreach (string day in learningDays)
        {
            if (Enum.TryParse<DayOfWeek>(day, true, out DayOfWeek parsedDay))
            {
                validDays.Add(parsedDay);
            }
        }

        if (validDays.Count == 0)
        {
            throw new ArgumentException("Invalid learning days provided.");
        }

        // Loop through days until all sessions are scheduled
        while (sessionsCompleted < numberOfSessions)
        {
            if (validDays.Contains(currentDate.DayOfWeek))
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


    public static bool IsValidTimeSlot(TimeOnly timeStart)
    {
        // Define allowed 2-hour intervals
        List<TimeOnly> validTimeSlots = new()
        {
            new TimeOnly(7, 0), new TimeOnly(9, 0),
            new TimeOnly(11, 0), new TimeOnly(13, 0),
            new TimeOnly(15, 0), new TimeOnly(17, 0),
            new TimeOnly(19, 0)
        };

        return validTimeSlots.Contains(timeStart);
    }

    public static DateOnly CalculateClassEndDate(DateOnly startDate, int totalDays, List<int> classDays)
    {
        int classDaysCount = 0;
        DateOnly currentDate = startDate;

        while (classDaysCount < totalDays)
        {
            if (classDays.Contains((int)currentDate.DayOfWeek))
                classDaysCount++;

            if (classDaysCount == totalDays)
                break;

            currentDate = currentDate.AddDays(1);
        }

        return currentDate;
    }


    public static DateTime CalculateClassEndTime(TimeOnly classTime, int durationInHours = 2)
    {
        // Get the current date and combine it with the TimeOnly object
        DateTime classStartDateTime = DateTime.Today.AddHours(classTime.Hour).AddMinutes(classTime.Minute);

        // Add the duration to calculate the class end time
        DateTime classEndDateTime = classStartDateTime.AddHours(durationInHours);

        return classEndDateTime;
    }

    public static List<Schedules> GenerateOneOnOneSchedules(Learning_Registration learningRegis)
    {
        var schedules = new List<Schedules>();

        DateOnly startDate = learningRegis.StartDay ?? DateOnly.FromDateTime(DateTime.Today);
        TimeOnly startTime = learningRegis.TimeStart;
        int learningTime = learningRegis.TimeLearning; // In minutes
        int sessions = learningRegis.NumberOfSession;

        List<DayOfWeek> learningDays = learningRegis.LearningRegistrationDay
            .Select(ld => (DayOfWeek)ld.DayOfWeek)
            .OrderBy(d => d)
            .ToList();

        for (int i = 0; i < sessions; i++)
        {
            TimeOnly endTime = startTime.AddMinutes(learningTime);

            schedules.Add(new Schedules
            {
                LearningRegisId =learningRegis.LearningRegisId,
                TeacherId = learningRegis.TeacherId,
                LearnerId = learningRegis.LearnerId,
                StartDay = startDate,
                TimeStart = startTime,
                TimeEnd = endTime,
                Mode = ScheduleMode.OneOnOne,
                //Status = "Scheduled"
            });

            startDate = GetNextLearningDay(startDate, learningDays);
        }

        return schedules;
    }

    private static DateOnly GetNextLearningDay(DateOnly current, List<DayOfWeek> availableDays)
    {
        do
        {
            current = current.AddDays(1);
        } while (!availableDays.Contains(current.DayOfWeek));

        return current;
    }

    public class DateOnlyTypeConverter : ITypeConverter<string, DateOnly>
    {
        public DateOnly Convert(string source, DateOnly destination, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source))
                return default;

            if (DateOnly.TryParse(source, out var result))
                return result;

            return default;
        }
    }

    public class TimeOnlyTypeConverter : ITypeConverter<string, TimeOnly>
    {
        public TimeOnly Convert(string source, TimeOnly destination, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(source))
                return default;

            if (TimeOnly.TryParse(source, out var result))
                return result;

            return default;
        }
    }

    // Helper method to calculate the end date
    public static DateOnly CalculateEndDate(DateOnly startDate, int totalDays, ICollection<DayOfWeeks> classDays)
    {
        DateOnly currentDate = startDate;
        int daysScheduled = 0;

        // Keep advancing the date until we've scheduled all required days
        while (daysScheduled < totalDays)
        {
            if (classDays.Contains((DayOfWeeks)currentDate.DayOfWeek))
            {
                daysScheduled++;

                // If we've reached the total days, return this date as the end date
                if (daysScheduled == totalDays)
                {
                    return currentDate;
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        return currentDate; // This should never be reached if the loop works correctly
    }
}
