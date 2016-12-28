using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using GMImagePicker;
using multimediachooser.iOS;
using Photos;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: Dependency(typeof(MultiMediaChooserPickerImplementation))]

namespace multimediachooser.iOS
{
    [Foundation.Preserve(AllMembers = true)]
    public class MultiMediaChooserPickerImplementation : IMultiMediaChooserPicker
    {
        private int GetRequestId()
        {
            var id = _requestId;
            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;

            return id;
        }

        private int _requestId;
        private TaskCompletionSource<List<ImageSource>> _completionSource;

        public Task<List<ImageSource>> PickMultiImage()
        {
            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<List<ImageSource>>(id);
            if (Interlocked.CompareExchange(ref _completionSource, ntcs, null) != null)
            {
#if DEBUG
                throw new InvalidOperationException("Only one operation can be active at a time");
#else
                return null;
#endif
            }

            //init media picker
            ShowImagePicker();

            return _completionSource.Task;
        }

        private PHAsset[] _preselectedAssets;

        public void ShowImagePicker()
        {
            var picker = new GMImagePickerController
            {
                Title = "Select Media",
                CustomDoneButtonTitle = "Finished",
                CustomCancelButtonTitle = "Cancel",
                ColsInPortrait = 3,
                ColsInLandscape = 5,
                MinimumInteritemSpacing = 2.0f,
                DisplaySelectionInfoToolbar = true,
                AllowsMultipleSelection = true,
                ShowCameraButton = true,
                AutoSelectCameraImages = true,
                ModalPresentationStyle = UIModalPresentationStyle.Popover,
                MediaTypes = new[] { PHAssetMediaType.Image },
                CustomSmartCollections = new[]
                    {
                        PHAssetCollectionSubtype.SmartAlbumUserLibrary,
                        PHAssetCollectionSubtype.AlbumRegular
                    },
                NavigationBarTextColor = Color.White.ToUIColor(),
                NavigationBarBarTintColor = Color.FromRgb(10, 82, 134).ToUIColor(),
                PickerTextColor = Color.White.ToUIColor(),
                ToolbarTextColor = Color.FromRgb(10, 82, 134).ToUIColor(),
                NavigationBarTintColor = Color.White.ToUIColor()
            };

            // You can limit which galleries are available to browse through
            if (_preselectedAssets != null)
            {
                foreach (var asset in _preselectedAssets)
                {
                    picker.SelectedAssets.Add(asset);
                }
            }

            // select image handler
            GMImagePickerController.MultiAssetEventHandler[] handler = {null};
            //cancel handler
            EventHandler[] cancelHandler = {null};

            //define
            handler[0] = (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine("User canceled picking image.");
                var tcs = Interlocked.Exchange(ref _completionSource, null);
                picker.FinishedPickingAssets -= handler[0];
                picker.Canceled -= cancelHandler[0];
                System.Diagnostics.Debug.WriteLine("User finished picking assets. {0} items selected.", args.Assets.Length);
                var imageManager = new PHImageManager();
                _preselectedAssets = args.Assets;
                if (!_preselectedAssets.Any())
                {
                    //no image selected
                    tcs.TrySetResult(null);
                }
                else
                {
                    var imageSources = new List<ImageSource>();
                    foreach (var asset in _preselectedAssets)
                    {
                        imageManager.RequestImageForAsset(asset,
                            new CGSize(asset.PixelWidth, asset.PixelHeight),
                            PHImageContentMode.Default,
                            null,
                            (image, info) => {
                                imageSources.Add(image.GetImageSourceFromUIImage());
                            });
                    }

                    tcs.TrySetResult(imageSources);
                }
            };
            picker.FinishedPickingAssets += handler[0];
            
            cancelHandler[0] = (sender, args) =>
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);
                picker.FinishedPickingAssets -= handler[0];
                picker.Canceled -= cancelHandler[0];
                tcs.TrySetResult(null);
            };
            picker.Canceled += cancelHandler[0];

