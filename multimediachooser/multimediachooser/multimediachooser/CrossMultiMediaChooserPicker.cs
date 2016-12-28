using System;
using Xamarin.Forms;

namespace multimediachooser
{
    /*how to use
     var result = await CrossMultiMediaChooserPicker.Current.PickMultiImage();

            var a = string.Empty;
         */
    public class CrossMultiMediaChooserPicker
    {
        private static readonly Lazy<IMultiMediaChooserPicker> Implementation = new Lazy<IMultiMediaChooserPicker>(CreateModalView,
            System.Threading.LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Current settings to use
        /// </summary>
        public static IMultiMediaChooserPicker Current
        {
            get
            {
                var ret = Implementation.Value;
                if (ret == null)
                {
                    throw NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        private static IMultiMediaChooserPicker CreateModalView()
        {
#if PORTABLE
            return null;
#else
            return DependencyService.Get<IMultiMediaChooserPicker>();
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return
                new NotImplementedException(
                    "This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}
