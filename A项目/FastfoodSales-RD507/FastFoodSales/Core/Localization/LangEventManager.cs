using System;
using System.Windows;

namespace DAQ.Core.Localization
{
    public class LangEventManager : WeakEventManager
    {
        private static LangEventManager CurrentManager
        {
            get
            {
                Type typeFromHandle = typeof(LangEventManager);
                LangEventManager langEventManager = (LangEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
                if (langEventManager == null)
                {
                    langEventManager = new LangEventManager();
                    WeakEventManager.SetCurrentManager(typeFromHandle, langEventManager);
                }

                return langEventManager;
            }
        }

        public static void AddListener(IWeakEventListener listener)
        {
            CurrentManager.ProtectedAddListener(null, listener);
        }

        public static void RemoveListener(IWeakEventListener listener)
        {
            CurrentManager.ProtectedRemoveListener(null, listener);
        }

        protected sealed override void StartListening(object source)
        {
            AppLocalizationService.LanguageChangeEvent = (EventHandler<EventArgs>)Delegate.Combine(AppLocalizationService.LanguageChangeEvent, new EventHandler<EventArgs>(OnLanguageChanged));
        }

        protected sealed override void StopListening(object source)
        {
            AppLocalizationService.LanguageChangeEvent = (EventHandler<EventArgs>)Delegate.Remove(AppLocalizationService.LanguageChangeEvent, new EventHandler<EventArgs>(OnLanguageChanged));
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            DeliverEvent(null, null);
        }
    }
}
