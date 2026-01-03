using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Galacticmon
namespace DCGO.CardEffects.EX11
{
    public class EX11_046 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Conditions

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Snatchmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 9, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Galacticmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Shared OP / WD

            string SharedEffectName = "Choose 1 Highest Cost Opponent's Digimon, delete the rest. If 4+ Vemmon, gain Blocker and immunity.";

            string SharedEffectDescription(string tag) => $"[{tag}] Choose 1 of your opponent's highest play cost Digimon and delete all of their other Digimon. Then, if this Digimon has 4 or more [Vemmon] in its digivolution cards, until your opponent's turn ends, it gains <Blocker> and isn't affected by their effects.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            bool CanSelectHighestCostCondition(Permanent permanent) => CardEffectCommons.IsMaxCost(permanent, card.Owner.Enemy, true);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectHighestCostCondition));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectHighestCostCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Opponent's Digimon, you will delete all other digimon.", "Opponent is selecting 1 digimon and will delete all others.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        List<Permanent> targetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(perm => perm != permanent);

                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(targetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                    }
                }

                Permanent thisPermanent = card.PermanentOfThisCard();
                if (thisPermanent != null && thisPermanent.DigivolutionCards.Count(cardSource => cardSource.EqualsCardName("Vemmon")) >= 4)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(targetPermanent: thisPermanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));

                    CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                    canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's effect", CanUseCondition1, card);
                    canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                    thisPermanent.UntilOpponentTurnEndEffects.Add((_timing) => canNotAffectedClass);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(thisPermanent));

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        if (thisPermanent.TopCard != null)
                        {
                            return true;
                        }

                        return false;
                    }

                    bool CardCondition(CardSource cardSource)
                    {
                        if (thisPermanent.TopCard != null)
                        {
                            if (thisPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(thisPermanent))
                            {
                                if (cardSource == thisPermanent.TopCard)
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    bool SkillCondition(ICardEffect cardEffect)
                    {
                        if (cardEffect != null)
                        {
                            if (cardEffect.EffectSourceCard != null)
                            {
                                if (cardEffect.EffectSourceCard.Owner == card.Owner.Enemy)
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }
                }
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition,(hash) => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region End Of Opponent's Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into Galacticmon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription() => "[End of Opponent's Turn] This Digimon may digivolve into [Galacticmon] in the hand or trash, ignoring digivolution requirements and without paying the cost.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOpponentTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition)
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition));
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Galacticmon") 
                        && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass, ignore: CardEffectCommons.IgnoreRequirement.All);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
                            cardCondition: CanSelectCardCondition,
                            payCost: false,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: fromHand,
                            activateClass: activateClass,
                            successProcess: null,
                            ignoreRequirements: CardEffectCommons.IgnoreRequirement.All));
                    }
                }
            }

            #endregion

            #region Assembly
            if (timing == EffectTiming.None)
            {
                AddAssemblyConditionClass addAssemblyConditionClass = new AddAssemblyConditionClass();
                addAssemblyConditionClass.SetUpICardEffect($"Assembly", CanUseCondition, card);
                addAssemblyConditionClass.SetUpAddAssemblyConditionClass(getAssemblyCondition: GetAssembly);
                addAssemblyConditionClass.SetNotShowUI(true);
                cardEffects.Add(addAssemblyConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                AssemblyCondition GetAssembly(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        AssemblyConditionElement element = new AssemblyConditionElement(CanSelectCardCondition);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.HasText("Vemmon");
                        }

                        AssemblyCondition assemblyCondition = new AssemblyCondition(
                            element: element,
                            CanTargetCondition_ByPreSelecetedList: null,
                            selectMessage: "8 cards w/[Vemmon] in text",
                            elementCount: 8,
                            reduceCost: 6);

                        return assemblyCondition;
                    }

                    return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
