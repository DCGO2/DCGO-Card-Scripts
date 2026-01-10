using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX11
{
    //Reina Oumi
    public class EX11_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Shared Card Condition
            bool IsNSo(CardSource cardSource)
            {
                return cardSource.EqualsTraits("NSo");
            }
            #endregion

            #region Shared SOYMP / OP

            string SharedEffectName = "Trash 1 [NSo] card from hand to Draw 1 and gain Memory +1";

            string SharedEffectDescription(string tag)=> $"[{tag}] By trashing 1 [NSo] trait card from your hand, <Draw 1> and gain 1 memory.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card) 
                    && card.Owner.HandCards.Count(IsNSo) >= 1;
            }

            IEnumerator SharedActivateCoroutine(Hashtable _hashtable, ActivateClass activateClass)
            {
                bool discarded = false;

                int discardCount = 1;

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: IsNSo,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: discardCount,
                    canNoSelect: true,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: null,
                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                    mode: SelectHandEffect.Mode.Discard,
                    cardEffect: activateClass);

                yield return StartCoroutine(selectHandEffect.Activate());

                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                {
                    if (cardSources.Count >= 1)
                    {
                        discarded = true;

                        yield return null;
                    }
                }

                if (discarded)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                    
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }
            #endregion

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("Start of Your Turn"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) 
                        && CardEffectCommons.IsOwnerTurn(card);
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName, CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) 
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DNA into [NSo] Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your [NSo] trait Digimon are deleted, by suspending this Tamer, 1 of your [NSo] trait Digimon and 1 [NSo] trait Digimon card in the trash may DNA digivolve into a Digimon card with the [NSo] trait in the hand.";
                }

                bool HasNSoPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) 
                        && IsNSo(permanent.TopCard);
                }

                bool HasDNAPermanent(Permanent permanent, CardSource jogressTarget, Permanent firstCondition)
                {
                    if(HasNSoPermanent(permanent))
                    {
                        if (firstCondition == null)
                        {
                            foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                            {
                                if (DNACondition.elements[0].EvoRootCondition(permanent))
                                {
                                    foreach(CardSource cardSource in card.Owner.TrashCards.Filter(HasNSoCard))
                                    {
                                        Permanent tempPermanent = PlayTempPermanent(cardSource);
                                        bool isValid = DNACondition.elements[1].EvoRootCondition(tempPermanent);
                                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(selectedCardSource));

                                        if(isValid)
                                            return true;

                                    }
                                }
                            }
                        }
                        else
                        {
                            if (permanent == firstCondition)
                            {
                                return false;
                            }
                            foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                            {
                                if (DNACondition.elements[0].EvoRootCondition(firstCondition) && DNACondition.elements[1].EvoRootCondition(permanent))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool HasNSoCard(CardSource source)
                {
                    return source.IsDigimon 
                        && IsNSo(source);
                }

                bool HasDNACardInTrash(CardSource cardSource, CardSource jogressTarget, Permanent firstCondition)
                {
                    bool isValid = false;
                    if(HasNSoCard(cardSource))
                    {
                        Permanent tempPermanent = PlayTempPermanent(cardSource);
                        foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                        {
                            if(isValid)
                                break;
                            if (firstCondition == null)
                            {
                                foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                                {
                                    if (DNACondition.elements[0].EvoRootCondition(tempPermanent))
                                    {
                                        foreach(Permanent permanent in card.Owner.GetBattleAreaDigimons().Filter(HasNSoPermanent))
                                        {
                                            if(DNACondition.elements[1].EvoRootCondition(tempPermanent))
                                            {
                                                isValid = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                                {
                                    if (DNACondition.elements[0].EvoRootCondition(firstCondition) && DNACondition.elements[1].EvoRootCondition(tempPermanent))
                                    {
                                        isValid = true;
                                        break;
                                    }
                                }
                            }
                        }
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(selectedCardSource));
                    }

                    return isValid;
                }

                bool HasNSoJogress(CardSource source)
                {
                    return CardEffectCommons.IsExistOnHand(source) 
                        && IsNSo(source) 
                        && source.jogressCondition.Count > 0
                        && (CardEffectCommons.HasMatchConditionPermanent(permanent => HasDNAPermanent(permanent, source, null)) 
                            || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, cardSource => HasDNACardInTrash(cardSource, source, null)));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, HasNSoPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card) 
                        && CardEffectCommons.CanActivateSuspendCostEffect(card)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, HasNSoJogress);
                }

                Permanent SelectPermanent(CardSource jogressTarget, Permanent firstCondition)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: permanent => HasDNAPermanent(permanent, firstCondition),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting 1 Digimon to DNA digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    return selectedPermanent;
                }

                CardSource SelectTrashCard(CardSource jogressTarget, Permanent firstCondition)
                {
                    CardSource selectedCardSource = null;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                    canTargetCondition: cardSource => HasDNACardInTrash(cardSource, firstCondition),
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => false,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    message: "Select 1 Digimon to DNA digivolve.",
                    maxCount: 1,
                    canEndNotMax: false,
                    isShowOpponent: false,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Custom,
                    customRootCardList: card.Owner.TrashCards.clone(),
                    canLookReverseCard: true,
                    selectPlayer: card.Owner,
                    cardEffect: activateClass);

                    selectCardEffect.SetNotShowCard();
                    selectCardEffect.SetNotAddLog();

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        selectedCardSource = cardSource;

                        yield return null;

                    }

                    return selectedCardSource;
                }

                Permament PlayTempPermanent(CardSource card)
                {
                    Permanent playedPermanent = null;
                    if (card = null)
                    {
                        int frameID = selectedCardSource.PreferredFrame;

                        if (0 <= frameID && frameID < card.Owner.fieldCardFrames.Count)
                        {
                            playedPermanent = new Permanent(new List<CardSource>() { selectedCardSource }) { IsSuspended = false };
                            
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.CreateNewPermanent(playedPermanent, frameID));
                        }
                    }
                    return playedPermanent;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource dnaTarget = null;
                    Permanent selectedPermanent = null;
                    CardSource selectedCardSource = null;
                    Permanent playedPermanent = null;

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: HasNSoJogress,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectDNACardCoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting 1 Digimon to DNA digivolve.");

                    yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                    IEnumerator SelectDNACardCoroutine(CardSource source)
                    {
                        dnaTarget = source;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                        canTargetCondition: HasNSoCard,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 Digimon to DNA digivolve.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetNotAddLog();

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCardSource = cardSource;

                            yield return null;

                        }

                        yield return null;
                    }

                    bool validPermanent = false;
                    bool validTrash = false;

                    if(validPermanent || validTrash)
                    {
                        #region select source cards
                        if (validPermanent && validTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"From Battle Area", value : true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"From Trash", value : false, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "From where will you select the first digimon?";
                            string notSelectPlayerMessage = "The opponent is selecting 1 Digimon to DNA digivolve.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(validPermanent);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool isPermanentFirst = GManager.instance.userSelectionManager.SelectedBoolValue;
                        if (isPermanentFirst)
                        {
                            selectedPermanent = SelectPermanent(dnaTarget, null);

                            if (selectedPermanent != null)
                            {
                                selectedCardSource = SelectTrashCard(dnaTarget, selectedPermanent.TopCard);

                                playedPermanent = PlayTempPermanent(selectedCardSource);
                            }
                        }
                        else
                        {
                            selectedCardSource = SelectTrashCard(dnaTarget, null);

                            if(selectedCardSource != null)
                            {
                                playedPermanent = PlayTempPermanent(selectedCardSource);

                                selectedPermanent = SelectPermanent(dnaTarget, selectedCardSource);
                            }
                        }
                        #endregion

                        if (selectedPermanent != null && selectedCardSource != null)
                        {
                            int[] JogressEvoRootsFrameIDs = { playedPermanent.PermanentFrame.FrameID, selectedPermanent.PermanentFrame.FrameID };

                            if (dnaTarget.CanPlayJogress(true))
                            {
                                PlayCardClass playCard = new PlayCardClass(
                                    cardSources: new List<CardSource>() { dnaTarget },
                                    hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                    payCost: true,
                                    targetPermanent: null,
                                    isTapped: false,
                                    root: SelectCardEffect.Root.Hand,
                                    activateETB: true);

                                playCard.SetJogress(JogressEvoRootsFrameIDs);

                                yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
                            }
                        }
                    }
                    if (dnaTarget == null || dnaTarget.PermanentOfThisCard() == null)
                    {
                        if (playedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(selectedCardSource));
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
