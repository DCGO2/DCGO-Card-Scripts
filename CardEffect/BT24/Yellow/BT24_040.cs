using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Venusmon
namespace DCGO.CardEffects.BT24
{
    public class BT24_040 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel5
                        && targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Reduce Play Cost (-5)

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce the play cost by 5", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("PlayCost-5_BT24_040");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When this card would be played, if you have 3 or fewer security cards, reduce the play cost by 5.";
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card 
                        && CardEffectCommons.IsExistOnHand(cardSource);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnHand(card) 
                        && card.Owner.SecurityCards.Count <= 3;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.CanReduceCost(null, card))
                    {
                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                    }

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Play Cost -5", CanUseCondition1, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        return true;
                    }

                    int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource))
                        {
                            if (RootCondition(root))
                            {
                                if (PermanentsCondition(targetPermanents))
                                {
                                    Cost -= 5;
                                }
                            }
                        }

                        return Cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        if (targetPermanents == null)
                        {
                            return true;
                        }
                        else
                        {
                            if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    bool CardSourceCondition(CardSource cardSource)
                    {
                        return cardSource == card;
                    }

                    bool RootCondition(SelectCardEffect.Root root)
                    {
                        return true;
                    }

                    bool isUpDown()
                    {
                        return true;
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));
                }
            }

            #endregion

            #region Reduce Play Cost (-5) Not Shown

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect($"Reduce Play Cost (-5)", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.PermanentOfThisCard() == null
                        && card.Owner.SecurityCards.Count <= 3;
                }

                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource))
                    {
                        if (RootCondition(root))
                        {
                            if (PermanentsCondition(targetPermanents))
                            {
                                Cost -= 5;
                            }
                        }
                    }

                    return Cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    if (targetPermanents == null)
                    {
                        return true;
                    }

                    else
                    {
                        if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardSourceCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool RootCondition(SelectCardEffect.Root root)
                {
                    return true;
                }

                bool isUpDown()
                {
                    return true;
                }
            }

            #endregion

            #region On Play / When Digivolving Shared

            string SharedEffectName = "Trash 1 Digimons sources. 2 Digimon or Tamers can't Suspend or Activate When Digivolving";

            string SharedEffectDescription(string tag) => $"[{tag}] Trash all digivolution cards of 1 of your opponent's Digimon. Then, until your opponent's turn ends, 2 of their Digimon or Tamers can't suspend or activate [When Digivolving] effects.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            bool CanTrashDigivolutionCardsCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent);
            }

            bool CanFreezeCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permament)
                    && (permanent.TopCard.IsDigimon || permanent.TopCard.IsTamer);
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanTrashDigivolutionCardsCondition));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanTrashDigivolutionCardsCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will trash digivolution cards.", "The opponent is selecting 1 Digimon that will trash digivolution cards.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanent, trashCount: permanent.DigivolutionCards.Count, isFromTop: true, activateClass: activateClass));
                }

                int maxCount1 = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanFreezeCondition));

                SelectPermanentEffect selectPermanentEffect1 = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect1.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanFreezeCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine1,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectPermanentEffect1.SetUpCustomMessage("Select 2 cards that cannot suspend or activate when digivolving.", "The opponent is selecting 2 cards that cannot suspend or activate when digivolving.");

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect1.Activate());

                IEnumerator SelectPermanentCoroutine1(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotSuspendPlayerEffect(
                                permanentCondition: (otherPermanent) => otherPermanent == permanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                isOnlyActivePhase: false,
                                effectName: "Can't Suspend"
                            ));

                        #region Can't Activate When Digivolving
                        DisableEffectClass invalidationClass = new DisableEffectClass();
                        invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseConditionDebuff, card);
                        invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass);

                        bool CanUseConditionDebuff(Hashtable hashtableDebuff)
                        {
                            return true;
                        }

                        bool InvalidateCondition(ICardEffect cardEffect)
                        {
                            return selectedPermanent.TopCard != null
                                && cardEffect != null
                                && cardEffect.EffectSourceCard != null
                                && isExistOnField(cardEffect.EffectSourceCard)
                                && cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent
                                && cardEffect.IsWhenDigivolving
                                && !selectedPermanent.TopCard.CanNotBeAffected(activateClass);
                        }
                        #endregion
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing a sourceless Digimon to Security, your [TS] digimon won't leave the field", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When any of your [TS] trait Digimon would leave the battle area other than by your effects, by placing 1 other Digimon with no digivolution cards as the bottom security card, they don't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition)
                        && !CardEffectCommons.IsOwnerEffect(activateClass, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanPlaceToSecurityCondition);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasTSTraits;
                }

                bool CanPlaceToSecurityCondition(Permanent permanent)
                {
                    return permanent != card.PermanentOfThisCard()
                        && CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                        && permanent.DigivolutionCards.Count == 0;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;
                    int maxCount = Math.min(1, CardEffectCommons.MatchConditionPermanentCount(CanPlaceToSecurityCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanPlaceToSecurityCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to place in security.", "The opponent is selecting 1 Digimon to place in security.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        if (!selectedPermament.TopCard.CanNotBeAffected(activateClass))
                        {
                            #region hashtable
                            Hashtable _hashtable = new Hashtable()
                            {
                                {"CardEffect", activateClass}
                            };
                            #endregion

                            CardSource toSecCard = selectedPermanent.TopCard;

                            yield return ContinuousController.instance.StartCoroutine(new IPutSecurityPermanent(
                                permanent: selectedPermament,
                                hashtable: _hashtable,
                                toTop: false).PutSecurity()
                            );

                            if (card.Owner.SecurityCards.Contains(toSecCard))
                            {
                                List<Permanent> protectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable)
                                    .Filter(PermanentCondition);

                                foreach (Permanent permanent in protectedPermanents)
                                {
                                    permanent.willBeRemoveField = false;
                                    permanent.HideDeleteEffect();
                                    permanent.HideHandBounceEffect();
                                    permanent.HideDeckBounceEffect();
                                    permanent.HideWillRemoveFieldEffect();
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}