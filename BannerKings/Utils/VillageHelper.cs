using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace BannerKings.Utils
{
    public static class VillageHelper
    {

        public static List<(ItemObject, float)> GetProductions(VillageData villageData)
        {
            List<(ItemObject, float)> productions = new List<(ItemObject, float)>(villageData.Village.VillageType.Productions);

            float tannery = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Tannery);
            if (tannery > 0)
            {
                /*ItemObject randomItem = this.GetRandomItem(production.Outputs[i].Item1, town);
                if (randomItem != null)
                {
                    list.Add(new ValueTuple<ItemObject, int>(randomItem, item));
                    num3 += town.GetItemPrice(randomItem, null, true) * item;
                } WorkshopCampaignBehavior for reference how to add arms to production    */
                productions.Add(new ValueTuple<ItemObject, float>(Game.Current.ObjectManager.GetObject<ItemObject>("leather"), tannery * 0.5f));
            }
                

            return productions;
        }

    }
}
