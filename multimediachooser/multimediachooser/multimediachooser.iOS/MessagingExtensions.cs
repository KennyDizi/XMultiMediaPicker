using System;
using UIKit;
using Xamarin.Forms;

#if __UNIFIED__

#else
using MonoTouch.UIKit;
#endif

namespace multimediachooser.iOS
{
    internal static class MessagingExtensions
    {
        #region Methods
        /// <summary>
        /// show view controller via extension
        /// </summary>
        /// <param name="controller"></param>
        public static void PresentUsingRootViewController(this UIViewController controller)
        {
            if (controller == null)
#if DEBUG
                throw new ArgumentNullException(nameof(controller));
#else
                return;
#endif
            var visibleViewController = GetVisibleViewController(null);
            visibleViewController?.PresentViewController(controller, true, null);
        }

        public static void DissmissUsingRootViewController(this UIViewController controller)
        {
            var visibleViewController = GetVisibleViewController(null);
            visibleViewController?.DismissModalViewController(true);
        }

        public static UIViewController GetVisibleViewController(UIViewController controller)
        {
            if (controller == null)
            {
                controller = UIApplication.SharedApplication.KeyWindow.RootViewController;
            }

            if (controller?.NavigationController?.VisibleViewController != null)
            {
                return controller.NavigationController.VisibleViewController;
            }

            if (controller != null && (controller.IsViewLoaded && controller.View?.Window != null))
            {
                return controller;
            }

            if (controller != null)
            {
                foreach (var childViewController in controller.ChildViewControllers)
                {
                    var foundVisibleViewController = GetVisibleViewController(childViewController);
                    if (foundVisibleViewController == null)
                        continue;

                    return foundVisibleViewController;
                }
            }            
            return controller;
        }

        public static ImageSource GetImageSourceFromUIImage(this UIImage uiImage)
        {
            try
            {
                return uiImage == null ? null : ImageSource.FromStream(() => uiImage.AsPNG().AsStream());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        #endregion
    }
}