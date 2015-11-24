using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greedy.Toolkit.BackgroundService
{
    public static class BackgroundServiceConfig
    {
        private static IList<IBackgroundTask> bgTasks = new List<IBackgroundTask>();
        private static Queue<Task> tasks = new Queue<Task>();

        public static void AddTask(IBackgroundTask task)
        {
            bgTasks.Add(task);
        }

        public static void Start()
        {
            foreach (var bgTask in bgTasks)
            {
                tasks.Enqueue(Task.Run(() => { bgTask.Start(); }));
            }
        }

        public static void Stop()
        {
            foreach (var bgTask in bgTasks)
            {
                bgTask.Stop();
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
