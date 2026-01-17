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
            if (!CardEffectCommons.IsExistOnBattleArea(topCard))
            {
                return true;
            }
        }
        if (cardSource.IsDigiEgg)
        {
            if (!CardEffectCommons.IsExistOnBattleArea(topCard))
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}