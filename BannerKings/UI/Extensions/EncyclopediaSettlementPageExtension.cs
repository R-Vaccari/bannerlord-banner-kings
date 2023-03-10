using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions
{
    [PrefabExtension("EncyclopediaSettlementPage",
        "descendant::ListPanel[@Id='RightSideList']/Children/Widget[1]/Children/ListPanel[1]/Children",
        "EncyclopediaSettlementPage")]
    internal class EncyclopediaSettlementPageExtension : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public EncyclopediaSettlementPageExtension()
        {   
            var workshops = new XmlDocument();
            workshops.LoadXml(
                "<SettlementEncyclopediaExtension/>");
            nodes = new List<XmlNode>
                { workshops };
        }

        public override InsertType Type => InsertType.Child;
        public override int Index => 7;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }
}