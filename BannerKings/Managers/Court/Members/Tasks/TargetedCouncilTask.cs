using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannerKings.Managers.Court.Members.Tasks
{
    public abstract class TargetedCouncilTask<T> : CouncilTask
    {
        protected TargetedCouncilTask(string id) : base(id)
        {
        }

        public T Target { get; protected set; }

        public abstract void ShowOptions();
        public abstract void MakeAiChoice();
        public abstract T GetDefaultTarget();
    }
}
