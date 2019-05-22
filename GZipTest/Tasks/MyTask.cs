using System;

namespace GZipTest.Tasks
{
    public class MyTask
    {
        public Action Action { get; }

        public MyTask(Action action)
        {
            Action = action;
        }
    }
}