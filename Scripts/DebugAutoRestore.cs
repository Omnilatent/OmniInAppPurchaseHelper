using UnityEngine;

namespace Omnilatent.InAppPurchase
{
    /// <summary>
    /// Runtime switch to stop launch-time purchase restoring, so a tester can play the game
    /// without the payouts of their previously owned products being granted again.
    /// Only has an effect in debug builds; release builds always follow the inspector setting.
    /// </summary>
    public class DebugAutoRestore
    {
        public const string ppDisableAutoRestore = "disableAutoRestorePurchase";

        /// <summary>
        /// False when a tester has disabled launch-time restoring on this device.
        /// </summary>
        public static bool IsAutoRestoreAllowed()
        {
            if (!Debug.isDebugBuild) { return true; }

            return PlayerPrefs.GetInt(ppDisableAutoRestore, 0) == 0;
        }

        /// <summary>
        /// Disable or re-enable launch-time purchase restoring on this device. Takes effect on next app open.
        /// Does not touch already granted payouts.
        /// </summary>
        public static void SetDisableAutoRestore(bool value)
        {
            PlayerPrefs.SetInt(ppDisableAutoRestore, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
