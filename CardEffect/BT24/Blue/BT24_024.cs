// Submarimon
namespace DCGO.CardEffects.BT24
{
    public class BT24_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alt Digivolution Condition

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Armadillomon") ||
                        (targetPermanent.TopCard.IsLevel3 && targetPermanent.TopCard.HasTSTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Armor Purge
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card));
            }
            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play one [Ts] Tamer for cost reduced by 2.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT24_024_WA_Play_Tamer");
                cardEffects.Add(activateClass);

                string EffectDescription()
                 => "[When Attacking] [Once Per Turn] You may play 1 Tamer card with the [TS] trait from your hand with the play cost reduced by 2.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) &&
                        CardEffectCommons.HasMatchConditionOwnersHand(CanPlayTamerCondition);
                }

                bool CanPlayTamerCondition(CardSource cardSource)
                {
                    return cardSource.IsTamer() && 
                        cardSource.HasTSTraits() &&
                        CardEffectCommons.CanPlayAsNewPermanent(
                            cardSource: cardSource, 
                            payCost: true, 
                            cardEffect: activateClass, 
                            fixedCost: Math.Max(0,cardSource.BasePlayCostFromEntity - 2)
                        );
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersHand(CanPlayTamerCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanPlayTamerCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: true,
                            isTapped: false,
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true,
                            fixedCost: Math.Max(0,cardSource.BasePlayCostFromEntity - 2)
                        ));
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}