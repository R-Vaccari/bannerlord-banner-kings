using System.Collections.Generic;
using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace BannerKings.UI.Extensions
{
    [PrefabExtension("TownManagement", "descendant::ListPanel[@Id='ManagementPlacementList']/Children/Widget[2]/Children/ListPanel[1]", "TownManagement")]
    internal class TownManagementExtensions1 : PrefabExtensionInsertPatch
    {
        private readonly List<XmlNode> nodes;

        public TownManagementExtensions1()
        {
            var firstChild = new XmlDocument();
            firstChild.LoadXml(
                "<TownManagementExtension/>");
            nodes = new List<XmlNode> {firstChild};
        }

        public override InsertType Type => InsertType.Replace;
        public override int Index => 1;

        [PrefabExtensionXmlNodes] public IEnumerable<XmlNode> Nodes => nodes;
    }
}