using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions
{
    [PrefabExtension("DiplomacyPanel", "descendant::Widget[1]/Children/ListPanel[1]/Children/Widget[1]/Children/ListPanel[1]/Children", "DiplomacyPanel")]
    internal class KingdomDiplomacyExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public KingdomDiplomacyExtension()
        {
            var firstChild = new XmlDocument();
            firstChild.LoadXml(
                "<ListPanel IsVisible=\"@WarExists\" WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" MarginTop=\"20\" MarginBottom=\"20\" MarginRight=\"75\" MarginLeft=\"75\" StackLayout.LayoutMethod=\"HorizontalLeftToRight\" ><Children><ListPanel HorizontalAlignment=\"Center\" HeightSizePolicy=\"CoverChildren\" WidthSizePolicy=\"StretchToParent\" SuggestedWidth=\"100\" StackLayout.LayoutMethod=\"VerticalBottomToTop\"><Children><RichTextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" Brush=\"Clan.Leader.Text\" Brush.FontSize=\"20\" Text=\"@JustificationHeader\" MarginBottom=\"15\" /><RichTextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" Brush=\"Kingdom.Wars.Stat.Name.Text\" Brush.FontSize=\"30\" Text=\"@JustificationText\" /></Children></ListPanel><ListPanel HorizontalAlignment=\"Center\" HeightSizePolicy=\"CoverChildren\" WidthSizePolicy=\"StretchToParent\"   SuggestedWidth=\"100\" StackLayout.LayoutMethod=\"VerticalBottomToTop\"><Children><RichTextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" Brush=\"Clan.Leader.Text\" Brush.FontSize=\"24\" Text=\"@WarScoreHeader\" MarginBottom=\"15\" /><RichTextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" Brush=\"Kingdom.Wars.Stat.Name.Text\" Brush.FontSize=\"36\" Text=\"@WarScoreText\" /></Children></ListPanel><ListPanel HorizontalAlignment=\"Center\" HeightSizePolicy=\"CoverChildren\" WidthSizePolicy=\"StretchToParent\"   SuggestedWidth=\"100\" StackLayout.LayoutMethod=\"VerticalBottomToTop\"><Children><RichTextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" Brush=\"Clan.Leader.Text\" Brush.FontSize=\"20\" Text=\"@WarFatigueHeader\" MarginBottom=\"15\" /><RichTextWidget WidthSizePolicy=\"StretchToParent\" HeightSizePolicy=\"CoverChildren\" HorizontalAlignment=\"Center\" Brush=\"Kingdom.Wars.Stat.Name.Text\" Brush.FontSize=\"30\" Text=\"@WarFatigueText\" /></Children></ListPanel></Children></ListPanel>");
            nodes = new List<XmlNode> { firstChild };
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 2;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }
}