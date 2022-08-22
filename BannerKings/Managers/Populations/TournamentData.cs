using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations
{
    public class TournamentData : BannerKingsData
    {
        private Town town;

        public TournamentData(Town town)
        {
            this.town = town;
            roster = new ItemRoster();
            active = true;
        }

        [SaveableProperty(1)] private ItemRoster roster { get; set; }

        [SaveableProperty(2)] private ItemObject prize { get; set; }

        [SaveableProperty(3)] private bool active { get; set; }

        public bool Active
        {
            get => active;
            set => active = value;
        }

        public ItemRoster Roster => roster;

        public ItemObject Prize
        {
            get
            {
                if (prize == null)
                {
                    var items = new List<ItemObject>();
                    foreach (var element in roster)
                    {
                        var equipment = element.EquipmentElement;
                        var item = equipment.Item;
                        if (item != null)
                        {
                            if (item.IsMountable || item.HasWeaponComponent || item.HasArmorComponent)
                            {
                                items.Add(item);
                            }
                        }
                    }

                    if (items.Count > 0)
                    {
                        items.Sort((x, y) => x.Value.CompareTo(y.Value));
                        prize = items[0];
                    }
                }

                return prize;
            }
        }

        internal override void Update(PopulationData data)
        {
            if (!data.Settlement.Town.HasTournament)
            {
                active = false;
            }
        }
    }
}