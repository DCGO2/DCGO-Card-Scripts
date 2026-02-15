using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when security cards added" effect
    public static bool CanTriggerWhenAddSecurity(Hashtable hashtable, Func<Player, bool> playerCondition)
    {
        return CanTriggerWhenLoseSecurity(hashtable, playerCondition);
    }
    #endregion

    #region Check card was sent to security
    public static bool WasSentToSecurity(CardSource cardSource)
    {
        if (cardSource.Owner.SecurityCards.Contains(cardSource))
            return true; 

        if (cardSource.IsToken)
        {
            if (!CardEffectCommons.IsExistOnBattleArea(cardSource))
            {
                return true;
            }
        }
        if (cardSource.IsDigiEgg)
        {
            if (!CardEffectCommons.IsExistOnBattleArea(cardSource))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Place in security with callbacks for success or failure
    public static IEnumerator PlacePermanentInSecurityAndProcessAccordingToResult(Permanent targetPermanent, ICardEffect activateClass, bool toTop, Func<CardSource, IEnumerator> successProcess, Func<IEnumerator> failureProcess = null, bool isFaceUp = false)
    {
        IPutSecurityPermanent putSecurityPermanent = new IPutSecurityPermanent(targetPermanent, CardEffectCommons.CardEffectHashtable(activateClass), toTop, isFaceUp);

        CardSource topCard = targetPermanent.TopCard;
        if (activateClass.EffectSourceCard != null 
            && activateClass.EffectSourceCard.Owner.CanAddSecurity(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(putSecurityPermanent.PutSecurity());
        }

        if (WasSentToSecurity(topCard))
        {
            if (successProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(successProcess(topCard));
            }
        }
        else
        {
            if (failureProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(failureProcess());
            }
        }
    }

    #endregion
}