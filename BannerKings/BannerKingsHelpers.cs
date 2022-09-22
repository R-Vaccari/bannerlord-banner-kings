using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace BannerKings
{
    public static class Util
    {
        public static void PrintGL(string message)
        {
            InformationManager.DisplayMessage(new InformationMessage(message));
        }

        public static void TryCatch(System.Action method)
        {
            try
            {
                method();
            }
            catch (NullReferenceException ex)
            {
                PrintGL(ex.Message);
            }
            finally
            {
                // logging or fixing the null here?
            }
        }

        public static void TryCatch<X>(Action<X> method) where X : class, new()
        {
            try
            {
                method(new X());
            }
            catch (NullReferenceException ex)
            {
                PrintGL(ex.Message);
            }
            finally
            {
                // logging or fixing the null here?
            }
        }

        /*
        public static void TryCatch<X, Y>(Action<X, Y> method) where X : class, new() where Y : class, new()
        {
            try
            {
                method(new X(), new Y());
            }
            catch (NullReferenceException ex)
            {
                PrintGL(ex.Message);
            }
            finally
            {
                // logging or fixing the null here?
            }
        }*/

        public static Type TryCatchReturn(System.Func<Type> method)
        {
            try
            {
                return method();
            }
            catch (NullReferenceException ex)
            {
                PrintGL(ex.Message);
            }
            finally
            {
                // logging or fixing the null here?
            }
            return null;
        }

        public static Exception TryCatchReturn(System.Func<NullReferenceException> method)
        {
            try
            {
                return method();
            }
            catch (NullReferenceException ex)
            {
                PrintGL(ex.Message);
            }
            finally
            {
                // logging or fixing the null here?
            }
            return null;
        }

        public static bool TryCatchReturn(System.Func<bool> method)
        {
            try
            {
                return method();
            }
            catch (NullReferenceException ex)
            {
                PrintGL(ex.Message);
                return false;
            }
            finally
            {
                // logging or fixing the null here?
            }
        }
    }
}
