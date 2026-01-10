using System.Collections;
using System.Collections.Generic;

// Koromon
namespace DCGO.CardEffects.EX11
{
    public class EX11_001 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve this Digimon into [Tyrannnomon] in name or [Dinosaur] trait.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("Digivolve_EX8_073");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[When Attacking] [Once Per Turn] This Digimon may digivolve into a Digimon card with [Tyrannomon] in its name or the [Dinosaur] trait in the hand.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && CardEffectCommons.CanTriggerOnAttack(hashtable, card);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && card.Owner.HandCards.Count >= 1;

                bool CanSelectCardCondition(CardSource cardSource)
                    => (cardSource.EqualsTraits("Dinosaur")
                    || cardSource.ContainsCardName("Tyrannomon"))
                    && cardSource.IsDigimon
                    && cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, true, activateClass, root: SelectCardEffect.Root.Hand);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Exists(CanSelectCardCondition))
                    {
                        List<CardSource> selectedCards = new List<CardSource>();
                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            mode: SelectHandEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectHandEffect.SetUpCustomMessage("Select 1 card to digivolve.", "The opponent is selecting 1 card to digivolve.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Selected card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);
                            yield return null;
                        }

                        if (selectedCards.Count >= 1)
                        {
                            CardSource selectedCard = selectedCards[0];
                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                                    cardSources: new List<CardSource>() { selectedCard },
                                    hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                    payCost: true,
                                    targetPermanent: card.PermanentOfThisCard(),
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true).PlayCard());
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}
