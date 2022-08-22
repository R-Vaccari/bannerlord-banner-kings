using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Conditions
{
    internal abstract class BKCondition : BannerKingsObject
    {
        protected static TextObject FailedReasonText => GameTexts.FindText("str_bk_common.placeholder");

        protected BKCondition(string stringId) : base(stringId)
        {

        }
    }
}