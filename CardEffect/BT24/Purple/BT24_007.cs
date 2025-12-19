using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tsunomon 
namespace DCGO.CardEffects.BT24
{
    public class BT24_007 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnDiscardHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Level 4 or higher [Demon]/[Titan] Digimon trashed from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("BT24_007_YT");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When level 4 or higher Digimon cards with the [Demon] or [Titan] trait are trashed from your hand, you may play 1 of them with the play cost reduced by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanTriggerOnTrashHand(hashtable, null, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CardCondition(CardSource card)
                {
                    return card.IsDigimon
                        && card.HasLevel && card.Level >= 4
                        && (card.EqualsTraits("Demon") || card.EqualsTraits("Titan"))
                        && CardEffectCommons.CanPlayAsNewPermanent(card, false, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    var trashedCards = CardEffectCommons.GetDiscardedCardsFromHashtable(hashtable)
                        .Filter(CardCondition);

                    if (trashedCards.Any())
                    {
                        CardSource selectedCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                        int maxCount = Math.Min(1, trashedCards.Count);

                        selectCardEffect.SetUp(
                                    canTargetCondition: CardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 digimon to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Custom,
                                    customRootCardList: trashedCards,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 digimon to play.", "The opponent is selecting 1 digimon card to play.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            payCost: true,
                            isTapped: false,
                            root: SelectCardEffect.Root.Trash,
                            activateETB: true,
                            fixedCost: selectedCard.BasePlayCostFromEntity - 2));
                    }

                }
            }

            return cardEffects;
        }
    }
}