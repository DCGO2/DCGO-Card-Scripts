if (card.Owner.GetBattleAreaDigimons().Count(HasDigimonOnOwnerBattleArea) > 1)
{
    int maxCountReceiverDigimon = 1;

    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

    selectPermanentEffect.SetUp(
            selectPlayer: card.Owner,
            canTargetCondition: HasDigimonOnOwnerBattleArea,
            canTargetCondition_ByPreSelecetedList: null,
            canEndSelectCondition: null,
            maxCount: maxCountReceiverDigimon,
            canNoSelect: true,
            canEndNotMax: false,
            selectPermanentCoroutine: SelectPermanentCoroutine,
            afterSelectPermanentCoroutine: null,
            mode: SelectPermanentEffect.Mode.Custom,
            cardEffect: activateClass);

    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get the digivolution card(s).", "The opponent is selecting 1 Digimon that will get the digivolution card(s).");

    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
}

else if (card.Owner.GetBattleAreaDigimons().Count(HasDigimonOnOwnerBattleArea) == 1)
{
    var list = new List<Permanent>();

    Permanent selectedPermanent = list[0];

    yield return ContinuousController.instance.StartCoroutine(SelectPermanentCoroutine(selectedPermanent));
}

IEnumerator SelectPermanentCoroutine(Permanent permanent)
{
    Permanent selectedPermanent = permanent;

    if (selectedPermanent != null)
    {
        yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(selectedCardsFromTrash, activateClass));
    }
}
