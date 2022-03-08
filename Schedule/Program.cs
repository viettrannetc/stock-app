// See https://aka.ms/new-console-template for more information
using FluentScheduler;
using Schedule;

//https://fluentscheduler.github.io/
JobManager.Initialize(new MyRegistry());
Console.ReadLine();