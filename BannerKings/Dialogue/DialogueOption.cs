using TaleWorlds.Localization;

namespace BannerKings.Dialogue
{
    public class DialogueOption
    {
        public DialogueOption(TextObject text, float relationWeight, float calculatingWeight, float mercyWeight, float honorWeight, float generousWeight, bool isDefault = false)
        {
            Text = text;
            RelationWeight = relationWeight;
            CalculatingWeight = calculatingWeight;
            MercyWeight = mercyWeight;
            HonorWeight = honorWeight;
            GenerousWeight = generousWeight;
            IsDefault = isDefault;
        }

        public bool IsDefault { get; private set; }
        public TextObject Text { get; private set; }
        public float RelationWeight { get; private set; }
        public float CalculatingWeight { get; private set; }
        public float MercyWeight { get; private set; }
        public float HonorWeight { get; private set; }
        public float GenerousWeight { get; private set; }
    }
}
