using System;
using System.Collections;
using System.Collections.Generic;

//ST22 Offensive Plug-in V
namespace DCGO.CardEffects.ST22
{
    public class ST22_08 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            switch (timing)
            {
                case EffectTiming.None:
                    cardEffects.Add(IgnoreColorCondition(card));
                    cardEffects.Add(LinkCondtion(card));
                    break;
                case EffectTiming.OnDeclaration:
                    cardEffects.Add(LinkAction(card));
                    break;
                case EffectTiming.OnEndTurn:
                    cardEffects.Add(EndOfTurnLinkedEffect(card));
                    break;
                case EffectTiming.SecuritySkill:
                    cardEffects.Add(SecurityEffect(card));
                    break;
                case EffectTiming.OptionSkill:
                    cardEffects.Add(MainEffect(card));
                    break;

            }

            return cardEffects;
        }

        IgnoreColorConditionClass IgnoreColorCondition(CardSource card)
        {
            IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
            ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
            ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsTamer);
            }

            bool CardCondition(CardSource cardSource)
            {
                return cardSource == card;
            }

            return ignoreColorConditionClass;
        }

        ActivateClass SecurityEffect(CardSource card)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Delete opponent's lowest dp Digimon and add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);

            string EffectDiscription()
                => "[Security] Delete 1 of your opponent's Digimon with the lowest DP. Then, add this card to the hand.";
           
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            bool CanSelectPermanentCondition(Permanent permanent)
                => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: false,
                    canEndNotMax: false,
                    selectPermanentCoroutine: null,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Destroy,
                    cardEffect: activateClass);

                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
            }

            return activateClass;
        }

        ActivateClass MainEffect(CardSource card)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());

            string EffectDiscription()
                => "[Main] You may link this card to 1 of your Digimon without paying the cost. Then, delete 1 of your opponent's Digimon with as much or less DP as 1 of your Digimon.";

            bool CanUseCondition(Hashtable hashtable)
                => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                #region Select Digimon To Link
                {
                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                                card.CanLinkToTargetPermanent(permanent, false);
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to link.", "The opponent is selecting 1 Digimon to link.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddLinkCard(card, activateClass));
                        }
                    }
                }
                #endregion

                Permanent comparePermanent = null;

                #region Select Digimon to Compare
                {
                    bool opponentHasAnyTarget = CardEffectCommons.HasMatchConditionOwnersPermanent(card, (ownerPermanent) => 
                    {
                        if (!ownerPermanent.IsDigimon)
                        {
                            return false;
                        }

                        return CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (opponentPermanent) =>
                        {
                            return opponentPermanent.IsDigimon && opponentPermanent.DP <= ownerPermanent.DP;
                        });
                    });

                    bool CanSelectPermanentCondition(Permanent permanent)
                            => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);

                    if (opponentHasAnyTarget && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 of your Digimon.", "The opponent is selecting 1 Digimon");

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            comparePermanent = permanent;
                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
                #endregion

                #region Select Digimon To Delete
                {
                    bool CanSelectPermanentCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) && 
                               permanent.DP <= comparePermanent.DP;
                    }

                    bool canDelete = false;
                    if (comparePermanent != null)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            canDelete = true;
                        }
                    }

                    if (canDelete)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
                #endregion
            }

            return activateClass;
        }

        ActivateClass LinkAction(CardSource card)
        {
            return CardEffectFactory.LinkEffect(card);
        }

        AddLinkConditionClass LinkCondtion(CardSource card)
        {
            static bool PermanentCondition(Permanent targetPermanent)
            {
                CardSource Top = targetPermanent.TopCard;
                return Top.IsDigimon && Top.Level >= 3;
            }

            return CardEffectFactory.AddSelfLinkConditionStaticEffect(permanentCondition: PermanentCondition, linkCost: 2, card: card);
        } 

        ActivateClass EndOfTurnLinkedEffect(CardSource card)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("This digimon may attack", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
            activateClass.SetIsLinkedEffect(true);
            activateClass.SetHashString("EOT_ST22_08");

            string EffectDiscription() => "[End of Your Turn] this Digimon may attack.";

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                       card.PermanentOfThisCard().CanAttack(activateClass);
            }

            IEnumerator ActivateCoroutine(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.PermanentOfThisCard().TopCard.PermanentOfThisCard().CanAttack(activateClass))
                    {
                        SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                        selectAttackEffect.SetUp(
                            attacker: card.PermanentOfThisCard().TopCard.PermanentOfThisCard(),
                            canAttackPlayerCondition: () => true,
                            defenderCondition: (permanent) => true,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                    }
                }
            }

            return activateClass;
        }
    }
}