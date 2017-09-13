using System;
using System.Collections.Generic;
using System.Collections;

namespace ArkCrossEngine
{
    public class CoroutineObject
    {
        public object Coroutine;
        public object RetObject;
        public IEnumerator CoroutineFunction;
    }

    public interface CoroutineLoader
    {
        CoroutineObject StartCoroutineLoader(IEnumerator f, CoroutineObject obj);
        bool IsCoroutineFinished(CoroutineObject obj);
    }

    public class CoroutineManager
    {
        private static CoroutineManager s_CoroutineMgr = new CoroutineManager();
        public static CoroutineManager Instance { get { return s_CoroutineMgr; } }
        
        private int MaxJobCount = 1024;

        public CoroutineManager()
        {
            for (int i = 0; i < MaxJobCount; ++i)
            {
                JobIdQueue.Enqueue(i);
                QueuedJobs.Add(null);
            }
        }

        public void SetCoroutineLoader(CoroutineLoader loader)
        {
            CoroutineNativeLoader = loader;
        }

        public CoroutineObject StartSingleManual(IEnumerator f, CoroutineObject obj)
        {
            if (f != null && CoroutineNativeLoader != null)
            {
                return CoroutineNativeLoader.StartCoroutineLoader(f, obj);
            }
            else
            {
                throw new Exception("native coroutine loader have not been set yet.");
            }
        }

        public CoroutineObject[] StartBatchManual(IEnumerator[] f)
        {
            if (f != null && f.Length > 0 && CoroutineNativeLoader != null)
            {
                CoroutineObject[] r = new CoroutineObject[f.Length];
                for (int i = 0; i < r.Length; ++i)
                {
                    r[i] = new CoroutineObject();
                    CoroutineNativeLoader.StartCoroutineLoader(f[i], r[i]);
                }
                return r;
            }
            else
            {
                throw new Exception("native coroutine loader have not been set yet.");
            }
        }

        public int StartSingle(IEnumerator f, CoroutineObject obj = null)
        {
            if (f != null && CoroutineNativeLoader != null)
            {
                if (obj == null)
                {
                    obj = new CoroutineObject();
                }
                obj.CoroutineFunction = f;
                int jobid = GetNewJobId();
                CoroutineState state = new CoroutineState();
                state.Queue = new List<CoroutineObject>() { obj };
                state.State = JobState.NotStarted;
                QueuedJobs[jobid] = state;
                return jobid;
            }
            else
            {
                throw new Exception("native coroutine loader have not been set yet.");
            }
        }

        public int StartBatch(IEnumerator[] f)
        {
            if (f != null && f.Length > 0 && CoroutineNativeLoader != null)
            {
                int jobid = GetNewJobId();
                CoroutineState state = new CoroutineState();
                state.Queue = new List<CoroutineObject>();
                state.State = JobState.NotStarted;
                for (int i = 0; i < f.Length; ++i)
                {
                    CoroutineObject obj = new CoroutineObject();
                    obj.CoroutineFunction = f[i];
                    state.Queue.Add(obj);
                }
                QueuedJobs[jobid] = state;
                return jobid;
            }
            else
            {
                throw new Exception("native coroutine loader have not been set yet.");
            }
        }

        public bool IsAllJobsDone()
        {
            for (int i = 0; i < QueuedJobs.Count; ++i)
            {
                if (QueuedJobs[i] != null && QueuedJobs[i].State != JobState.Finished)
                {
                    return false;
                }
            }

            return true;
        }

        public void RemoveJob(int handle)
        {
            if (QueuedJobs[handle] != null && QueuedJobs[handle].State != JobState.Running)
            {
                QueuedJobs[handle] = null;
                RecycleJobId(handle);
            }
        }

        public void RemoveAllFinishedJobs()
        {
            for (int i = 0; i < QueuedJobs.Count; ++i)
            {
                if (QueuedJobs[i] != null && QueuedJobs[i].State == JobState.Finished)
                {
                    QueuedJobs[i] = null;
                    RecycleJobId(i);
                }
            }
        }

        public void Tick()
        {
            if (CoroutineNativeLoader == null)
            {
                return;
            }

            // check finished jobs
            for (int i = 0; i < QueuedJobs.Count; ++i)
            {
                if (QueuedJobs[i] != null && QueuedJobs[i].State == JobState.Running)
                {
                    if (QueuedJobs[i].Queue.TrueForAll(e => CoroutineNativeLoader.IsCoroutineFinished(e)))
                    {
                        QueuedJobs[i].State = JobState.Finished;
                    }
                }
            }

            // start new if needed
            for (int i = 0; i < QueuedJobs.Count; ++i)
            {
                if (QueuedJobs[i] != null && QueuedJobs[i].State == JobState.NotStarted)
                {
                    for (int j = 0; j < QueuedJobs[i].Queue.Count; ++j)
                    {
                        CoroutineObject obj = new CoroutineObject();
                        StartSingleManual(QueuedJobs[i].Queue[j].CoroutineFunction, obj);
                    }
                    QueuedJobs[i].State = JobState.Running;
                }
            }
        }

        private int GetNewJobId()
        {
            if (JobIdQueue.Count > 0)
            {
                return JobIdQueue.Dequeue();
            }
            else
            {
                throw new Exception("too many jobs queued.");
            }
        }

        private void RecycleJobId(int id)
        {
            JobIdQueue.Enqueue(id);
        }

        CoroutineLoader CoroutineNativeLoader;
        enum JobState
        {
            NotStarted,
            Running,
            Finished,
            Closed
        }
        class CoroutineState
        {
            public List<CoroutineObject> Queue;
            public JobState State;
        }
        List<CoroutineState> QueuedJobs = new List<CoroutineState>();
        Queue<int> JobIdQueue = new Queue<int>();
        int RunningCount = 0;
    }
}
