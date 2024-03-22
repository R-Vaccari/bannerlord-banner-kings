using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Recruits
{
    public class DefaultRecruitSpawns : DefaultTypeInitializer<DefaultRecruitSpawns, RecruitSpawn>
    {
        public List<RecruitSpawn> XmlSpawns { get; private set; } = new List<RecruitSpawn>();
        public bool IgnoreCode { get; set; } = false;

        public override IEnumerable<RecruitSpawn> All
        {
            get
            {
                if (!IgnoreCode)
                {
                    foreach (RecruitSpawn item in ModAdditions)
                    {
                        yield return item;
                    }
                }

                foreach (RecruitSpawn item in XmlSpawns)
                {
                    yield return item;
                }
            }
        }

        public List<RecruitSpawn> GetPossibleSpawns(CultureObject culture, Settlement settlement = null)
        {
            List<RecruitSpawn> spawns = new List<RecruitSpawn>(2);
            if (culture == null) return spawns;

            foreach (RecruitSpawn spawn in All)
            {
                if (spawn.FiefStrings.Count > 0)
                {
                    if (spawn.FiefStrings.Contains(settlement.StringId) || (settlement.IsVillage &&
                        spawn.FiefStrings.Contains(settlement.Village.Bound.StringId)))
                    {
                        spawns.Add(spawn);
                    }

                    continue;
                }

                if (culture != spawn.Culture) continue;

                if (settlement.IsVillage && !spawn.SpawnVillages) continue;
                if (settlement.IsCastle && !spawn.SpawnCastles) continue;
                if (settlement.IsTown && !spawn.SpawnTowns) continue;

                if (spawn.Kingdom != null && settlement.MapFaction != spawn.Kingdom)
                {
                    continue;
                }

                spawns.Add(spawn);
            }

            return spawns;
        }

        public List<RecruitSpawn> GetPossibleSpawns(CultureObject culture, PopType popType, Settlement settlement = null)
        {
            return GetPossibleSpawns(culture, settlement).Where(x => x.GetChance(popType) > 0f).ToList();
        }  

        public override void Initialize()
        {
            if (BannerKingsConfig.Instance.RecruitsXmlPath != null && XmlSpawns.IsEmpty())
            {
                XmlDocument doc = Utils.Helpers.CreateDocumentFromXmlFile(BannerKingsConfig.Instance.RecruitsXmlPath);
                XmlSpawns = new List<RecruitSpawn>(doc.ChildNodes.Count);
                var recruitsNode = doc.ChildNodes[1].ChildNodes[0];
                IgnoreCode = bool.Parse(recruitsNode.Attributes["wipeCodeRecruits"].Value);

                foreach (XmlNode node in recruitsNode.ChildNodes)
                {
                    if (node.Name != "recruit") return;

                    string character = "";
                    string kingdom = "";
                    string culture = "";
                    List<string> fiefs = new List<string>();
                    float slaves = 0f;
                    float serfs = 0f;
                    float tenants = 0f;
                    float craftsmen = 0f;
                    float nobles = 0f;
                    bool villages = true;
                    bool castles = true;
                    bool towns = true;

                    character = node.Attributes["character"].Value;
                    CharacterObject troop = GetTroop(character);
                    if (troop == null)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=!}Invalid character {CHAR} from recruits path {PATH}")
                            .SetTextVariable("CHAR", character)
                            .SetTextVariable("PATH", BannerKingsConfig.Instance.RecruitsXmlPath).ToString(),
                            Color.FromUint(Utils.TextHelper.COLOR_LIGHT_RED)));
                        continue;
                    }

                    if (node.Attributes["kingdom"] != null) kingdom = node.Attributes["kingdom"].Value;
                    if (node.Attributes["culture"] != null) culture = node.Attributes["culture"].Value;
                    if (node.Attributes["slaves"] != null) slaves = float.Parse(node.Attributes["slaves"].Value);
                    if (node.Attributes["serfs"] != null) serfs = float.Parse(node.Attributes["serfs"].Value);
                    if (node.Attributes["tenants"] != null) tenants = float.Parse(node.Attributes["tenants"].Value);
                    if (node.Attributes["craftsmen"] != null) craftsmen = float.Parse(node.Attributes["craftsmen"].Value);
                    if (node.Attributes["nobles"] != null) nobles = float.Parse(node.Attributes["nobles"].Value);
                    if (node.Attributes["spawnVillages"] != null) villages = bool.Parse(node.Attributes["spawnVillages"].Value);
                    if (node.Attributes["spawnCastles"] != null) castles = bool.Parse(node.Attributes["spawnCastles"].Value);
                    if (node.Attributes["spawnTowns"] != null) towns = bool.Parse(node.Attributes["spawnTowns"].Value);

                    RecruitSpawn spawn = new RecruitSpawn();
                    spawn.Initialize(GetTroop(character),
                        GetCulture(culture),
                        new Dictionary<PopType, float>()
                        {
                            { PopType.Slaves, slaves },
                            { PopType.Serfs, serfs },
                            { PopType.Tenants, tenants },
                            { PopType.Craftsmen, craftsmen },
                            { PopType.Nobles, nobles }
                        },
                        Kingdom.All.FirstOrDefault(x => x.StringId == "kingdom"),
                        villages,
                        towns,
                        castles);
                    foreach (string fief in fiefs) spawn.AddFiefString(fief);
                    XmlSpawns.Add(spawn);
                }
            }
        }

        private CultureObject GetCulture(string id) => TaleWorlds.CampaignSystem.Campaign.Current.ObjectManager.GetObject<CultureObject>(x => x.StringId == id);
        private CharacterObject GetTroop(string id) => TaleWorlds.CampaignSystem.Campaign.Current.ObjectManager.GetObject<CharacterObject>(x => x.StringId == id);
    }
}
