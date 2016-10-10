using Emgu.CV;
using Emgu.CV.Structure;
using HciLab.Utilities;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace UIKinect.Core
{
    class BackendControl
    {
        private static BackendControl instance;
        public MainWindow guihandle = null;
        public ProjectorWindow projectorhandle = null;

        public static BackendControl Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BackendControl();
                }
                return instance;
            }
        }

        public void startUpApplication()
        {

            guihandle = new MainWindow();
            guihandle.InitializeComponent();
            guihandle.Show();

            projectorhandle = new ProjectorWindow();
            projectorhandle.InitializeComponent();
            projectorhandle.Show();
            // open the frontend GUI
            App app = new App();
            app.Run();
        }
    }
}
