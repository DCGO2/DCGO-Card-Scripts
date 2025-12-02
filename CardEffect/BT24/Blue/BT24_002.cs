using System.Collections;
using System.Collections.Generic;
using System;

// Bukamon
namespace DCGO.CardEffects.BT24
{
    public class BT24_002 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region End of Your Turn - ESS

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Pay 1 to unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Pay1ToUnsuspend_BT24_002");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[End of Your Turn] [Once Per Turn] By paying 1 cost, this blue Digimon with the [TS] train unsuspends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && permanent.TopCard.CardColors.Contains(CardColor.Blue)
                        && CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard())
                        && permanent.TopCard.CardTraits.Contains("TS");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(-1, activateClass));

                    if (card.PermanentOfThisCard().CanActivateCondition()) {
                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent> { card.PermanentOfThisCard() }, activateClass).Unsuspend());
                    }                
                }
            }

            #endregion

            return cardEffects;
        }
    }
}