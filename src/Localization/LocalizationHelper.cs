using System.Collections.Generic;

namespace QM_SortAllTabs.LocalizationSupport
{
    public static class LocalizationHelper
    {
        public static void AddLocalizationToAllLanguages(string key, string text)
        {
            foreach (var langDict in MGSC.Localization.Instance.db.Values)
            {
                if (langDict.ContainsKey(key))
                    langDict[key] = text;
                else
                    langDict.Add(key, text);
            }
        }

        public static void AddLocalization(string key, string text, MGSC.Localization.Lang language)
        {
            var langDict = MGSC.Localization.Instance.db[language];
            if (langDict.ContainsKey(key))
                langDict[key] = text;
            else
                langDict.Add(key, text);
        }
    }
}
