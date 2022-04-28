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
            TextObject text = null;
            if (rank == 3)
                text = new TextObject("{=!}Peace be upon you, outlander. May you walk with the safety of Asera while you dwell in the company of his sons - and while you walk the lands where his Code is law. I am an Imam of the Aserai, and I bid you hospitality expecting it fully in kind. Is there something I may do for our mutual good?");

            return text;
        }

        public override TextObject GetClergyGreetingInducted(int rank)
        {
            TextObject text = null;
            if (rank == 3)
                text = new TextObject("{=!}Mashaera, blood of my blood. It is good to see you are alive and that you are in good health; for the world is rife with conflict beyond our brotherhood and little is to be held as certain. Are you here on pilgrimage or duty, in good tidings or ill news? How may this humble Imam be of service to his sibling?");

            return text;
        }

        public override TextObject GetClergyInduction(int rank)
        {
            throw new NotImplementedException();
        }

        public override TextObject GetClergyPreachingAnswer(int rank)
        {
            TextObject text = null;
            if (rank == 3)
                text = new TextObject("{=!}I preach the glory of Asera, our progenitor of legend. I preach lessons learned from the treasury of wisdom he has granted us as his children. I preach that the Aserai shall reign in happiness and in sorrow, so long as we remain as brothers within the Banu confederacies.");

            return text;
        }
        public override TextObject GetClergyPreachingAnswerLast(int rank)
        {
            TextObject text = null;
            if (rank == 3)
                text = new TextObject("{=!}I preach that our future shall be as luminous as the sun and as enduring as the Nahasa, so long as we remain united, so long as we are charitable, so long as we welcome even the Jawwal to embrace the Code as their own guiding path in life!.");

            return text;
        }

        public override TextObject GetClergyProveFaith(int rank)
        {
            TextObject text = null;
            if (rank == 3)
                text = new TextObject("{=!}A common question among the devout and misguided alike; for how can one truly be assured they walk the righteous path? We know not until the end comes and we are weighed against the deeds of our progenitor; but I shall guide you with what wisdom I can.");

            return text;
        }

        public override TextObject GetClergyProveFaithLast(int rank)
        {
            TextObject text = null;
            if (rank == 3)
                text = new TextObject("{=!}Foremost in my viewing, as a man of the cities of our lands; is the act of almsgiving. We are as keepers to our brothers, to our sisters and siblings; and only a cruel soul would let the blood of their blood starve.You are only as pure as the lowest among us, and thus you purify yourself of sin when you seek to uplift the whole of the Aserai.");

            return text;
        }

        public override string GetId() => "asera";

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
