using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Com.Nostra13.Universalimageloader.Cache.Disc.Impl;
using Com.Nostra13.Universalimageloader.Cache.Memory.Impl;
using Com.Nostra13.Universalimageloader.Core;
using Com.Nostra13.Universalimageloader.Core.Assist;
using Com.Nostra13.Universalimageloader.Core.Listener;
using Com.Nostra13.Universalimageloader.Utils;
using Xamarin.Forms;
using OS = Android.OS;
using Button = Android.Widget.Button;
using File = Java.IO.File;

namespace multimediachooser.Droid
{
    [IntentFilter(
        new[] {Action.ActionPick, Action.ActionPickMultiple},
        Categories = new[] {Intent.CategoryDefault})]
    [Activity]
    public class CustomGalleryActivity : Activity
    {
        internal static event EventHandler<XViewEventArgs> MediaSelected;
        private GridView _gridGallery;
        private Handler _handler;
        private GalleryAdapter _adapter;

        private ImageView _imgNoMedia;
        private Button _btnGalleryOk;

        private string _action;
        private ImageLoader _imageLoader;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.gallery);

            _action = Intent.Action;
            if (string.IsNullOrEmpty(_action))
            {
                SetResult(Result.Canceled, null);
                Finish();
            }
            InitImageLoader();
            Init();
        }

        private void InitImageLoader()
        {
            try
            {
                var CACHE_DIR = OS.Environment.ExternalStorageDirectory.AbsoluteFile + "/.temp_tmp";
                new File(CACHE_DIR).Mkdirs();

                var cacheDir = StorageUtils.GetOwnCacheDirectory(BaseContext, CACHE_DIR);

                var defaultOptions =
                    new DisplayImageOptions.
                            Builder()
                        .CacheOnDisk(true).
                        ImageScaleType(ImageScaleType.Exactly).
                        BitmapConfig(Bitmap.Config.Rgb565).
                        Build();

                var builder =
                    new ImageLoaderConfiguration.
                            Builder(BaseContext).
                        DefaultDisplayImageOptions(defaultOptions).
                        DiskCache(new UnlimitedDiskCache(cacheDir)).
                        MemoryCache(new WeakMemoryCache());

                var config = builder.Build();
                _imageLoader = ImageLoader.Instance;
                _imageLoader.Init(config);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e.Message);
                // not going to swallow the exception
                throw;
            }
        }

        private void Init()
        {
            _handler = new Handler();

            _gridGallery = (GridView) FindViewById(Resource.Id.gridGallery);
            _gridGallery.FastScrollEnabled = true;

            var listener = new PauseOnScrollListener(_imageLoader, true, true);
            _gridGallery.SetOnScrollListener(listener);

            _adapter = new GalleryAdapter(ApplicationContext, _imageLoader);

            if (string.Compare(_action, Action.ActionPickMultiple, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                FindViewById(Resource.Id.llBottomContainer).Visibility = ViewStates.Visible;
                _adapter.IsMultiplePick = true;

            }
            else if (string.Compare(_action, Action.ActionPick, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                FindViewById(Resource.Id.llBottomContainer).Visibility = ViewStates.Gone;
                _adapter.IsMultiplePick = false;
            }


            _gridGallery.Adapter = _adapter;
            _imgNoMedia = (ImageView) FindViewById(Resource.Id.imgNoMedia);

            _gridGallery.ItemClick += OnItemClicked;

            _btnGalleryOk = (Button) FindViewById(Resource.Id.btnGalleryOk);
            _btnGalleryOk.Click += OnOkClicked;

            Task.Run(() =>
            {
                _handler.Post(() =>
                {
                    _adapter.AddAll(GalleryPhotos);
                    CheckImageStatus();
                });
            });
        }

        private void CheckImageStatus()
        {
            _imgNoMedia.Visibility = _adapter.IsEmpty ? ViewStates.Visible : ViewStates.Gone;
        }

        /// <summary>
        /// Handles the click event for the 'OK' button, rather than using a Listener
        /// </summary>
        private void OnOkClicked(object sender, EventArgs args)
        {
            var allPath =
                _adapter.
                    Selected.
                    Select(x => x.SdCardPath).
                    ToArray();

            //linq
            var listStream = allPath.Select(IOUtil.ReadFileFromPath).Select(file => ImageSource.FromStream(() => new MemoryStream(file))).ToList();
            MediaSelected?.Invoke(this, new XViewEventArgs(nameof(MediaSelected), listStream));
            Finish();
        }

        public override void OnBackPressed()
        {
            MediaSelected?.Invoke(this, new XViewEventArgs(nameof(MediaSelected), null));
            base.OnBackPressed();
        }

        /// <summary>
        /// Handles the click event for a photo on the gallery
        /// </summary>
        private void OnItemClicked(object sender, AdapterView.ItemClickEventArgs args)
        {
            if (_adapter.IsMultiplePick)
            {
                _adapter.ChangeSelection(args.View, args.Position);
            }
            else
            {
                var item = _adapter[args.Position];
                var data = new Intent().PutExtra("single_path", item.SdCardPath);
                SetResult(Result.Ok, data);
                Finish();
            }
        }

        private IEnumerable<CustomGallery> GalleryPhotos
        {
            get
            {
                var galleryList = new List<CustomGallery>();

                try
                {
                    var columns =
                        new[]
                        {
                            MediaStore.Images.ImageColumns.Data,
                            MediaStore.Images.ImageColumns.Id
                        };

                    var orderBy = MediaStore.Images.ImageColumns.Id;

                    var imagecursor = ManagedQuery(
                        MediaStore.Images.Media.ExternalContentUri,
                        columns,
                        null,
                        null,
                        orderBy);

                    if (imagecursor != null && imagecursor.Count > 0)
                    {

                        while (imagecursor.MoveToNext())
                        {
                            var item = new CustomGallery();

                            var dataColumnIndex = imagecursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);

                            item.SdCardPath = imagecursor.GetString(dataColumnIndex);

                            galleryList.Add(item);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Write(e.Message);
                    throw;
                }

                // show newest photo at beginning of the list
                galleryList.Reverse();

                return galleryList;
            }
        }
    }
}