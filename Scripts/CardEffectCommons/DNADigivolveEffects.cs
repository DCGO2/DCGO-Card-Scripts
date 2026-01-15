public partial class CardEffectCommons
{
    /// <summary>
    /// Creates a temporary permanent and returns the frameID. Calling method can later ensure frame Id is cleared
    /// </summary>
    /// <param name="card">Card to make a permanent of</param>
    /// <param name="finalCard">If true, CardObjectController will be used to more properly create the permanent so it works fully with the jogress</param>
    /// <returns>FrameID of the permanent in card.Owner.FieldPermanents</returns>
    private static int PlayTempPermanentReturnFrame(CardSource card, bool finalCard = false)
    {
        Permanent playedPermanent = null;
        if (card = null)
        {
            int frameID = selectedCardSource.PreferredFrame;

            if (0 <= frameID && frameID < card.Owner.fieldCardFrames.Count)
            {
                playedPermanent = new Permanent(new List<CardSource>() { selectedCardSource }) { IsSuspended = false };

                if (finalCard)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.CreateNewPermanent(playedPermanent, frameID));
                }
                else
                {
                    card.Owner.FieldPermanents[frameID] = playedPermanent;
                }
                return frameID;
            }
        }
        return -1;
    }

    /// <summary>
    /// Check for if a cardSource can meet the jogress requirements of another given card
    /// </summary>
    /// <param name="cardSource">The Card to verify if it is a potential jogress root</param>
    /// <param name="jogressTarget">Card that will be DNA digivolved into</param>
    /// <param name="firstCondition">If the first root has already been found, it is passed here so we only check if this card can fulfill the second root. Otherwise we check if this card can fulfill the first root and some permanent can fulfill the second</param>
    /// <param name="permanentCondition">Condition to filter which permaments may be used for this effect</param>
    /// <param name="cardCondition">Condition to filter which cards may be used for this effect</param>
    /// <returns></returns>
    private static bool CardFulfillsRequirement(CardSource cardSource, CardSource jogressTarget, Permanent firstCondition, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> cardCondition = null)
    {
        if (jogressTarget.jogressCondition.Count > 0)
            return false;
        bool isValid = false;
        if(cardCondition == null || cardCondition(cardSource))
        {
            int frameID = PlayTempPermanentReturnFrame(cardSource);
            if (frameID < 0)
                return false;
            Permanent tempPermanent = card.Owner.FieldPermanents[frameID];
            foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
            {
                if(isValid)
                    break;
                if (firstCondition == null)
                {
                    foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                    {
                        if(isValid)
                            break;
                        if (DNACondition.elements[0].EvoRootCondition(tempPermanent))
                        {
                            foreach(Permanent permanent in card.Owner.GetBattleAreaDigimons().Filter(permanentCondition => permanentCondition == null || permanentCondition(permanent)))
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
            card.Owner.FieldPermanents[frameID] = null;
        }
        return isValid;
    }

    private static CardSource SelectHandCard(CardSource jogressTarget, Permanent firstCondition, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> digivolutionCardCondition = null)
    {
        CardSource selectedCardSource = null;

        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

        selectHandEffect.SetUp(
            selectPlayer: card.Owner,
            canTargetCondition: cardSource => CardFulfillsRequirement(cardSource, jogressTarget, firstCondition, permanentCondition, digivolutionCardCondition),
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            maxCount: 1,
            canNoSelect: () => isOptional,
            canEndNotMax: false,
            isShowOpponent: true,
            selectCardCoroutine: SelectCardCoroutine,
            afterSelectCardCoroutine: null,
            mode: SelectHandEffect.Mode.Custom,
            cardEffect: activateClass);

        selectHandEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting DNA digivolution cards.");
        selectHandEffect.SetNotShowCard();

        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

        IEnumerator SelectCardCoroutine(CardSource cardSource)
        {
            selectedCardSource = cardSource;

            yield return null;

        }

        return selectedCardSource;
    }

    private static CardSource SelectTrashCard(CardSource jogressTarget, Permanent firstCondition, bool isOptional,  Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> digivolutionCardCondition = null)
    {
        CardSource selectedCardSource = null;

        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

        selectCardEffect.SetUp(
        canTargetCondition: cardSource => CardFulfillsRequirement(cardSource, jogressTarget, firstCondition, permanentCondition, digivolutionCardCondition),
        canTargetCondition_ByPreSelecetedList: null,
        canEndSelectCondition: null,
        canNoSelect: () => isOptional,
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

        return selectedCardSource;
    }

    /// <summary>
    /// Check for if a permanent can meet the jogress requirements of another given card. 
    /// </summary>
    /// <param name="permanent">The Permanent to verify if it is a potential jogress root</param>
    /// <param name="jogressTarget">Card that will be DNA digivolved into</param>
    /// <param name="firstCondition">If the first root has already been found, it is passed here so we only check if this permanent can fulfill the second root. Otherwise we check if this permanent can fulfill the first root and some card can fulfill the second</param>
    /// <param name="isWithHand">If the cardsource for the second condition is coming form hand, otherwise it will be taken from trash</param>
    /// <param name="permanentCondition">Condition to filter which permaments may be used for this effect</param>
    /// <param name="cardCondition">Condition to filter which cards may be used for this effect</param>
    /// <returns></returns>
    private static bool PermanentFulfillsRequirement(Permanent permanent, CardSource jogressTarget, Permanent firstCondition, bool isWithHandCard, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> cardCondition = null)
    {
        if (jogressTarget.jogressCondition.Count > 0 || jogressTarget.CanNotEvolve(permanent))
            return false;
        if(permanentCondition == null || permanentCondition(permanent))
        {
            if (firstCondition == null)
            {
                foreach (JogressCondition DNACondition in jogressTarget.jogressCondition)
                {
                    if (DNACondition.elements[0].EvoRootCondition(permanent))
                    {
                        List<CardSource> sources = isWithHandCard ? cardCondition.Owner.HandCards : card.Owner.TrashCards;

                        foreach(CardSource cardSource in sources.Filter(cardSource => cardCondition == null || cardCondition(cardSource)))
                        {
                            int frameID = PlayTempPermanentReturnFrame(cardSource);
                            if (frameID < 0)
                                continue;
                            Permanent tempPermanent = card.Owner.FieldPermanents[frameID];
                            bool isValid = DNACondition.elements[1].EvoRootCondition(tempPermanent);
                            card.Owner.FieldPermanents[frameID] = null;

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

    private static Permanent SelectPermanent(CardSource jogressTarget, Permanent firstCondition, bool isOptional, bool isWithHand, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> digivolutionCardCondition = null)
    {
        Permanent selectedPermanent = null;

        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

        selectPermanentEffect.SetUp(
            selectPlayer: card.Owner,
            canTargetCondition: permanent => PermanentFulfillsRequirement(permanent, jogressTarget, firstCondition, isWithHand, permanentCondition, digivolutionCardCondition),
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            maxCount: 1,
            canNoSelect: isOptional,
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

    public static bool CanJogressWithHandOrTrash(CardSource source, bool isWithHandCard, bool isIntoHandCard, Func<CardSource, bool> targetCardCondition = null, Func<Permanent, bool> permanentCondition = null, Func<CardSource, bool> digivolutionCardCondition = null)
    {
        return (isIntoHandCard ? IsExistOnHand(source) : IsExistOnTrash(source))
            && (targetCardCondition == null || targetCardCondition(source)) 
            && source.jogressCondition.Count > 0
            && (HasMatchConditionPermanent(permanent => PermanentFulfillsRequirement(permanent, source, null, isWithHandCard, permanentCondition, digivolutionCardCondition)) 
                || (isWithHandCard ?
                    HasMatchConditionOwnersHand(card, cardSource => CardFulfillsRequirement(cardSource, source, null, permanentCondition, digivolutionCardCondition)) : 
                    HasMatchConditionOwnersCardInTrash(card, cardSource => CardFulfillsRequirement(cardSource, source, null, permanentCondition, digivolutionCardCondition))));
    }

    /// <summary>
    /// Method that allows the user to DNA digivolve a Permanent on the field with a card in hand or trash as the other DNA root into a card in the hand or trash
    /// </summary>
    /// <param name="targetCardCondition">CardCondition for the digimon that will be DNA Digivolved into</param>
    /// <param name="permanentCondition">PermanentCondition for the permanent which will make one of the roots</param>
    /// <param name="digivolutionCardCondition">CardCondition for the card that will become one of the roots</param>
    /// <param name="payCost">If the pay cost for the digivolution must be payed</param>
    /// <param name="isWithHandCard">If the card that will a root is coming from the Hand, if false it comes from trash</param>
    /// <param name="isIntoHandCard">If the card to be DNA digivolved into is coming from hand, if false it comes from trash</param>
    /// <param name="activateClass">ActivateClass for the effect causing this DNA Digivolution</param>
    /// <param name="successProcess">IEnumerator to run on success</param>
    /// <param name="failedProcess">IEnumerator to run on failure</param>
    /// <param name="isOptional">If this effect is optional. If true, the user may no select at any time</param>
    /// <returns></returns>
    public static IEnumerator DNADigivolveWithHandOrTrashCardIntoHandOrTrash(
        Func<CardSource, bool> targetCardCondition, 
        Func<Permanent, bool> permanentCondition, 
        Func<CardSource, bool> digivolutionCardCondition,
        bool payCost,
        bool isWithHandCard, 
        bool isIntoHandCard,
        ICardEffect activateClass,
        IEnumerator successProcess,
        bool ignoreSelection = false
        IEnumerator failedProcess = null,
        bool isOptional = true)
    {
        CardSource dnaTarget = null;
        Permanent selectedPermanent = null;
        CardSource selectedCardSource = null;
        Permanent playedPermanent = null;

        IEnumerator SelectDNACardCoroutine(CardSource source)
        {
            dnaTarget = source;

            yield return null;
        }

        if (isFromHand)
        {
            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

            selectHandEffect.SetUp(
                selectPlayer: card.Owner,
                canTargetCondition: cardSource => CanJogressWithHandOrTrash(cardSource, isWithHandCard, isIntoHandCard, targetCardCondition, permanentCondition, digivolutionCardCondition),
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: 1,
                canNoSelect: isOptional,
                canEndNotMax: false,
                isShowOpponent: true,
                selectCardCoroutine: SelectDNACardCoroutine,
                afterSelectCardCoroutine: null,
                mode: SelectHandEffect.Mode.Custom,
                cardEffect: activateClass);

            selectHandEffect.SetUpCustomMessage("Select 1 Digimon to DNA digivolve.", "The opponent is selecting 1 Digimon to DNA digivolve.");

            yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
        }
        else if(ignoreSelection)
        {
            dnaTarget = activateClass.EffectSourceCard;
        }
        else
        {
            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            selectCardEffect.SetUp(
            canTargetCondition: cardSource => CanJogressWithHandOrTrash(cardSource, isWithHandCard, isIntoHandCard, targetCardCondition, permanentCondition, digivolutionCardCondition),
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            canNoSelect: () => isOptional,
            selectCardCoroutine: SelectDNACardCoroutine,
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
        }

        if (dnaTarget == null)
            yield break;

        bool validPermanent = HasMatchConditionPermanent(permanent => PermanentFulfillsRequirement(permanent, source, null, isWithHandCard, permanentCondition, digivolutionCardCondition));
        bool validHandOrTrash = isWithHandCard ?
                    HasMatchConditionOwnersHand(card, cardSource => CardFulfillsRequirement(cardSource, source, null, permanentCondition, digivolutionCardCondition)) : 
                    HasMatchConditionOwnersCardInTrash(card, cardSource => CardFulfillsRequirement(cardSource, source, null, permanentCondition, digivolutionCardCondition));

        if(validPermanent || validHandOrTrash)
        {
            #region select source cards
            if (validPermanent && validHandOrTrash)
            {
                List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                {
                    new SelectionElement<bool>(message: $"From Battle Area", value : true, spriteIndex: 0),
                    new SelectionElement<bool>(message: isWithHandCard ? $"From Hand" : $"From Trash", value : false, spriteIndex: 1),
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
                selectedPermanent = SelectPermanent(dnaTarget, null, isOptional, isWithHandCard, permanentCondition, digivolutionCardCondition);

                if (selectedPermanent != null)
                {
                    selectedCardSource = isWithHandCard ? 
                                            SelectHandCard(dnaTarget, selectedPermanent, isOptional, null, permanentCondition, digivolutionCardCondition) : 
                                            SelectTrashCard(dnaTarget, selectedPermanent, isOptional, null, permanentCondition, digivolutionCardCondition);
                    if (selectedCardSource != null)
                    {
                        int frameID = PlayTempPermanentReturnFrame(selectedCardSource, true);
                            if (frameID < 0)
                                yield break;
                            playedPermanent = card.Owner.FieldPermanents[frameID];
                    }
                            
                }
            }
            else
            {
                selectedCardSource = isWithHandCard ? 
                                            SelectHandCard(dnaTarget, null, isOptional, null, permanentCondition, digivolutionCardCondition) : 
                                            SelectTrashCard(dnaTarget, null, isOptional, null, permanentCondition, digivolutionCardCondition);

                if(selectedCardSource != null)
                {
                    int frameID = PlayTempPermanentReturnFrame(selectedCardSource, true);
                    if (frameID < 0)
                        yield break;
                    playedPermanent = card.Owner.FieldPermanents[frameID];

                    selectedPermanent = SelectPermanent(dnaTarget, playedPermanent, isOptional, isWithHand, permanentCondition, digivolutionCardCondition);
                }
            }
            #endregion

            if (selectedPermanent != null && playedPermanent != null)
            {
                List<Permanent> orderedRoots = isPermanentFirst ? new List<Permanent>() { selectedPermanent, playedPermanent } : new List<Permanent>() { playedPermanent, selectedPermanent };
                int[] JogressEvoRootsFrameIDs = orderedRoots.map(permanent => permanent.PermanentFrame.FrameID);

                if (dnaTarget.CanJogressFromTargetPermanents(orderedRoots, paycost))
                {
                    PlayCardClass playCard = new PlayCardClass(
                        cardSources: new List<CardSource>() { dnaTarget },
                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                        payCost: payCost,
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
                if (isWithHandCard)
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCard(selectedCardSource, false));
                else
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(selectedCardSource));
            }
        }
    }
}