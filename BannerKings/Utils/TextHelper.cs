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
                return new TextObject("{=!}Luxury Goods");
            }

            if (type == ConsumptionType.Industrial)
            {
                return new TextObject("{=!}Industrial Goods");
            }

            if (type == ConsumptionType.Food)
            {
                return new TextObject("{=!}Food Goods");
            }

            return new TextObject("{=!}General Goods");
        }

        public static TextObject GetKnightTitle(CultureObject culture, bool female, bool plural)
        {
            string id = culture.StringId;
            if (id == "battania")
            {
                if (plural) return new TextObject("{=!}Fianna");
                return new TextObject("{=!}Fiann");
            }

            if (id == "sturgia")
            {
                if (plural) return new TextObject("{=!}Druzhina");
                if (female) return new TextObject("{=!}Druzhinnica");
                return new TextObject("{=!}Druzhinnik");
            }

            if (id == "empire")
            {
                if (plural) return new TextObject("{=!}Pronoiarii");
                return new TextObject("{=!}Pronoiarius");
            }

            if (id == "aserai")
            {
                if (plural) return new TextObject("{=!}Fursaan");
                return new TextObject("{=!}Faaris");
            }

            if (id == "khuzait")
            {
                if (plural) return new TextObject("{=!}Kheshignud");
                return new TextObject("{=!}Kheshig");
            }

            if (plural) return new TextObject("{=ph4LMn6k}Knights");
            if (female) return new TextObject("{=!}Knightess");
            return new TextObject("{=!}Knight");
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
            TextObject title = female ? new TextObject("{=8V8i6QCm}Lady") : new TextObject("{=!}Lord"); ;
            if (culture != null) 
            {
                string id = culture.StringId;
                if (id == "battania") title = female ? new TextObject("{=tD38koMS}Baintighearna") :
                    new TextObject("{=!}Tighearna");
                else if (id == "empire") title = female ? new TextObject("{=go9MmDKr}Domina") :
                        new TextObject("{=!}Dominus");
                else if (id == "sturgia") title = female ? new TextObject("{=Ec79HNiF}Gospoda") : 
                        new TextObject("{=!}Gospodin");
                else if (id == "aserai") title = female ? new TextObject("{=VGXsX6Ue}Beghum") :
                        new TextObject("{=!}Mawlaa");
                else if (id == "khuzait") title = female ? new TextObject("{=dB9Rfp6W}Khatagtai") :
                        new TextObject("{=!}Erxem");
            }

            return title;
        }

        private static TextObject GetBaronyTitle(bool female, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=yxq4RV7E}Baroness") : new TextObject("{=!}Baron");
            if (culture != null)
            {
                string id = culture.StringId;
                if (id == "battania") title = female ? new TextObject("{=811sxLhn}Thaoiseach") :
                    new TextObject("{=!}Toisiche");
                else if (id == "empire") title = female ? new TextObject("{=dYq0qGzZ}Baronessa") :
                        new TextObject("{=!}Baro");
                else if (id == "sturgia") title = female ? new TextObject("{=bYQLoRUt}Voivodina") :
                        new TextObject("{=!}Voivode");
                else if (id == "aserai") title = female ? new TextObject("{=LYY1ZegU}Walia") :
                        new TextObject("{=!}Wali");
                else if (id == "khuzait") title = female ? new TextObject("{=Ajj9ptAU}Begum") :
                        new TextObject("{=!}Bey");
            }

            return title;
        }

        private static TextObject GetCountyTitle(bool female, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=o513XU29}Countess") : new TextObject("{=!}Count");
            if (culture != null)
            {
                string id = culture.StringId;
                if (id == "battania") title = female ? new TextObject("{=FMWNKESs}Bantiarna") :
                    new TextObject("{=!}Mormaer");
                else if (id == "empire") title = female ? new TextObject("{=ex7NjOtr}Cometessa") :
                        new TextObject("{=!}Conte");
                else if (id == "sturgia") title = female ? new TextObject("{=KTxFYNyo}Boyarina") :
                        new TextObject("{=!}Boyar");
                else if (id == "aserai") title = female ? new TextObject("{=AUoParHT}Shaykah") :
                        new TextObject("{=!}Sheikh");
                else if (id == "khuzait") title = female ? new TextObject("{=cepkCz19}Khanum") :
                        new TextObject("{=!}Khan");
            }

            return title;
        }

        private static TextObject GetDuchyTitle(bool female, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=5uFw1EmO}Duchess") : new TextObject("{=!}Duke");
            if (culture != null)
            {
                string id = culture.StringId;
                if (id == "battania") title = female ? new TextObject("{=XLAgsQ0J}Banrigh") :
                    new TextObject("{=!}Rìgh");
                else if (id == "empire") title = female ? new TextObject("{=5aCrjmFi}Ducissa") :
                        new TextObject("{=!}Dux");
                else if (id == "sturgia") title = female ? new TextObject("{=S3kc2bhW}Knyaginya") : 
                        new TextObject("{=1XDPfDim}Knyaz");
                else if (id == "aserai") title = female ? new TextObject("{=MVjsWtcZ}Emira") :
                        new TextObject("{=!}Emir");
                else if (id == "khuzait") title = female ? new TextObject("{=Mfuxa8SP}Bekhi") :
                        new TextObject("{=!}Baghatur");
            }

            return title;
        }

        private static TextObject GetKingdomTitle(bool female, GovernmentType government, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=JmdALFU2}Queen") : new TextObject("{=!}King");
            if (culture != null)
            {
                string id = culture.StringId;
                if (id == "battania") title = female ? new TextObject("{=25sz3WPn}Ard-Banrigh") :
                    new TextObject("{=!}Ard-Rìgh");
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
                            : new TextObject("{=!}Rex");
                    }
                }
                else if (id == "sturgia") title = female ? new TextObject("{=LmHWN0vt}Velikaya Knyaginya") : 
                        new TextObject("{=!}Velikiy Knyaz");
                else if (id == "aserai") title = female ? new TextObject("{=DQXH6NeY}Sultana") :
                        new TextObject("{=!}Sultan");
                else if (id == "khuzait") title = female ? new TextObject("{=SdJk1Vpf}Khatun") :
                        new TextObject("{=!}Khagan");
            }

            return title;
        }

        private static TextObject GetEmpireTitle(bool female, GovernmentType government, CultureObject culture = null)
        {
            TextObject title = female ? new TextObject("{=!}Empress") : new TextObject("{=!}Emperor");
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
