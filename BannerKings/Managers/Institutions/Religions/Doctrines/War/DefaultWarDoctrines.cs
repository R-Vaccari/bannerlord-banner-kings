using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Managers.Institutions.Religions.Doctrines.War;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Doctrines.Marriage
{
    public class DefaultWarDoctrines : DefaultTypeInitializer<DefaultWarDoctrines, WarDoctrine>
    {
        public WarDoctrine OpenWarfare { get; set; }
        public WarDoctrine Reclamation { get; set; }
        public WarDoctrine NoWarfare { get; set; }
        public override IEnumerable<WarDoctrine> All
        {
            get
            {
                yield return OpenWarfare;
                yield return Reclamation;
                yield return NoWarfare; 
            }
        }

        public override void Initialize()
        {
            OpenWarfare = new WarDoctrine("OpenWarfare",
                new TextObject("{=!}Open Warfare"),
                new TextObject("{=!}Open Warfare doctrine endorses both general forms of holy war: expansionist and reclamation holy wars. Expansionist wars are declared against enemies of the faith, while reclamation wars are targeted towards holy sites of the faith. Declaring a holy war requires a faith leader."),
                new TextObject(),
                new List<Doctrine>(),
                new Dictionary<CasusBelli, int>() 
                {
                    { DefaultCasusBelli.Instance.HolyWar, 1000 },
                    { DefaultCasusBelli.Instance.DivineReclamation, 600 },
                });

            Reclamation = new WarDoctrine("Reclamation",
                new TextObject("{=!}Reclamation Warfare"),
                new TextObject("{=!}Reclamation Warfare endorses reclamation holy wars. Reclamation wars are targeted towards holy sites or the faith seat of your faith, and may be used whenever enemies of the faith hold them. Declaring a holy war requires a faith leader."),
                new TextObject(),
                new List<Doctrine>(),
                new Dictionary<CasusBelli, int>()
                {
                    { DefaultCasusBelli.Instance.DivineReclamation, 500 },
                });

            NoWarfare = new WarDoctrine("NoWarfare",
                new TextObject("{=!}Forbidden Warfare"),
                new TextObject("{=!}Forbidden Warfare does not allow for any type of holy wars to be declared by the faith."),
                new TextObject(),
                new List<Doctrine>(),
                new Dictionary<CasusBelli, int>()
                {
                });
        }
    }
}
