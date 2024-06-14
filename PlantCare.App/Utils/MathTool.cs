using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantCare.App.Utils
{
    internal class MathTool
    {
        internal static TimeSpan CalculateAverageInterval(List<DateTime> timestamps)
        {
            if (timestamps == null || timestamps.Count < 2)
            {
                throw new ArgumentException("At least two timestamps are required to calculate an interval.");
            }

            // Sort the timestamps in ascending order
            timestamps.Sort();

            // List to store the intervals in TimeSpan
            List<TimeSpan> intervals = [];

            for (int i = 1; i < timestamps.Count; i++)
            {
                intervals.Add(timestamps[i] - timestamps[i - 1]);
            }

            // Calculate the total interval
            TimeSpan totalInterval = TimeSpan.FromTicks(intervals.Sum(interval => interval.Ticks));

            // Calculate the average interval
            TimeSpan averageInterval = new(totalInterval.Ticks / intervals.Count);

            return averageInterval;
        }
    }
}