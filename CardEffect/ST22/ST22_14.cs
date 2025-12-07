using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//DoGatchamon
namespace DCGO.CardEffects.ST22
{
    public class ST22_14 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Static effects

                #region Alternate Digivolution Cost

                if (timing == EffectTiming.None)
                {
                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        return targetPermanent.TopCard.IsLevel5 && (targetPermanent.TopCard.HasFallenAngelTraits || targetPermanent.TopCard.HasCSTraits);
                    }

                    cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                        permanentCondition: PermanentCondition,
                        digivolutionCost: 3,
                        ignoreDigivolutionRequirement: false,
                        card: card,
                        condition: null)
                    );
                }

                #endregion

                #region Play Cost Reduction

                if (timing == EffectTiming.BeforePayCost)
                {
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect("Reduce the Play Cost by 5", CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                    activateClass.SetHashString("PlayCost-4_P_174");
                    cardEffects.Add(activateClass);

                    string EffectDiscription()
                    {
                        return "When this card would be played, if [Nightmare Soldiers] is in your face up security cards, reduce the play cost by 4.";
                    }

                    
                    bool OpponentHandTrashCondition()
                    {
                        return card.Owner.Enemy.TrashCards.Count >= 10 || card.Owner.Enemy.HandCards.Count >= 10;
                    }

                    bool CardCondition(CardSource cardSource)
                    {
                        return (cardSource == card)
                            && CardEffectCommons.IsExistOnHand(cardSource);
                    }

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                    }

                    bool CanActivateCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnHand(card)
                            && OpponentHandTrashCondition();
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
                                        int targetCost = 0;

                                        if (OpponentHandTrashCondition())
                                            targetCost += 5;

                                        Cost -= targetCost;
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

            #endregion

            #region Shared (On Play/When Digivolving)

            // TODO

            #endregion

            #region On Play

            // TODO

            #endregion

            #region When Digivolving

            // TODO

            #endregion

            #region End of Your Turn

            // TODO

            #endregion



            return cardEffects;
        }
    }
}