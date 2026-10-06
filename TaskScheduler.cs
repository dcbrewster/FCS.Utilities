using Microsoft.Win32.TaskScheduler;
using System;
using System.Text.RegularExpressions;
using Task = Microsoft.Win32.TaskScheduler.Task;

namespace FCS.Utilities;

/// <summary>
/// Utility class for managing tasks in the Windows Task Scheduler.
/// </summary>
public static class TaskScheduler
{
    /// <summary>
    /// Add Task to Task Scheduler
    /// </summary>
    /// <param name="taskPath">Name Of Task</param>
    /// <param name="taskDescription">Description of Task</param>
    /// <param name="execTime">Description of Task</param>
    /// <param name="exePath">Command To Execute</param>
    /// <param name="workingDirectory">Working Directory - default to null</param>
    /// Task Definition Settings - Defaults
    /// Setting                         Value                                                   Type
    /// =======                         =====                                                   ====
    /// AllowDemandStart	            true	                                                bool
    /// AllowHardTerminate	            true	                                                bool
    /// Compatibility                   V2                                                      Microsoft.Win32.TaskScheduler.TaskCompatibility
    /// DeleteExpiredTaskAfter          {00:00:00}                                              System.TimeSpan
    /// DisallowStartIfOnBatteries	    true	                                                bool
    /// DisallowStartOnRemoteAppSession	false	                                                bool
    /// Enabled	                        true	                                                bool
    /// ExecutionTimeLimit	            {3.00:00:00}                                            System.TimeSpan
    /// Hidden                          false	                                                bool
    /// IdleSettings	                {Microsoft.Win32.TaskScheduler.IdleSettings}            Microsoft.Win32.TaskScheduler.IdleSettings
    /// MaintenanceSettings             {Microsoft.Win32.TaskScheduler.MaintenanceSettings}     Microsoft.Win32.TaskScheduler.MaintenanceSettings
    /// MultipleInstances	            IgnoreNew	                                            Microsoft.Win32.TaskScheduler.TaskInstancesPolicy
    /// NetworkSettings	                {Microsoft.Win32.TaskScheduler.NetworkSettings}	        Microsoft.Win32.TaskScheduler.NetworkSettings
    /// Priority	                    BelowNormal	                                            System.Diagnostics.ProcessPriorityClass
    /// RestartCount	                0	                                                    int
    /// RestartInterval	                {00:00:00}	                                            System.TimeSpan
    /// RunOnlyIfIdle	                false	                                                bool
    /// RunOnlyIfLoggedOn	            true	                                                bool
    /// RunOnlyIfNetworkAvailable	    false	                                                bool
    /// StartWhenAvailable	            false	                                                bool
    /// StopIfGoingOnBatteries	        true	                                                bool
    /// UseUnifiedSchedulingEngine	    false	                                                bool
    /// Volatile	                    false	                                                bool
    /// WakeToRun	                    false	                                                bool
    public static bool AddTask(string taskPath, string taskDescription, DateTime execTime, string exePath, string? workingDirectory = null)
    {
        using TaskService ts = new();                                       // Get the service on the local machine
        Task existingTask = ts.GetTask(taskPath);                           // Check if the task already exists

        if (existingTask != null) return false;                             // Task already exists - bail

        TaskDefinition? td = ts.NewTask();                                  // Create a new task definition
        td.RegistrationInfo.Description = taskDescription;                  // Add task description
        td.Principal.RunLevel = TaskRunLevel.LUA;                           // Run with limited privileges

        DateTime triggerTime = execTime;                                    // Set the trigger time to the specified execution time
        td.Triggers.Add(new DailyTrigger { StartBoundary = triggerTime });  // Create a daily trigger that starts at the specified time

        // Create an action that will launch the program whenever the trigger fires
        td.Actions.Add(new ExecAction(exePath, null, workingDirectory));

        // Register the task in the root folder
        ts.RootFolder.RegisterTaskDefinition(
            taskPath,                                                       // Task Name/Path
            td,                                                             // Task Definition
            TaskCreation.CreateOrUpdate,                                    // Create or update the task
            null,                                                           // Use current user
            null,                                                           // No password needed
            TaskLogonType.InteractiveToken,                                 // Run only when user is logged on
            null);                                                          // No SDDL-defined security descriptor needed

        td = null;                                                          // Kill Object

        return true;                                                        // Return Result
    }

    /// <summary>
    /// Remove a task from the local machine's Task Scheduler
    /// </summary>
    /// <param name="taskName">Name of Task To Be Deleted/Removed</param>
    public static void RemoveTask(string taskName)
    {
        using TaskService ts = new();                                           // Get the service on the local machine
        Task existingTask = ts.GetTask(taskName);                               // Check if the task already exists
        TaskCollection tasks = ts.RootFolder.GetTasks(new Regex(taskName));     // Get the tasks that match the regex

        ts.RootFolder.DeleteTask(tasks[0].Name);                                // Remove First Occurrence Only

        // Remove All Tasks
        //foreach(Task task in tasks)                                           // Process each task
        //    ts.RootFolder.DeleteTask(task.Name);                              // Delete the tasks
    }
}