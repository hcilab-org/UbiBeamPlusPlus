using UIKinect.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIKinect
{
    class ApplicationCore
    {
        /// <summary>
        ///  This is the main class which specifies the entry point of the application.
        ///  As this project is a WPF application - this has to be specified in the settings.
        /// </summary>
        /// <author>Markus Funk</author>
        /// <date>09.25.2013</date>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main()
        {
            try
            {
                BackendControl.Instance.startUpApplication();
            }
            catch (Exception e)
            {
            //    Logger.Instance.Log(e.Message, Logger.LoggerState.CRITICAL);
            }
        }
    }
}
