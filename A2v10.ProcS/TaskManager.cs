// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
    public class SyncTaskManager : ITaskManager
    {
        public void AddTask(Task task)
        {
            task.RunSynchronously();
        }
    }
}
