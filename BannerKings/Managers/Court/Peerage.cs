using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court
{
    public class Peerage
    {

        public Peerage(TextObject name, bool vote, bool election, bool knighthood, bool fiefs, bool council, bool boostsVotes)
        {
            Name = name;
            CanVote = vote;
            CanStartElection = election;
            CanGrantKnighthood = knighthood;
            CanHaveFief = fiefs;
            BoostsVotes = boostsVotes;
        }

        public static Peerage GetAdequatePeerage(Clan clan)
        {
            if (clan.IsUnderMercenaryService)
            {
                return new Peerage(new TextObject("{=!}No Peerage"), false, false, false, false, false, false);
            }


            if (clan.Kingdom != null)
            {
                var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
                if (titles.Any(x => x.type != Titles.TitleType.Lordship) || clan.Fiefs.Count > 0)
                {
                    return new Peerage(new TextObject("{=!}Full Peerage"), true, true, true, true, true, false);
                }
                else
                {
                    return new Peerage(new TextObject("{=!}Knight Peerage"), true, false, false, false, true, true);
                }
            }

            return new Peerage(new TextObject("{=!}No Peerage"), false, false, false, false, false, false);
        }

        public TextObject Name { get; private set; }
        public bool CanVote { get; private set; }
        public bool CanStartElection { get; private set; }
        public bool CanGrantKnighthood { get; private set; }
        public bool CanHaveFief { get; private set; }
        public bool CanHaveCouncil { get; private set; }
        public bool BoostsVotes { get; private set; }

        public TextObject GetRights() => new TextObject("{=!}Voting Allowed: {VOTING}\nElections Allowed: {ELECTIONS}\nKnighthood Granting: {KNIGHT}\nFiefs Allowed: {FIEFS}\nCouncil Allowed: {COUNCIL}")
            .SetTextVariable("VOTING", GetStr(CanVote))
            .SetTextVariable("ELECTIONS", GetStr(CanStartElection))
            .SetTextVariable("KNIGHT", GetStr(CanGrantKnighthood))
            .SetTextVariable("FIEFS", GetStr(CanHaveFief))
            .SetTextVariable("COUNCIL", GetStr(CanHaveCouncil));

        private TextObject GetStr(bool option) => option ? GameTexts.FindText("str_yes") : GameTexts.FindText("str_no");
    }
}
