using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace multimediachooser
{
    public interface IMultiMediaChooserPicker
    {
        Task<List<ImageSource>> PickMultiImage();
    }

    public class FileData
    {
        public ImageSource Source { get; set; }
        public string FileName { get; set; }
    }
}