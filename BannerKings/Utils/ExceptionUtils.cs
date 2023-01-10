using System;
using System.IO;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Utils
{
    internal static class ExceptionUtils
    {
        private static readonly string path = BasePath.Name + "Modules/BannerKings/";
        private static readonly string fileName = "errorlog.txt";

        public static void TryCatch(Action method, string className, bool notifty = true)
        {
            try
            {
                method();
            }
            catch (NullReferenceException ex)
            {
                var exception = new BannerKingsException(new TextObject("{=LZTFr8jR}Exception in {CLASS} class.")
                    .SetTextVariable("CLASS", className)
                    .ToString(),
                    ex);
                File.AppendAllText(path + fileName, Environment.NewLine + "Version: " + BannerKingsConfig.Instance.VersionName + 
                    Environment.NewLine + exception.Message + 
                    Environment.NewLine + ex.StackTrace);
                if (notifty)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=3dMr1P4W}A Banner Kings error was detected. Send the contents of {FILE} from BannerKings module to support channel.")
                    .SetTextVariable("FILE", fileName)
                    .ToString()));
                }
            }
        }
    }
}
