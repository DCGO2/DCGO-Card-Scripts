public partial class CardEffectCommons
{
    public static IEnumerator DNADigivolvePermanentsIntoHandOrTrashCard(
        Func<CardSource, bool> canSelectDNACardCondition,
        bool payCost,
        bool isHand,
        ICardEffect activateClass,
        Func<Permanent, bool>[] permanentConditions = null,
        IEnumerator successProcess = null,
        bool ignoreSelection = false,
        IEnumerator failedProcess = null,
        bool isOptional = true)
    {
        Player owner = activateClass.EffectSourceCard.Owner;
        CardSource dnaTarget = null;

        IEnumerator SelectCardCoroutine(CardSource cardSource)
        {
            dnaTarget = cardSource;

            yield return null;
        }

        bool CanJogressCondition(CardSource cardSource)
        {
            return (canSelectDNACardCondition == null || canSelectDNACardCondition(cardSource))
                && cardSource.CanPlayJogress(true);
        }
        
        int maxCount = 1;

        if (isHand && owner.HandCards.Some(canSelectDNACardCondition))
        {
            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

            selectHandEffect.SetUp(
                selectPlayer: owner,
                canTargetCondition: canSelectDNACardCondition,
                canTargetCondition_ByPreSelecetedList: null,
                canEndSelectCondition: null,
                maxCount: maxCount,
                canNoSelect: isOptional,
                canEndNotMax: false,
                isShowOpponent: true,
                selectCardCoroutine: SelectCardCoroutine,
                afterSelectCardCoroutine: null,
                mode: SelectHandEffect.Mode.Custom,
                cardEffect: activateClass);

            selectHandEffect.SetUpCustomMessage("Select 1 card to DNA digivolve.", "The opponent is selecting 1 card to DNA digivolve.");
            selectHandEffect.SetNotShowCard();

            yield return StartCoroutine(selectHandEffect.Activate());
        } 
        else if (ignoreSelection)
        {
            dnaTarget == activateClass.EffectSourceCard;
        }
        else if(owner.TrashCards.some(canSelectDNACardCondition))
        {
            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                    canTargetCondition: canSelectDNACardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    canNoSelect: () => isOptional,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    message: "Select 1 card to digivolve.",
                    maxCount: maxCount,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    mode: SelectCardEffect.Mode.Custom,
                    root: SelectCardEffect.Root.Trash,
                    customRootCardList: null,
                    canLookReverseCard: true,
                    selectPlayer: owner,
                    cardEffect: activateClass);

            selectCardEffect.SetUpCustomMessage("Select 1 card to DNA digivolve.", "The opponent is selecting 1 card to DNA digivolve.");
            selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
        }
        
        if (dnaTarget != null)
        {
            _jogressEvoRootsFrameIDs = new int[0];

            yield return GManager.instance.photonWaitController.StartWait("DNA_Digivolve_by_Effect");

            if (owner.isYou || GManager.instance.IsAI)
            {
                GManager.instance.selectJogressEffect.SetUp_SelectDigivolutionRoots
                                            (card: selectedCard,
                                            isLocal: true,
                                            isPayCost: true,
                                            canNoSelect: true,
                                            endSelectCoroutine_SelectDigivolutionRoots: EndSelectCoroutine_SelectDigivolutionRoots,
                                            noSelectCoroutine: null);

                if(permanentConditions != null)
                    GManager.instance.selectJogressEffect.SetUpCustomPermanentConditions(permanentConditions);

                yield return ContinuousController.instance.StartCoroutine(GManager.instance.selectJogressEffect.SelectDigivolutionRoots());

                IEnumerator EndSelectCoroutine_SelectDigivolutionRoots(List<Permanent> permanents)
                {
                    if (permanents.Count == 2)
                    {
                        _jogressEvoRootsFrameIDs = permanents.Distinct().ToArray().Map(permanent => permanent.PermanentFrame.FrameID);
                    }

                    yield return null;
                }

                photonView.RPC("SetJogressEvoRootsFrameIDs", RpcTarget.All, _jogressEvoRootsFrameIDs);
            }
            else
            {
                GManager.instance.commandText.OpenCommandText("The opponent is choosing a card to DNA digivolve.");
            }

            yield return new WaitWhile(() => !_endSelect);
            _endSelect = false;

            GManager.instance.commandText.CloseCommandText();
            yield return new WaitWhile(() => GManager.instance.commandText.gameObject.activeSelf);

            if (_jogressEvoRootsFrameIDs.Length == 2)
            {
                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { dnaTarget }, "Played Card", true, true));

                PlayCardClass playCard = new PlayCardClass(
                    cardSources: new List<CardSource>() { dnaTarget },
                    hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                    payCost: true,
                    targetPermanent: null,
                    isTapped: false,
                    root: isHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash,
                    activateETB: true);

                playCard.SetJogress(_jogressEvoRootsFrameIDs);

                yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
            }
        }
    }
}