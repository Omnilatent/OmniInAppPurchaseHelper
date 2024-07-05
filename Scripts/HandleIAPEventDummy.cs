using UnityEngine;

namespace Omnilatent.InAppPurchase
{
    public class HandleIAPEventDummy : HandleIAPEventBase
    {
        protected override void OnToggleLoading(bool isLoading) { }

        protected override void ShowErrorPopup(PurchaseResultArgs resultArgs) { }

        protected override void ToggleShowAdOnResume(bool value) { }
    }
}