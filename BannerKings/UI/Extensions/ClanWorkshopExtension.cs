using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using System.Collections.Generic;
using System.Xml;

namespace BannerKings.UI.Extensions
{
    internal class ClanWorkshopExtension
    {

        [PrefabExtension("ClanIncomeWorkshop", "descendant::Widget/Children/ListPanel[1]/Children", "ClanIncomeWorkshop")]
        internal class ClanWorkshopInfoPatch : PrefabExtensionInsertPatch
        {
            private readonly List<XmlNode> nodes;

            public ClanWorkshopInfoPatch()
            {
                var list = new XmlDocument();
                list.LoadXml(
                   "<ListPanel DataSource=\"{..\\WorkshopInfo}\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" StackLayout.LayoutMethod=\"VerticalBottomToTop\"><ItemTemplate><Widget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\"><Children><ListPanel WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" MarginBottom=\"15\" StackLayout.LayoutMethod=\"HorizontalLeftToRight\"><Children><TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" Brush=\"Clan.Stat.Name.Text\" Text=\"@Description\" /><TextWidget WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" Brush=\"Clan.Stat.Value.Text\" MarginLeft=\"10\" Text=\"@Value\" /></Children></ListPanel></Children><HintWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" DataSource=\"{Hint}\" Command.HoverBegin=\"ExecuteBeginHint\" Command.HoverEnd=\"ExecuteEndHint\" /></Widget></ItemTemplate></ListPanel>");

                var button = new XmlDocument();
                button.LoadXml("<ButtonWidget DataSource=\"{..}\" DoNotPassEventsToChildren=\"true\" WidthSizePolicy=\"Fixed\" HeightSizePolicy=\"Fixed\" IsEnabled=\"@CanUpgrade\" HorizontalAlignment=\"Center\" MarginTop=\"20\" Brush=\"ButtonBrush2\" Command.Click=\"ExecuteUpgrade\" UpdateChildrenStates=\"true\" SuggestedWidth=\"230\" SuggestedHeight=\"40\"><Children><TextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"StretchToParent\" Brush=\"Kingdom.GeneralButtons.Text\" IsEnabled=\"@CanUpgrade\" Text=\"@UpgradeText\" /></Children></ButtonWidget>");

                nodes = new List<XmlNode> { list, button };
            }

            public override InsertType Type => InsertType.Child;
            public override int Index => 7;

            [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
        }
    }
}
