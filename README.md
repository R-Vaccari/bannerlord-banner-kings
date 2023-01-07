# Banner Kings

[![CodeFactor](https://www.codefactor.io/repository/github/r-vaccari/bannerlord-banner-kings/badge)](https://www.codefactor.io/repository/github/r-vaccari/bannerlord-banner-kings)

<a href="https://www.buymeacoffee.com/basilevsmodding" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/arial-red.png" alt="Buy Me A Coffee" width="220px"></a> 
<a href="https://discord.gg/z7DS5R46wC" target="_blank"><img src="https://i.imgur.com/xsWPvks.png" alt="Discord" width="220px"></a>


Banner Kings is a suite of features developed for Mount & Blade: Bannerlord. The modification focuses on adding depth to gameplay. This is done by expanding and adding layers of complexity to non-combat related features of the game. Inspiration for the mod systems is mostly drawn from games such as Crusader Kings.

## Installation
This is a tutorial kindly made by a community member. I recommend using it if you are struggling to setup the mod.
https://www.youtube.com/watch?v=ssSwW2z80wU

## Features
Features are being described in the [wiki](https://github.com/R-Vaccari/bannerlord-banner-kings/wiki). The wiki is not considered finished and is always susceptible to updates.

## FAQ
- What is Banner Kings? Banner Kings is a suite of systems to deepen non-combat gameplay. It heavily revolves around settlement management and kingdom gameplay. Banner Kings draws ideas heavily from Paradox games, mainly Crusader Kings.
- Works adding to savegames? Yes. BK is optimized for game start due to how assigning ownership of titles to lords works. However it is able to be added into savegames even when these lords are dead / missing.
- Can it be removed from savegames? No. The mod is provided AS IS so use at your own discretion.
- How do I use the mod? You can get some insight from the [wiki](https://github.com/R-Vaccari/bannerlord-banner-kings/wiki). Better explanations on some key doubts are being made. You can also use [Discord](https://discord.gg/z7DS5R46wC) to ask questions and often other uses will answer themselves.

## Bug Reporting
- Try to make sure the mod is at fault. Just because this is probably the biggest mod you have installed, doesn't mean it's responsible for every issue you have. 
- Support is only provided through [Discord](https://discord.gg/z7DS5R46wC), or here with Github Issues. Bug reporting has a guide that must be followed, otherwise the issue will be terminated.

The mod is big, complicated and not perfect. If you have an issue, describe it purposefuly and in an educated manner, if you really want it fixed.

## Known Issues

- Missing texts or localisation keys.
- A minority of people have reported freezes. The reason for it is still unclear.
- A loading crash when combined with CE. CE will be updated with it fixed. A placeholder fix is provided in the [Discord](https://discord.gg/z7DS5R46wC).
- Most issues are reported and fixed first on [Discord](https://discord.gg/z7DS5R46wC) through test-versions. Test-versions are separate versions of the mod which are usually in-between 2 official updates, and are used to verify fixes mostly. A lot of issues are identified and fixed quickly, you don't need to make the 15th comment about it.

## For Modders
BannerKings was bult from the principle of being a framework. Both for multiple official versions of the mod and possibly third parties extensions of it.

### Extending Models
A lot of the mod's effects are implemented through models. Bannerlord models are me to be overridable. By default, only 1 mod can override each model type. However it is still possible to keep the effects of several mods through Inheritance.

#### Vanilla Models
Just like BannerKings inherits from Bannerlord models, your mod would need to inherit from BannerKings. Let's see an easy example.
```
public class BKCrimeModel : DefaultCrimeModel
{
    public override ExplainedNumber GetDailyCrimeRatingChange(IFaction faction, bool includeDescriptions = false)
    {
        var result = base.GetDailyCrimeRatingChange(faction, includeDescriptions);
        if (Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>()
            .HasDebuff(DefaultStartOptions.Instance.Outlaw))
        {
            return new ExplainedNumber(0f, includeDescriptions, DefaultStartOptions.Instance.Outlaw.Name);
        }

        return result;
    }
}
```
Here BannerKings extends the vanilla `DefaultCrimeModel` through it's class `BKCrimeModel`. This means that all the inherit logic from `DefaultCrimeModel` is maintained while allowing BannerKings to add it's own effects on top of it. Here we override the `GetDailyCrimeRatingChange` method, and we can access the super class' result by using `base.GetDailyCrimeRatingChange(faction, includeDescriptions)`. You can then add onto it, or even return a completely new result, as it's done here. 

Likewise, your mod, with your own `ModXCrimeModel` would extend `BKCrimeModel`. Thus the game would have the full effects of `ModXCrimeModel`, `BKCrimeModel` and `DefaultCrimeModel` combined.

#### BannerKings Models
To be done.

### Religions
To be done,
