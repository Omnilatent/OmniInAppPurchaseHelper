using UnityEngine;

namespace Omnilatent.InAppPurchase
{
    public class HandleIAPEventDummy : HandleIAPEventBase
    {
        protected override void OnToggleLoading(bool isLoading) { }

        protected override void ShowErrorPopup(PurchaseResultArgs resultArgs) { }

        protected override void ToggleShowAdOnResume(bool value) { }

        protected override void OnPurchaseRestored(bool success, string errorMessage)
        {
            var restoreMessage = string.Empty;
            if (success)
            {
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.
                restoreMessage = "Restore Successful";
            }
            else
            {
                // Restoration failed.
                restoreMessage = $"Restore Failed. Error: {errorMessage}";
            }
            Debug.Log(restoreMessage);
        }
    }
}