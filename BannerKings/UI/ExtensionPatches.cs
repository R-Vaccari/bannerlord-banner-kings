
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BannerKings.UI
{
    internal class ExtensionPatches
    {
        /*
        [PrefabExtension("ClanScreen", "descendant::Widget[@Id='ClanScreenWidget']/Children/OptionsTabToggle", "ClanScreen")]
        internal class AppendExamplePatch : PrefabExtensionInsertPatch
        {
            public override InsertType Type => InsertType.Append;

            private XmlDocument document;

            public TestPrefabExtensionReplacePatch()
            {
                document = new XmlDocument();
                document.LoadXml("<OptionsTabToggle Id=\"AppendedTabToggle\"/>");
            }

            [PrefabExtensionXmlNode]
            public XmlNode GetPatchContent() => document.DocumentElement;
        } */
    }
}
