using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Asera
{
    public class AseraFaith : MonotheisticFaith
    {
        public override TextObject GetClergyForbiddenAnswer(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyGreeting(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyGreetingInducted(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyInduction(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyProveFaith(int rank)
        {
            throw new NotImplementedException();
        }

        public override int GetIdealRank(Settlement settlement)
        {
            if (settlement.IsTown)
                return 3;
            if (settlement.IsCastle)
                return 2;

            return 1;
        }

        public override List<Divinity> GetMainDivinities()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetMainDivinitiesDescription()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetMainGodDescription()
        {
            throw new NotImplementedException();
        }

        public override int GetMaxClergyRank() => 4;

        public override TextObject GetRankTitle(int rank)
        {
            TextObject text = null;
            if (rank == 4)
                text = new TextObject("{=!}Murshid");
            else if (rank == 3)
                text = new TextObject("{=!}Imam");
            else if (rank == 2)
                text = new TextObject("{=!}Akhund");
            else text = new TextObject("{=!}Faqir");

            return text;
        }

        public override List<Divinity> GetSecondaryDivinities()
        {
            throw new NotImplementedException();
        }

        public override TextObject GetSecondaryDivinitiesDescription()
        {
            throw new NotImplementedException();
        }
    }
}
