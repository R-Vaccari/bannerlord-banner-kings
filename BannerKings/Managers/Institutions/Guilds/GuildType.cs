using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Guilds
{
    public class GuildType
    {
        public GuildType(GuildTrade trade)
        {
            Trade = trade;
        }

        public TextObject Name
        {
            get
            {
                var result = Trade switch
                {
                    GuildTrade.Merchants => new TextObject("{=!}Merchants Guild"),
                    GuildTrade.Masons => new TextObject("{=!}Masons Guild"),
                    _ => new TextObject("{=!}Metalsmiths Guild")
                };

                return result;
            }
        }

        public TextObject Description
        {
            get
            {
                var result = Trade switch
                {
                    GuildTrade.Merchants => new TextObject(),
                    GuildTrade.Masons => new TextObject(),
                    _ => new TextObject()
                };

                return result;
            }
        }

        public IEnumerable<ValueTuple<ItemObject, float>> Productions
        {
            get
            {
                IEnumerable<ValueTuple<ItemObject, float>> result = Trade switch
                {
                    GuildTrade.Metalworkers => new List<ValueTuple<ItemObject, float>>
                    {
                        (Game.Current.ObjectManager.GetObjectTypeList<ItemObject>().First(x => x.StringId == "tools"),
                            1f)
                    },
                    _ => new List<ValueTuple<ItemObject, float>>()
                };

                return result;
            }
        }

        public GuildTrade Trade { get; }
    }

    public enum GuildTrade
    {
        Merchants,
        Masons,
        Metalworkers
    }
}