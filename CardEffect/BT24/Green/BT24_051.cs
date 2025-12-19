using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Merukimon
namespace DCGO.CardEffects.BT24
{
    public class BT24_051 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static Effects

            #region Digivolution Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5
                        && targetPermanent.TopCard.EqualsTraits("Beastkin") || targetPermanent.TopCard.EqualsTraits("TS");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Reduce Play Cost

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce play cost (5)", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When this card would be played, if there are 3 or more Digimon, reduce the play cost by 5.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, cardSource => cardSource == card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return card.Owner.GetBattleAreaDigimons().Count + card.Owner.Enemy.GetBattleAreaDigimons().Count >= 3;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.CanReduceCost(null, card))
                    {
                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                    }

                    ChangeCostClass changeCostClass = new ChangeCostClass();
                    changeCostClass.SetUpICardEffect("Play Cost -5", hashtable => true, card);
                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                    card.Owner.UntilCalculateFixedCostEffect.Add(_ => changeCostClass);

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(hashtable));

                    int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                        List<Permanent> targetPermanents)
                    {
                        if (CardSourceCondition(cardSource) &&
                            RootCondition(root) &&
                            PermanentsCondition(targetPermanents))
                        {
                            cost -= 5;
                        }

                        return cost;
                    }

                    bool PermanentsCondition(List<Permanent> targetPermanents)
                    {
                        return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
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
            }

            #endregion

            #region Reduce Play Cost - Not Shown

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -5", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.Owner.GetBattleAreaDigimons().Count + card.Owner.Enemy.GetBattleAreaDigimons().Count >= 3;
                }

                int ChangeCost(CardSource cardSource, int cost, SelectCardEffect.Root root,
                        List<Permanent> targetPermanents)
                {
                    if (CardSourceCondition(cardSource) &&
                        RootCondition(root) &&
                        PermanentsCondition(targetPermanents))
                    {
                        cost -= 5;
                    }

                    return cost;
                }

                bool PermanentsCondition(List<Permanent> targetPermanents)
                {
                    return targetPermanents == null || targetPermanents.Count(targetPermanent => targetPermanent != null) == 0;
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

            #endregion

            #region OP/WD Shared

            string SharedEffectName()
                => "Suspend 2, then 1 Digimon may gain 5k DP and attack.";

            string SharedEffectDescription(string tag)
            {
                return $"[{tag}] Suspend 2 of your opponent's Digimon or Tamers. Then, 1 of your Digimon may get +5000 DP for the turn and attack your opponent's Digimon.";
            }

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            bool CanSelectOpponentPermanentCondition(CardSource cardSource)
            {
                return (cardSource.IsDigimon
                    || cardSource.IsTamer);
            }

            bool CanSelectOwnerPermanentCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon
                    && ;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                {
                    int maxCount = Math.Min(2,
                        CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOpponentPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }

                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnerPermanentCondition))
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectOwnerPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that may get +5000 DP and attack.",
                        "The opponent is selecting 1 Digimon that may get +500 DP and attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null && selectedPermanent.CanAttack(activateClass))
                    {
                        // Give +5000 DP until end of turn
                        ContinuousController.instance.StartCoroutine(
                            new IChangePermanentDP(
                                targetPermanents: new List<Permanent>() { selectedPermanent },
                                dpChange: 5000,
                                durationType: IChangePermanentDP.DurationType.UntilEndOfTurn,
                                activateClass: activateClass).ChangeDP());

                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: selectedPermanent,
                            canAttackPlayerCondition: () => false,
                            defenderCondition: _ => true,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, SharedActivateCoroutine, -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving 1

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, SharedActivateCoroutine, -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region WD/WA Shared

            #endregion

            #region When Digivolving 2

            #endregion

            #region When Attacking

            #endregion

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                bool CanUseCondition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.EqualsTraits("Iliad");
                }

                cardEffects.Add(CardEffectFactory.RushStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition));

                cardEffects.Add(CardEffectFactory.PierceStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition));
            }

            #endregion

            return cardEffects;
        }
    }
}
