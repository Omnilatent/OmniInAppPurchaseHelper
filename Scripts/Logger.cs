namespace Omnilatent.InAppPurchase
{
    using System;
    using UnityEngine;

    [Flags]
    public enum DebugModeFlag
    {
        NotSet = 0,
        Release = 1,
        DebugBuild = 2,
        DebugMode = 4, // game is debug mode regardless of build
        DebugModeAndBuild = DebugMode | DebugBuild, // debug build AND debug mode
        Always = DebugBuild | Release
    }

    public interface ILoggerService
    {
        void Log(object message);
        void Log(object message, UnityEngine.Object context);
        void LogWarning(object message);
        void LogWarning(object message, UnityEngine.Object context);
        void LogError(object message);
        void LogError(object message, UnityEngine.Object context);
        void LogException(Exception exception);
        void LogException(Exception exception, UnityEngine.Object context);
        void LogAssertion(object message);
        void LogAssertion(object message, UnityEngine.Object context);
    }

    public class UnityLogger : ILoggerService
    {
        private readonly DebugModeFlag _allowedModes;
        private readonly bool _isDebugBuild;
        private readonly bool _isDebugMode;

        public UnityLogger(DebugModeFlag allowedModes = DebugModeFlag.DebugBuild, bool isDebugMode = false)
        {
            _allowedModes = allowedModes;
            _isDebugBuild = Debug.isDebugBuild;
            _isDebugMode = isDebugMode;
        }

        private bool IsAllowed()
        {
            if ((_allowedModes & DebugModeFlag.DebugBuild) != 0 && _isDebugBuild)
                return true;
            
            if (_allowedModes == DebugModeFlag.Always)
                return true;

            if ((_allowedModes & DebugModeFlag.Release) != 0 && !_isDebugBuild)
                return true;

            if ((_allowedModes & DebugModeFlag.DebugMode) != 0 && _isDebugMode)
                return true;

            if ((_allowedModes & DebugModeFlag.DebugModeAndBuild) != 0 && _isDebugMode && _isDebugBuild)
                return true;

            return false;
        }

        public void Log(object message)
        {
            if (IsAllowed())
                Debug.Log(message);
        }

        public void Log(object message, UnityEngine.Object context)
        {
            if (IsAllowed())
                Debug.Log(message, context);
        }

        public void LogWarning(object message)
        {
            if (IsAllowed())
                Debug.LogWarning(message);
        }

        public void LogWarning(object message, UnityEngine.Object context)
        {
            if (IsAllowed())
                Debug.LogWarning(message, context);
        }

        public void LogError(object message)
        {
            if (IsAllowed())
                Debug.LogError(message);
        }

        public void LogError(object message, UnityEngine.Object context)
        {
            if (IsAllowed())
                Debug.LogError(message, context);
        }

        public void LogException(Exception exception)
        {
            if (IsAllowed())
                Debug.LogException(exception);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            if (IsAllowed())
                Debug.LogException(exception, context);
        }

        public void LogAssertion(object message)
        {
            if (IsAllowed())
                Debug.LogAssertion(message);
        }

        public void LogAssertion(object message, UnityEngine.Object context)
        {
            if (IsAllowed())
                Debug.LogAssertion(message, context);
        }
    }
}