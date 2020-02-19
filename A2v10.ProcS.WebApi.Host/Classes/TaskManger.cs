// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using A2v10.ProcS.Infrastructure;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace A2v10.ProcS.WebApi.Host.Classes
{
    public class TaskManager : ITaskManager
    {
        private readonly List<Task> tasks;

        public TaskManager()
        {
            tasks = new List<Task>();
        }

        public void AddTask(Task task)
        {
            tasks.Add(task);
            task.Start();
        }
    }
}
