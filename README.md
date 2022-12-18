# Banner Kings

[![CodeFactor](https://www.codefactor.io/repository/github/r-vaccari/bannerlord-banner-kings/badge)](https://www.codefactor.io/repository/github/r-vaccari/bannerlord-banner-kings)


Banner Kings is a suite of features developed for Mount & Blade: Bannerlord. The modification focuses on adding depth to gameplay. This is done by expanding and adding layers of complexity to non-combat related features of the game. Inspiration for the mod systems is mostly drawn from games such as Crusader Kings.

## Features
Features are being described in the [wiki](https://github.com/R-Vaccari/bannerlord-banner-kings/wiki). The wiki is not considered finished and is always susceptible to updates.

## Bug Reporting
Support is only provided through [Discord](https://discord.gg/z7DS5R46wC), or here with Github Issues. Bug reporting has a guide that must be followed, otherwise the issue will be terminated. Reports in Nexus page or any future mod pages will be 100% ignored.

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
