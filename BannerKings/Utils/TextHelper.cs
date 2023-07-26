using BannerKings.Managers.Titles;
using SandBox.BoardGames.Objects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Utils
{
    public static class TextHelper
    {
        public static uint COLOR_LIGHT_BLUE = 3468224;
        public static uint COLOR_LIGHT_RED = 13582400;
        public static uint COLOR_LIGHT_YELLOW = 16246615;

        public static TextObject GetConsumptionSatisfactionText(ConsumptionType type)
        {
            if (type == ConsumptionType.Luxury)
            {
                return new TextObject("{=FU9JZQuP}Luxury Goods");
            }

            if (type == ConsumptionType.Industrial)
            {
                return new TextObject("{=GsJWKW0o}Industrial Goods");
            }

            if (type == ConsumptionType.Food)
            {
                return new TextObject("{=SuHW6rM5}Food Goods");
            }

            return new TextObject("{=HoU7ZObZ}General Goods");
        }

        public static TextObject GetKnightTitle(CultureObject culture, bool female, bool plural)
        {
            string id = culture.StringId;
            if (id == "battania")
            {
                if (plural) return new TextObject("{=4b5WsUZ7}Fianna");
                return new TextObject("{=V7fYyS93}Fiann");
            }

            if (id == "sturgia")
            {
                if (plural) return new TextObject("{=TOWzCa0Y}Druzhina");
                if (female) return new TextObject("{=HEAw5x03}Druzhinnica");
                return new TextObject("{=Qucsad67}Druzhinnik");
            }

            if (id == "empire")
            {
                if (plural) return new TextObject("{=VLB8LyH0}Pronoiarii");
                return new TextObject("{=oapL1nXd}Pronoiarius");
            }

            if (id == "aserai")
            {
                if (plural) return new TextObject("{=kgPoTnBi}Fursaan");
                return new TextObject("{=GmHEzRcz}Faaris");
            }

            if (id == "khuzait")
            {
                if (plural) return new TextObject("{=RYd2Z1OU}Kheshignud");
                return new TextObject("Kheshig");
            }

            if (plural) return new TextObject("{=ph4LMn6k}Knights");
            if (female) return new TextObject("{=6LHeHpCo}Knightess");
            return new TextObject("{=W9G4PTpt}Knight");
        }

        public static TextObject GetPrinceTitles(GovernmentType government, bool isFemale, CultureObject culture = null)
        {
            TextObject result = null;

            if (culture!= null)
            {
                if (culture.StringId == "empire")
                {
                    result = isFemale ? new TextObject("{=gNVEqLz4}Principissa") : new TextObject("{=ouHkQtyZ}Princeps");
                }
                else if (culture.StringId == "sturgia")
                {
                    result = isFemale ? new TextObject("{=S3kc2bhW}Knyaginya"): new TextObject("{=1XDPfDim}Knyaz"); 
                }
                else if (culture.StringId == "battania") 
                {
                    result = isFemale ? new TextObject("{=RYoxePAG}Bana-Phrionnsa") : new TextObject("{=7z7iEwxU}Prionnsa");
                }
            }

            if (result == null)
            {
                result = isFemale ? new TextObject("{=e7Nhe2YX}Princess") : new TextObject("{=V219eHY6}Prince");
            }

            return result;
        }

        public static TextObject GetName(GovernmentType value)
        {
            return GameTexts.FindText("str_bk_" + value.ToString().ToLower());
        }

        public static TextObject GetName(InheritanceType value)
        {
            return GameTexts.FindText("str_bk_" + value.ToString().ToLower());
        }

        public static TextObject GetName(GenderLaw value)
        {
            return GameTexts.FindText("str_bk_" + value.ToString().ToLower());
        }

        public static TextObject GetName(SuccessionType value)
        {
            return GameTexts.FindText("str_bk_" + value.ToString().ToLower());
        }

        private static TextObject GetLordshipTitle(bool female, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=8V8i6QCm}Lady") : new TextObject("{=Jd1iqDAX}Lord"); ;
            if (culture != null) 
            {
                string id = culture.StringId;
                if (id == "battania") title = female ? new TextObject("{=tD38koMS}Baintighearna") :
                    new TextObject("{=8z4dKfDu}Tighearna");
                else if (id == "empire") title = female ? new TextObject("{=go9MmDKr}Domina") :
                        new TextObject("{=S5rJfgac}Dominus");
                else if (id == "sturgia") title = female ? new TextObject("{=Ec79HNiF}Gospoda") : 
                        new TextObject("Gospodin");
                else if (id == "aserai") title = female ? new TextObject("{=VGXsX6Ue}Beghum") :
                        new TextObject("{=jes0Kij8}Mawlaa");
                else if (id == "khuzait") title = female ? new TextObject("{=dB9Rfp6W}Khatagtai") :
                        new TextObject("{=uut6Aqo3}Erxem");
            }

            return title;
        }

        private static TextObject GetBaronyTitle(bool female, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=yxq4RV7E}Baroness") : new TextObject("{=LvgTvjd1}Baron");
            if (culture != null)
            {
                string id = culture.StringId;
                if (id == "battania") title = female ? new TextObject("{=811sxLhn}Thaoiseach") :
                    new TextObject("{=6XmUmfNm}Toisiche");
                else if (id == "empire") title = female ? new TextObject("{=dYq0qGzZ}Baronessa") :
                        new TextObject("{=DZ8MWLNZ}Baro");
                else if (id == "sturgia") title = female ? new TextObject("{=bYQLoRUt}Voivodina") :
                        new TextObject("{=P5tZwHtn}Voivode");
                else if (id == "aserai") title = female ? new TextObject("{=LYY1ZegU}Walia") :
                        new TextObject("{=Ry8vAGrA}Wali");
                else if (id == "khuzait") title = female ? new TextObject("{=Ajj9ptAU}Begum") :
                        new TextObject("{=A04x6QXO}Bey");
            }

            return title;
        }

        private static TextObject GetCountyTitle(bool female, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=o513XU29}Countess") : new TextObject("{=hG2krbg9}Count");
            if (culture != null)
            {
                string id = culture.StringId;
                if (id == "battania") title = female ? new TextObject("{=FMWNKESs}Bantiarna") :
                    new TextObject("{=hGD00EFy}Mormaer");
                else if (id == "empire") title = female ? new TextObject("{=ex7NjOtr}Cometessa") :
                        new TextObject("{=o8Cxhcm6}Conte");
                else if (id == "sturgia") title = female ? new TextObject("{=KTxFYNyo}Boyarina") :
                        new TextObject("{=t8LfQad9}Boyar");
                else if (id == "aserai") title = female ? new TextObject("{=AUoParHT}Shaykah") :
                        new TextObject("{=if2pPJ6z}Sheikh");
                else if (id == "khuzait") title = female ? new TextObject("{=cepkCz19}Khanum") :
                        new TextObject("{=R8N3D1L5}Khan");
            }

            return title;
        }

        private static TextObject GetDuchyTitle(bool female, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=5uFw1EmO}Duchess") : new TextObject("{=vFJ7NjqE}Duke");
            if (culture != null)
            {
                string id = culture.StringId;
                if (id == "battania") title = female ? new TextObject("{=XLAgsQ0J}Banrigh") :
                    new TextObject("{=f62sQAtH}R�gh");
                else if (id == "empire") title = female ? new TextObject("{=5aCrjmFi}Ducissa") :
                        new TextObject("{=oDRVNajf}Dux");
                else if (id == "sturgia") title = female ? new TextObject("{=S3kc2bhW}Knyaginya") : 
                        new TextObject("{=1XDPfDim}Knyaz");
                else if (id == "aserai") title = female ? new TextObject("{=MVjsWtcZ}Emira") :
                        new TextObject("{=CNgjQs21}Emir");
                else if (id == "khuzait") title = female ? new TextObject("{=Mfuxa8SP}Bekhi") :
                        new TextObject("{=yQvLRfx9}Baghatur");
            }

            return title;
        }

        private static TextObject GetKingdomTitle(bool female, GovernmentType government, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=JmdALFU2}Queen") : new TextObject("{=zyKSROjQ}King");
            if (culture != null)
            {
                string id = culture.StringId;
                if (id == "battania") title = female ? new TextObject("{=25sz3WPn}Ard-Banrigh") :
                    new TextObject("{=PFQ9jztU}Ard-R�gh");
                else if (id == "empire")
                {
                    if (government == GovernmentType.Republic)
                    {
                        title = female
                            ? new TextObject("{=gNVEqLz4}Principissa")
                            : new TextObject("{=ouHkQtyZ}Princeps");
                    }
                    else
                    {
                        title = female
                            ? new TextObject("{=BJvarzpV}Regina")
                            : new TextObject("{=fo59CGyy}Rex");
                    }
                }
                else if (id == "sturgia") title = female ? new TextObject("{=LmHWN0vt}Velikaya Knyaginya") : 
                        new TextObject("{=DA4APi0H}Velikiy Knyaz");
                else if (id == "aserai") title = female ? new TextObject("{=DQXH6NeY}Sultana") :
                        new TextObject("{=RSfZQRPy}Sultan");
                else if (id == "khuzait") title = female ? new TextObject("{=SdJk1Vpf}Khatun") :
                        new TextObject("{=oQULc2S8}Khagan");
            }

            return title;
        }

        private static TextObject GetEmpireTitle(bool female, GovernmentType government, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=gbpokx6s}Empress") : new TextObject("{=9WOQTiBr}Emperor");
            if (culture != null)
            {
                string id = culture.StringId;
                if (id == "empire") title = female ? new TextObject("{=b3dURwfW}Imperatrix") : 
                        new TextObject("{=uTLJjN8A}Imperator");
            }

            return title;
        }

        public static TextObject GetTitleHonorary(TitleType type, GovernmentType government, bool female, CultureObject culture = null)
        {
            TextObject title;
            if (type == TitleType.Lordship) title = GetLordshipTitle(female, culture);
            else if (type == TitleType.Barony) title = GetBaronyTitle(female, culture);
            else if (type == TitleType.County) title = GetCountyTitle(female, culture);
            else if (type == TitleType.Dukedom) title = GetDuchyTitle(female, culture);
            else if (type == TitleType.Kingdom) title = GetKingdomTitle(female, government, culture);
            else title = GetEmpireTitle(female, government, culture);

            return title;
        }
    }
}
