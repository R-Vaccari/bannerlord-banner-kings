using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Guilds;

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
            TextObject result = null;
            switch (Trade)
            {
                case GuildTrade.Merchants:
                    result = new TextObject("{=!}Merchants Guild");
                    break;
                case GuildTrade.Masons:
                    result = new TextObject("{=!}Masons Guild");
                    break;
                default:
                    result = new TextObject("{=!}Metalsmiths Guild");
                    break;
            }

            return result;
        }
    }

    public TextObject Description
    {
        get
        {
            TextObject result = null;
            switch (Trade)
            {
                case GuildTrade.Merchants:
                    result = new TextObject();

                    break;
                case GuildTrade.Masons:
                    result = new TextObject();
                    break;
                default:
                    result = new TextObject();
                    break;
            }

            return result;
        }
    }

    public IEnumerable<ValueTuple<ItemObject, float>> Productions
    {
        get
        {
            IEnumerable<ValueTuple<ItemObject, float>> result = null;
            switch (Trade)
            {
                case GuildTrade.Metalworkers:
                    result = new List<ValueTuple<ItemObject, float>>
                    {
                        (Game.Current.ObjectManager.GetObjectTypeList<ItemObject>().First(x => x.StringId == "tools"),
                            1f)
                    };
                    break;
                default:
                    result = new List<ValueTuple<ItemObject, float>>();
                    break;
            }

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