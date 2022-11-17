using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions
{
    /*[PrefabExtension("EncyclopediaHeroPage",
        "descendant::ListPanel[@Id='RightSideList']/Children/Widget[1]/Children/ListPanel[1]/Children",
        "EncyclopediaHeroPage")]
    internal class EncyclopediaHeroPageExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public EncyclopediaHeroPageExtension()
        {
            var marriage1 = new XmlDocument();
            marriage1.LoadXml(
                "<EncyclopediaDivider Id=\"MarriageDivider\" MarginTop=\"50\" Parameter.Title=\"@MarriageText\" Parameter.ItemList=\"..\\MarriageGrid\"/>");
            var marriage2 = new XmlDocument();
            marriage2.LoadXml(
                "<GridWidget Id=\"MarriageGrid\" DataSource=\"{Marriage}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" SuggestedWidth=\"350\" SuggestedHeight=\"350\" DefaultCellWidth=\"100\" DefaultCellHeight=\"100\" HorizontalAlignment=\"Left\" ColumnCount=\"7\" MarginTop=\"10\" MarginLeft=\"15\"><ItemTemplate><ListPanel HeightSizePolicy =\"CoverChildren\" WidthSizePolicy=\"CoverChildren\" MarginLeft=\"15\" MarginTop=\"3\"><Children><AutoHideRichTextWidget HeightSizePolicy =\"CoverChildren\" WidthSizePolicy=\"CoverChildren\" VerticalAlignment=\"Center\" Brush=\"Encyclopedia.Stat.DefinitionText\" Text=\"@Definition\" MarginRight=\"5\"/><AutoHideRichTextWidget HeightSizePolicy =\"CoverChildren\" WidthSizePolicy=\"CoverChildren\" VerticalAlignment=\"Center\" Brush=\"Encyclopedia.Stat.ValueText\" Text=\"@Value\" PositionYOffset=\"2\" /></Children></ListPanel></ItemTemplate></GridWidget>");

            nodes = new List<XmlNode>
                {marriage1, marriage2 };
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 5;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    } */
}