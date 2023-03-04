using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Court.Members.Tasks
{
    public class CouncilTask : BannerKingsObject
    {
        public CouncilTask(string id) : base(id)
        {
            Efficiency = 0f;
        }

        public CouncilTask GetCopy()
        {
            CouncilTask task = new CouncilTask(StringId);
            task.Initialize(task.Name, task.Description, task.Effects,
                task.StartingProgress);
            return task;
        }

        public void Initialize(TextObject name, TextObject description,
            TextObject effects, float startingProgress)
        {
            Initialize(name, description);
            Effects = effects;
            StartingProgress = startingProgress;
            if (StartingProgress > Efficiency)
            {
                Efficiency = StartingProgress;
            }
        }

        public void PostInitialize()
        {
            CouncilTask c = DefaultCouncilTasks.Instance.GetById(this);
            Initialize(c.Name, c.Description, c.Effects, c.StartingProgress);
        }

        public void ResetBuildUp() => Efficiency = StartingProgress;

        public TextObject Effects { get; private set; }
        public float StartingProgress { get; private set; }
        [SaveableProperty(100)] public float Efficiency { get; private set; }

        public void Tick()
        {
            if (Efficiency < 1f)
            {
                Efficiency += 0.015f;
            }

            if (Efficiency > 1f)
            {
                Efficiency = 1f;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is CouncilTask)
            {
                return StringId == (obj as CouncilTask).StringId && base.Equals(obj);
            }
            return base.Equals(obj);
        }
    }
}
