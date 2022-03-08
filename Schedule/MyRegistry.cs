using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule
{
    /// <summary>
    /// https://stackoverflow.com/questions/42978573/how-to-use-fluentscheduler-library-to-schedule-tasks-in-c
    /// </summary>
    public class MyRegistry : Registry
    {
        public MyRegistry()
        {
            Action someMethod = new Action(() =>
            {
                Console.WriteLine("Timed Task - Will run now");
            });

            // Schedule schedule = new Schedule(someMethod);
            // schedule.ToRunNow();

            this.Schedule(someMethod).ToRunNow();
        }
    }
}
