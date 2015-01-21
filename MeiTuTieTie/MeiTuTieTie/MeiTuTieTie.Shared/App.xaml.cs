﻿using Shared.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace MeiTuTieTie
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {

#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;

            //custom code
            CurrentInstance = this;
            this.UnhandledException += App_UnhandledException;
            Shared.Utility.NetworkHelper.Current.StartListening();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MeiTuTieTie.Pages.HomePage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
            CollectDeviceInformation();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

#if WINDOWS_PHONE_APP
            if (args is FileOpenPickerContinuationEventArgs)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                var page = rootFrame.Content as MeiTuTieTie.Pages.OperationPage;
                if (page == null)
                {
                    if (!rootFrame.Navigate(typeof(MeiTuTieTie.Pages.OperationPage)))
                    {
                        throw new Exception("Failed to create OperationPage");
                    }
                    page = rootFrame.Content as MeiTuTieTie.Pages.OperationPage;
                }
                
                page.PickPhotosContiue((FileOpenPickerContinuationEventArgs)args);

                //ensure the current window is active
                Window.Current.Activate();
            }
#endif

        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }


        /************************ custom code from here on ************************/

        public static double SCREEN_RESOLUTION_WIDTH = 0;
        public static double SCREEN_RESOLUTION_HEIGHT = 0;

        public static App CurrentInstance
        {
            get;
            private set;
        }

        private Frame rootFrame;

        public Frame RootFrame
        {
            get { return rootFrame; }
            set { rootFrame = value; }
        }

        public Material SelectedMaterial { get; set; }

        private void CollectDeviceInformation()
        {
            DisplayInformation di = DisplayInformation.GetForCurrentView();
#if WINDOWS_PHONE_APP
            SCREEN_RESOLUTION_WIDTH = (int)(Math.Round(di.RawPixelsPerViewPixel * Window.Current.Bounds.Width));
            SCREEN_RESOLUTION_HEIGHT = (int)(Math.Round(di.RawPixelsPerViewPixel * Window.Current.Bounds.Height));
#else
            SCREEN_RESOLUTION_WIDTH = (int)di.ResolutionScale * 0.01 * Window.Current.Bounds.Width;
            SCREEN_RESOLUTION_HEIGHT = (int)di.ResolutionScale * 0.01 * Window.Current.Bounds.Height;
#endif
        }

        public async void RunAsync(Action action)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (action != null)
                {
                    action();
                }
            });
        }

        void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}