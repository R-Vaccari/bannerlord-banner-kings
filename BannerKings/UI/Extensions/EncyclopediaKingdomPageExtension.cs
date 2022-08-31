using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions
{
    [PrefabExtension("EncyclopediaFactionPage",
        "descendant::ListPanel[@Id='RightSideList']/Children/Widget[1]/Children/ListPanel[1]/Children",
        "EncyclopediaFactionPage")]
    internal class EncyclopediaFactionPageExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public EncyclopediaFactionPageExtension()
        {
            var titles1 = new XmlDocument();
            titles1.LoadXml(
                "<EncyclopediaDivider MarginTop=\"30\" Parameter.Title=\"@DemesneText\" Parameter.ItemList=\"..\\TitlesGrid\"/>");
            var titles2 = new XmlDocument();
            titles2.LoadXml(
                "<GridWidget Id=\"TitlesGrid\" DataSource=\"{Titles}\" WidthSizePolicy=\"CoverChildren\" HeightSizePolicy=\"CoverChildren\" SuggestedWidth=\"1500\" SuggestedHeight=\"1000\" DefaultCellWidth=\"1500\" DefaultCellHeight=\"800\" HorizontalAlignment=\"Center\" ColumnCount=\"1\" MarginTop=\"10\" MarginLeft=\"15\"><ItemTemplate><DemesneHierarchy/></ItemTemplate></GridWidget>");

           // nodes = new List<XmlNode>
            //    {titles1, titles2};
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 13;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }
}