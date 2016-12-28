using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using multimediachooser.Annotations;
using Xamarin.Forms;

namespace multimediachooser
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public MainPageViewModel()
        {
            PickPhotoCommand = new Command(PickPhotoAction);
        }

        private async void PickPhotoAction()
        {
            var result = await CrossMultiMediaChooserPicker.Current.PickMultiImage();
            if(result.Any())
                ImageSources = new ObservableCollection<ImageSource>(result);
        }

        public ICommand PickPhotoCommand { get; }

        private ObservableCollection<ImageSource> _imageSources;

        public ObservableCollection<ImageSource> ImageSources
        {
            get { return _imageSources ?? (_imageSources = new ObservableCollection<ImageSource>()); }
            set
            {
                _imageSources = value;
                OnPropertyChanged(nameof(ImageSources));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
