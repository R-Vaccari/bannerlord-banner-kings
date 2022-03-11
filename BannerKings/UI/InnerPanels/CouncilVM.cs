using BannerKings.Managers.Court;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Library;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.UI.Items
{
    public class CouncilVM : SettlementGovernorSelectionVM
    {
        private Action<Hero> onDone;
        private Council council;
        public CouncilPosition Position { get; set; }

        public CouncilVM(Action<Hero> onDone, Council council, CouncilPosition position) : base(null, onDone)
        {
            this.onDone = onDone;
            this.council = council;
            this.Position = position;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            MBBindingList<SettlementGovernorSelectionItemVM> newList = new MBBindingList<SettlementGovernorSelectionItemVM>();
            newList.Add(this.AvailableGovernors[0]);
            this.AvailableGovernors = newList;

            MBReadOnlyList<Town> towns = Clan.PlayerClan.Fiefs;
            if (towns != null && towns.Count > 0)
            {
                foreach (Town town in towns)
                {
                    MBReadOnlyList<Hero> notables = town.Settlement.Notables;
                    if (notables != null && notables.Count > 0)
                        foreach (Hero notable in notables)
                            if (!council.IsMember(notable))
                                this.AvailableGovernors
                                    .Add(new CouncilMemberVM(notable, new Action<SettlementGovernorSelectionItemVM>(this.OnSelection),
                                    Position, council.GetCompetence(notable, Position)));
                }
            }

            if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(Hero.MainHero))
            {
                List<FeudalTitle> vassals = BannerKingsConfig.Instance.TitleManager.GetVassals(Hero.MainHero);
                if (vassals != null && vassals.Count > 0)
                    foreach (FeudalTitle vassal in vassals)
                        if (!council.IsMember(vassal.deJure))
                            this.AvailableGovernors
                                .Add(new CouncilMemberVM(vassal.deJure, new Action<SettlementGovernorSelectionItemVM>(this.OnSelection),
                                Position, council.GetCompetence(vassal.deJure, Position)));
            }

            MBReadOnlyList<Hero> members = Clan.PlayerClan.Heroes;
            if (members != null && members.Count > 0)
                foreach (Hero member in members)
                    if (member != Hero.MainHero && !member.IsChild && member.IsAlive && !council.IsMember(member))
                        this.AvailableGovernors
                                .Add(new CouncilMemberVM(member, new Action<SettlementGovernorSelectionItemVM>(this.OnSelection),
                                Position, council.GetCompetence(member, Position)));
        }

        private void OnSelection(SettlementGovernorSelectionItemVM item)
        {
            onDone(item.Governor);
        }
    }
}