            //show picker
            picker.PresentUsingRootViewController();
        }
    }
}

/*v1
 public class MultiMediaChooserPickerImplementation : IMultiMediaChooserPicker
    {
        private int GetRequestId()
        {
            var id = _requestId;
            if (_requestId == int.MaxValue)
                _requestId = 0;
            else
                _requestId++;

            return id;
        }

        private int _requestId;
        private TaskCompletionSource<List<ImageSource>> _completionSource;

        public Task<List<ImageSource>> PickMultiImage()
        {
            var id = GetRequestId();

            var ntcs = new TaskCompletionSource<List<ImageSource>>(id);
            if (Interlocked.CompareExchange(ref _completionSource, ntcs, null) != null)
            {
#if DEBUG
                throw new InvalidOperationException("Only one operation can be active at a time");
#else
                return null;
#endif
            }

            //init media picker
            ShowImagePicker();

            return _completionSource.Task;
        }

        private PHAsset[] _preselectedAssets;

        public void ShowImagePicker()
        {
            var picker = new GMImagePickerController
            {
                Title = "Select Media",
                CustomDoneButtonTitle = "Finished",
                CustomCancelButtonTitle = "Cancel",
                ColsInPortrait = 3,
                ColsInLandscape = 5,
                MinimumInteritemSpacing = 2.0f,
                DisplaySelectionInfoToolbar = true,
                AllowsMultipleSelection = true,
                ShowCameraButton = true,
                AutoSelectCameraImages = true,
                ModalPresentationStyle = UIModalPresentationStyle.Popover,
                MediaTypes = new[] { PHAssetMediaType.Image },
                CustomSmartCollections = new[]
                    {
                        PHAssetCollectionSubtype.SmartAlbumUserLibrary,
                        PHAssetCollectionSubtype.AlbumRegular
                    },
                NavigationBarTextColor = Color.White.ToUIColor(),
                NavigationBarBarTintColor = Color.FromRgb(10, 82, 134).ToUIColor(),
                PickerTextColor = Color.White.ToUIColor(),
                ToolbarTextColor = Color.FromRgb(10, 82, 134).ToUIColor(),
                NavigationBarTintColor = Color.White.ToUIColor()
            };

            // You can limit which galleries are available to browse through
            if (_preselectedAssets != null)
            {
                foreach (var asset in _preselectedAssets)
                {
                    picker.SelectedAssets.Add(asset);
                }
            }

            // select image handler
            GMImagePickerController.MultiAssetEventHandler handler = null;
            handler = (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine("User canceled picking image.");
                var tcs = Interlocked.Exchange(ref _completionSource, null);
                picker.FinishedPickingAssets -= handler;
                System.Diagnostics.Debug.WriteLine("User finished picking assets. {0} items selected.", args.Assets.Length);
                var imageManager = new PHImageManager();
                _preselectedAssets = args.Assets;
                if (!_preselectedAssets.Any())
                {
                    //no image selected
                    tcs.TrySetResult(null);
                }
                else
                {
                    var imageSources = new List<ImageSource>();
                    foreach (var asset in _preselectedAssets)
                    {
                        imageManager.RequestImageForAsset(asset,
                            new CGSize(asset.PixelWidth, asset.PixelHeight),
                            PHImageContentMode.Default,
                            null,
                            (image, info) => {
                                imageSources.Add(image.GetImageSourceFromUIImage());
                            });
                    }

                    tcs.TrySetResult(imageSources);
                }
            };
            picker.FinishedPickingAssets += handler;

            //cancel handler
            EventHandler cancelHandler = null;
            cancelHandler = (sender, args) =>
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);
                picker.Canceled -= cancelHandler;
                tcs.TrySetResult(null);
            };
            picker.Canceled += cancelHandler;

            //show picker
            picker.PresentUsingRootViewController();
        }
    }
}
     */
