using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public class Offering : Rite
    {
        private ItemObject input;
        private int inputCount;

        public Offering(ItemObject input, int inputCount)
        {
            this.input = input;
            this.inputCount = inputCount;
        }

        public override void Complete(Hero actionTaker)
        {

        }

        public override void Execute(Hero executor)
        {

        }

        public override TextObject GetDescription() => new TextObject();

        public override TextObject GetName() => new TextObject();

        public override float GetPietyReward() => input.Value;

        public override RiteType GetRiteType() => RiteType.OFFERING;

        public override float GetTimeInterval() => 1f;

        public override void SetDialogue()
        {
        
        }
    }
}
