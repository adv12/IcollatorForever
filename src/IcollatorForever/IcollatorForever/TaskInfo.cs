using System;

namespace IcollatorForever
{
    public class TaskInfo<T>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public T Result { get; set; }

        public bool IsCompleted { get; set; } = false;

        public bool IsCanceled { get; set; } = false;

        public bool IsFaulted { get; set; } = false;

        public string ErrorString { get; set; } = "";
    }
}
