using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Loudmon
namespace DCGO.CardEffects.EX11
{
    public class EX11_050 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4
                        && (targetPermanent.TopCard.EqualsTraits("Dark Dragon") 
                            || targetPermanent.TopCard.EqualsTraits("Evil Dragon"));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Shared OP / WD

            string SharedEffectName() => "Discard 2, delete 1 opponent's digimon with less DP then a [Dark Dragon]/[Evil Dragon] trait digimon.";

            string SharedEffectDescription(string tag) => $"[{tag}] Trash 2 cards in your hand. Then, delete 1 of your opponent's Digimon with as much or less DP as 1 of your [Dark Dragon] or [Evil Dragon] trait Digimon.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.HandCards.Count >= 1)
                {
                    int discardCount = Math.Min(2, card.Owner.HandCards.Count);

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: discardCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Discard,
                        cardEffect: activateClass);

                    yield return StartCoroutine(selectHandEffect.Activate());
                }

                #region Delete Digimon Setup

                Permanent selectedOwnerDigimon = null;

                List<Permanent> ownerDigimonList = card.Owner.GetBattleAreaDigimons();
                List<Permanent> opponentDigimonList = card.Owner.Enemy.GetBattleAreaDigimons();

                int highestDp = ownerDigimonList.Count > 0 ? ownerDigimonList.Max(x => x.DP) : -1;

                bool CanSelectOwnerDigimon(Permanent permanent)
                        => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                bool CanSelectOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.DP <= selectedOwnerDigimon.DP;
                }

                #endregion

                #region Select Digimon to Compare

                // Comparing to our highest dp is just used as a fast way to check if there is a least 1 vaild selection.
                if (ownerDigimonList.Count > 0 && opponentDigimonList.Any(x => x.DP <= highestDp))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnerDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon.", "The opponent is selecting 1 Digimon");

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedOwnerDigimon = permanent;
                        yield return null;
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                #endregion

                #region Select Digimon To Delete

                if (selectedOwnerDigimon != null && opponentDigimonList.Any(x => x.DP <= selectedOwnerDigimon.DP))
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                #endregion
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, (hashTable) => SharedActivateCoroutine(hashTable, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.None)
            {
                AddSkillClass addSkillClass = new AddSkillClass();
                addSkillClass.SetUpICardEffect("Your NSo trait Digimon gain Scapegoat", CanUseCondition, card);
                addSkillClass.SetUpAddSkillClass(cardSourceCondition: CardSourceCondition, getEffects: GetEffects);
                cardEffects.Add(addSkillClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) 
                        && CardEffectCommons.HasMatchConditionPermanent(PermanentCondition)
                        && card.Owner.HandCards.Count <= 4;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return card.Owner == permanent.TopCard.Owner
                        && CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.EqualsTraits("Dark Dragon")
                            || permanent.TopCard.EqualsTraits("Evil Dragon"));
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(cardSource)
                        && cardSource.Owner == card.Owner
                        && cardSource == cardSource.PermanentOfThisCard().TopCard
                        && PermanentCondition(cardSource.PermanentOfThisCard());
                }

                List<ICardEffect> GetEffects(CardSource cardSource, List<ICardEffect> cardEffects, EffectTiming _timing)
                {
                    if (_timing == EffectTiming.WhenPermanentWouldBeDeleted)
                    {
                        bool Condition()
                        {
                            return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(cardSource.PermanentOfThisCard(), card)
                                && (cardSource.EqualsTraits("Dark Dragon")
                                    || cardSource.EqualsTraits("Evil Dragon"));
                        }

                        cardEffects.Add(CardEffectFactory.ScapegoatSelfEffect(isInheritedEffect: false, card: cardSource, condition: Condition, effectName: "<Scapegoat>", effectDiscription: null));
                    }

                    return cardEffects;
                }
            }

            #endregion


            #region Inherit

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.ContainsTraits("Dark Dragon")
                            || permanent.TopCard.ContainsTraits("Evil Dragon"));
                }

                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && card.Owner.HandCards.Count <= 4;
                }

                cardEffects.Add(CardEffectFactory.ChangeSAttackStaticEffect(PermanentCondition, 1, true, card, CanUseCondition));
            }

            #endregion

            return cardEffects;
        }
    }
}
