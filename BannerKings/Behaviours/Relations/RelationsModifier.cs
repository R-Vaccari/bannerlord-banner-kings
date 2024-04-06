using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Relations
{
    public class RelationsModifier
    {
        [SaveableProperty(1)] public int Relation {  get; private set; }
        [SaveableProperty(2)] public TextObject Explanation { get; private set; }
        [SaveableProperty(3)] public CampaignTime Expiry { get; private set; }

        public RelationsModifier(int relation, TextObject explanation)
        {
            Relation = relation;
            Explanation = explanation;
            Expiry = CampaignTime.Never;
        }

        public RelationsModifier(int relation, TextObject explanation, CampaignTime expiry)
        {
            Relation = relation;
            Explanation = explanation;
            Expiry = expiry;
        }
    }
}
