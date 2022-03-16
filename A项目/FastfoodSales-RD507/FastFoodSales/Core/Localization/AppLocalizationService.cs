using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Xml;

namespace DAQ.Core.Localization
{
    public static class AppLocalizationService
    {
        private static CultureInfo curLanguageDefine;

        public static EventHandler<EventArgs> LanguageChangeEvent;

        public static CultureInfo CurLanguageDefine
        {
            get
            {
                return curLanguageDefine;
            }
            set
            {
                if (curLanguageDefine.LCID != value.LCID)
                {
                    curLanguageDefine = value;
                    GetResource(curLanguageDefine);
                }
            }
        }

        public static ResourceDictionary LangResourceDictionary
        {
            get;
            private set;
        }

        static AppLocalizationService()
        {
            curLanguageDefine = new CultureInfo("zh-cn");
            GetResource(CurLanguageDefine);
        }

        public static ResourceDictionary GetResource(CultureInfo cultureInfo)
        {
            try
            {
                Uri source = new Uri("pack://SiteOfOrigin:,,,/Lang/" + cultureInfo.Name + ".xaml", UriKind.RelativeOrAbsolute);
                LangResourceDictionary = new ResourceDictionary
                {
                    Source = source
                };
                return LangResourceDictionary;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string UnDoTranslate(string info)
        {
            return "%" + info + "%";
        }

        public static string GetLang(string InfoKey)
        {
            try
            {
                if (string.IsNullOrEmpty(InfoKey))
                {
                    return null;
                }

                if (InfoKey.First() == '%' && InfoKey.Last() == '%' && InfoKey.Length > 1)
                {
                    return InfoKey.Substring(1, InfoKey.Length - 2);
                }

                if (LangResourceDictionary == null)
                {
                    return InfoKey;
                }

                return ((string)LangResourceDictionary[InfoKey]) ?? InfoKey;
            }
            catch
            {
                return InfoKey;
            }
        }

        public static string GetLangSet(string inputString, char separator)
        {
            return string.Join(separator.ToString() ?? "", from p in inputString.Split(separator)
                                                           select GetLang(p));
        }

        public static void ChangeLanguage(string language)
        {
            if (LanguageChangeEvent != null)
            {
                LanguageChangeEvent(null, null);
            }
        }

        public static void LangSetSave(string Langtype)
        {
            XmlDocument xmlDocument = new XmlDocument();
            string filename = AppDomain.CurrentDomain.BaseDirectory + "LangCFG\\LanguageSet.cfg";
            xmlDocument.Load(filename);
            xmlDocument.SelectSingleNode("VisionMasterCfgRoot/LangSet").InnerText = Langtype;
            xmlDocument.Save(filename);
        }

        public static string GetLangParm()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("LangCFG/LanguageSet.cfg");
            return xmlDocument.SelectSingleNode("VisionMasterCfgRoot/LangSet").InnerText;
        }
    }
}
