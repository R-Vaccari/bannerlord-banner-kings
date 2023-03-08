using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Court.Members.Tasks
{
    public abstract class TargetedCouncilTask<T> : CouncilTask
    {
        protected TargetedCouncilTask(string id) : base(id)
        {
        }

        [SaveableProperty(200)] public T Target { get; protected set; }

        public abstract void ShowOptions();
        public abstract void MakeAiChoice();
        public abstract T GetDefaultTarget();
    }
}
