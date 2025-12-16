using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Calmaramon
namespace DCGO.CardEffects.BT24
{
    public class BT24_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Lanamon")
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.IsLevel3 && targetPermanent.TopCard.HasTSTraits);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Blocker

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Decode <[Lanamon]>

            if (timing == EffectTiming.WhenRemoveField)
            {
                bool SourceCondition(CardSource source)
                {
                    return source.EqualsCardName("Lanamon");
                }

                string[] decodeStrings = { "(Lanamon)", "Lanamon" };
                cardEffects.Add(CardEffectFactory.DecodeSelfEffect(card: card, isInheritedEffect: false, decodeStrings: decodeStrings, sourceCondition: SourceCondition, condition: null));
            }

            #endregion

            #region Shared OP/WD

            string EffectDiscriptionShared(string tag)
            {
                return $"[{tag}] Return 1 of your opponent's level 4 or lower Digimon to the bottom of the deck. Then, if played by effects, 1 of their Digimon or Tamers can't suspend until their turn ends.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card)
                    && permanent.TopCard.HasLevel && permanent.TopCard.Level <= 4;
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            IEnumerator ActivateCoroutineShared(Hashtable hashtable)
            {
                if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
                {
                    int maxCount1 = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to bottom deck.", "The opponent is selecting 1 Digimon to bottom deck.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                }
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bot deck 1 lvl 4 or lower, possibly prevent suspension", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, hash => ActivateCoroutineShared(hash, activateClass), -1, true, EffectDiscriptionShared("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Bot deck 1 lvl 4 or lower, possibly prevent suspension", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, hash => ActivateCoroutineShared(hash, activateClass), -1, true, EffectDiscriptionShared("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }

            #endregion

            #region ESS Jamming
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.JammingSelfStaticEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }
            #endregion  

            return cardEffects;
        }
    }
}
