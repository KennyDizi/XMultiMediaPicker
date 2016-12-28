using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using Xamarin.Forms;

namespace multimediachooser
{
    public class MainPageViewModel : ReactiveObject
    {
        public MainPageViewModel()
        {
            PickPhotoCommand = new Command(PickPhotoAction);
        }

        private async void PickPhotoAction()
        {
            var result = await CrossMultiMediaChooserPicker.Current.PickMultiImage();
            if(result == null) return;
            if(result.Any())
                ImageSources = new ObservableCollection<ImageSource>(result);
        }

        public ICommand PickPhotoCommand { get; }

        private ObservableCollection<ImageSource> _imageSources;

        public ObservableCollection<ImageSource> ImageSources
        {
            get { return _imageSources ?? (_imageSources = new ObservableCollection<ImageSource>()); }
            set { this.RaiseAndSetIfChanged(ref _imageSources, value); }
        }
    }
}
