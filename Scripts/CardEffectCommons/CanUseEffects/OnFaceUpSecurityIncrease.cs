using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{

    #region Can trigger "When face-up security carsd increases" effect
    public static bool CanTriggerOnFaceUpSecurityIncreases(Hashtable hashtable, Player player = null, Func<CardSource, bool> cardCondition)
    {
        Player _player = GetPlayerFromHashtable;
        
        if (player == null || player.equals(Player))
        {
            List<CardSource> FaceUpCards = GetCardSourcesFromHashtable(hashtable);

            if (FaceUpCards != null)
            {
                if (FaceUpCards.Count(cardSource => cardCondition == null || cardCondition(cardSource)) >= 1)
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion
}