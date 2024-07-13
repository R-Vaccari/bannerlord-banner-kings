using System.Collections.Generic;
using BannerKings.Managers.Goals.Decisions;
using BannerKings.Settings;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals
{
    public class DefaultGoals : DefaultTypeInitializer<DefaultGoals, Goal>
    {
        public override IEnumerable<Goal> All
        {
            get
            {
                //yield return MakeCamp;
                yield return CallBannersGoal;
                yield return AssumeCulture;
                yield return LevyDuty;
                if (BannerKingsSettings.Instance.Feasts)
                {
                    yield return OrganizeFeastDecision;
                }
                 
                yield return AcquireBookDecision;
                yield return RecruitCompanionDecision;
                yield return RequestCouncil;
                yield return RequestPeerageDecision;
                yield return DemesneLawChangeDecision;
                yield return FoundKingdomGoal;
                yield return CalradicEmpireGoal;
                yield return RelocateCourtGoal;
                yield return SentenceCriminal;
                yield return FaithLeaderDecision;
                yield return MergeArmy;
                foreach (Goal item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public Goal FaithLeaderDecision { get; } = new FaithLeaderDecision();
        public Goal LevyDuty { get; } = new LevyDuty();
        public Goal AssumeCulture { get; private set; } = new AssumeCultureDecision();
        public Goal CalradicEmpireGoal { get; private set; } = new CalradicEmpireGoal();
        public Goal FoundKingdomGoal { get; private set; } = new FoundKingdomGoal();
        public Goal RecruitCompanionDecision { get; private set; } = new RecruitCompanionDecision();
        public RequestCouncilDecision RequestCouncil { get; private set; } = new RequestCouncilDecision();
        public Goal AcquireBookDecision { get; private set; } = new AcquireBookDecision();
        public Goal DemesneLawChangeDecision { get; private set; } = new DemesneLawChangeDecision();
        public RequestPeerageDecision RequestPeerageDecision { get; private set; } = new RequestPeerageDecision();
        public OrganizeFeastDecision OrganizeFeastDecision { get; private set; } = new OrganizeFeastDecision();
        public CallBannersGoal CallBannersGoal { get; private set; } = new CallBannersGoal();
        public Goal RelocateCourtGoal { get; } = new MoveCourtDecision();
        public Goal SentenceCriminal { get; } = new SentenceCriminalDecision();
        public Goal FoundEmpire { get; } = new FoundEmpireGoal();
        public Goal MakeCamp { get; } = new MakeCamp();
        public Goal MergeArmy { get; } = new MergeArmyGoal();
        public Goal PetitionRight { get; } = new PetitionRightGoal();

        public override void Initialize()
        {
            MakeCamp.Initialize(new TextObject("{=XRknzX3j}Make Camp"),
                null);

            PetitionRight.Initialize(new TextObject("{=!}Petition Right"),
                new TextObject("{=!}Petition a right to your suzerain"));

            MergeArmy.Initialize(new TextObject("{=!}Merge Armies"),
                new TextObject("{=!}Summon another existing army within your realm to merge with yours. The army to be merged must not be otherwise preoccupied, such as in a battle or in a siege. The leader of said army must be of same or lower rank than yours - a county holder cannot summon a duchy holder. Merging an army costs a reduced amount of influence compared to gathering it by yourself."));

            FoundEmpire.Initialize(new TextObject("{=e0t4jZoO}Found Empire"),
                 new TextObject("{=Zi7h8WK3}Found an Empire-level title. An Empire is the highest form of title, ruling over kingdoms. Empires may absorb kingdom titles as their vassals through the process of De Jure Drift.{newline}{newline}"));

            LevyDuty.Initialize(new TextObject("{=C8eDVQJw}Levy Duty"),
                new TextObject("{=!}Levy a duty from one of your vassals, such as taxes. Duties can be levied according to your kingdom's contract aspects. Levying a duty implies a relation loss with the vassal.{newline}"));

            SentenceCriminal.Initialize(new TextObject("{=pKoKaKNd}Sentence Criminal"),
                new TextObject("{=R6X7JFKz}As a Peer within a realm, you are able to sentence those found to be criminals that you hold within your dungeons."));

            RelocateCourtGoal.Initialize(new TextObject("{=v094GOtN}Relocate Court"),
                new TextObject("{=ojVEsng4}Relocate your House's court to a different castle or town."));

            FaithLeaderDecision.Initialize(new TextObject("{=!}Create Faith Leader"),
                new TextObject("{=!}As a ruler, endorse a new leader for your faith group. The possible faith leaders vary according to how the faith group works. A Faith Leader is important to push the faith's fervor, as well as sanctioning holy wars.\n"));

            AssumeCulture.Initialize(new TextObject("{=LcqUwqJz}Assume Culture"),
                new TextObject("{=XCancyYB}Assume a culture different than your current. Cultures can be assumed from settlements, your spouse or your faction leader. Direct family members will assume the culture as well. Assuming a culture yields a significant negative impact on clan renown.\n\n"));

            CalradicEmpireGoal.Initialize(new TextObject("{=cZzO6kya}Reform the Imperium Calradium"),
                new TextObject("{=!}Re-establish the former Calradian Empire. The Empire spanned most of the continent before emperor Arenicos died without a clear heir. By reforming the empire, you crush the validity of claimants, and ahead of you lies a new path for greatness. You must bring all imperial duchies under control of your realm.{newline}{newline}"));

            FoundKingdomGoal.Initialize(new TextObject("{=nbV21qZv}Found Kingdom"),
                new TextObject("{=XpFaiiny}Establish your own kingdom title. This new title will be bound to your Kingdom faction, and represent it in terms of Demesne laws, Succession and Inheritance laws, and all other types of laws attached to titles. Your faction must be one that is not already represented by a sovereign-level title (Kingdom or Empire titles)."));

            RecruitCompanionDecision.Initialize(new TextObject("{=HcGkCnSH}Seek Guests"),
                new TextObject("{=Ug94AACX}Invite guests to your court. They will live within your court for some time, where you can reliably find them. Seeking out guests costs influence relative to your House's position. Guests of different cultures and expertises can be sought out, for different costs."));
            
            RequestCouncil.Initialize(new TextObject("{=oBxXQmTb}Request Council Position"),
                new TextObject("{=7aLyDGEt}Request a position in your suzerain's council."));

            AcquireBookDecision.Initialize(new TextObject("{=DNAVAvqp}Acquire Book"),
                new TextObject("{=b4tSEcHn}Acquire a book from local book seller. Books can be read for skill improvements and progression in Scholarship."));

            DemesneLawChangeDecision.Initialize(new TextObject("{=YgefyGT4}Propose Demesne Law Change"),
                new TextObject("{=Ba2hpnco}Propose a contract change to your faction's titles."));

            OrganizeFeastDecision.Initialize(new TextObject("{=RH2NC5ij}Organize Feast"),
                new TextObject("{=8XXOBM1L}Organize a feast. Summon lords of the realm to one of your towns or castles, and celebrate with bountiful food. Feasts are an opportunity to improve relations with your Peers. However, some planning is necessary - you don't want your guests out of food or alcohol! Despite all planning, some unfortunate events may occur...\n"));

            RequestPeerageDecision.Initialize(new TextObject("{=sdpM1PD3}Request Full Peerage"),
                new TextObject("{=O7LLRFEX}Request the recognition of your family as a full Peer of the realm. A full Peer does not have legal restrictions on voting, starting elections, granting knighthood, hosting a council or being awarded fiefs. They are the very top of the realm's nobility. Successfully requesting Peerage will require renown (clan tier 4 minimum is recommended) and having good relations with full Peers. Holding property (caravans, workshops, estates, lordships) is a good positive factor as well.\n"));
            
            CallBannersGoal.Initialize(new TextObject("{=zzjbxN9h}Call Banners"),
                new TextObject("{=!}Establish your own De Jure kingdom title. Your faction must be one that is not already represented by a kingdom title (a title hierarchy of its own)."));
        }
    }
}