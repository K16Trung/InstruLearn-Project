using System;
using System.Collections.Generic;
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

    public static List<Schedules> GenerateOnonOnSchedules(Learning_Registration learningRegis)
    {
        List<Schedules> schedules = new List<Schedules>();
        if (learningRegis.StartDay == null || learningRegis.LearningRegistrationDay == null || !learningRegis.LearningRegistrationDay.Any())
        {
            return schedules;
        }

        DateOnly currentDate = learningRegis.StartDay.Value;
        int sessionsCreated = 0;

        while (sessionsCreated < learningRegis.NumberOfSession)
        {
            foreach (var day in learningRegis.LearningRegistrationDay.OrderBy(d => d.DayOfWeek))
            {
                if (sessionsCreated >= learningRegis.NumberOfSession)
                    break;

                // Find the next available date for this day of the week
                while (currentDate.DayOfWeek != (DayOfWeek)day.DayOfWeek)
                {
                    currentDate = currentDate.AddDays(1);
                }

                schedules.Add(new Schedules
                {
                    LearnerId = learningRegis.LearnerId,
                    TeacherId = learningRegis.TeacherId,
                    LearningRegisId = learningRegis.LearningRegisId,
                    TimeStart = learningRegis.TimeStart,
                    TimeEnd = learningRegis.TimeStart.AddMinutes(learningRegis.TimeLearning),
                    Mode = ScheduleMode.OneOnOne, // or another default mode
                });

                sessionsCreated++;
                currentDate = currentDate.AddDays(1);
            }
        }

        return schedules;
    }


}
