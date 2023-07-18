using System;


namespace PrimitiveCodebaseElements.Primitive
{
    
    public class ProgressTracker
    {
        readonly Action<float> Tracker;
        readonly float From;
        readonly float Range;

        public ProgressTracker(Action<float> tracker, float from = default, float to = default)
        {
            if (to < from) throw new Exception($"Invalid range. from: {from}, to: {to}");
            Tracker = tracker;
            From = from;
            Range = (to == 0 ? 1 : to) - from;
        }

        public float Progress
        {
            set => Tracker(From + Range * value);
        }

        public ProgressStepper Steps(int steps)
        {
            return new ProgressStepper(this, steps);
        }

        public static ProgressTracker Dummy = new(x => { });
    }

    public class ProgressStepper
    {
        readonly ProgressTracker ProgressTracker;
        readonly int StepsOverall;
        int CurrentStepNo;

        public ProgressStepper(ProgressTracker tracker, int stepsOverall)
        {
            ProgressTracker = tracker;
            StepsOverall = stepsOverall;
        }

        public void Step()
        {
            lock (this)
            {
                CurrentStepNo++;
                if (StepsOverall < CurrentStepNo)
                    PrimitiveLogger.Logger.Instance()
                        .Error($"more than 100% progress: step {CurrentStepNo} of {StepsOverall}");
                ProgressTracker.Progress = CurrentStepNo / (float)StepsOverall;
            }
        }

        public void Done()
        {
            lock (this)
            {
                ProgressTracker.Progress = 1.0f;
            }
        }
    }
}