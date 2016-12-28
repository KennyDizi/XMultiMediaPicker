using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using Com.Nostra13.Universalimageloader.Core;
using Com.Nostra13.Universalimageloader.Core.Listener;

namespace multimediachooser.Droid
{
    public class GalleryAdapter : BaseAdapter<CustomGallery>
    {
        public class ViewHolder : Java.Lang.Object
        {
            public ImageView ImgQueue { get; set; }

            public ImageView ImgQueueMultiSelected { get; set; }
        }

        private class SimpleImageLoadingListenerImpl : SimpleImageLoadingListener
        {
            public SimpleImageLoadingListenerImpl(ViewHolder holder)
            {
                _holder = holder;
            }

            private readonly ViewHolder _holder;

            public override void OnLoadingStarted(string imageUri, View view)
            {
                _holder.
                    ImgQueue.
                    SetImageResource(Resource.Drawable.no_media);

                base.OnLoadingStarted(imageUri, view);
            }
        }

        private readonly LayoutInflater _inflater;
        private readonly List<CustomGallery> _data;
        private readonly ImageLoader _imageLoader;

        public GalleryAdapter(Context c, ImageLoader imageLoader)
        {

            _data = new List<CustomGallery>();
            _inflater = (LayoutInflater) c.GetSystemService(Context.LayoutInflaterService);

            _imageLoader = imageLoader;
            // clearCache();
        }

        public override int Count => _data.Count;

        public override CustomGallery this[int index] => _data[index];

        public override long GetItemId(int position)
        {
            return position;
        }

        public bool IsMultiplePick { get; set; }


        public void SelectAll(bool selection)
        {
            foreach (var t in _data)
            {
                t.IsSelected = selection;
            }
            NotifyDataSetChanged();
        }

        public bool AllSelected
        {
            get { return _data.All(x => x.IsSelected); }
        }

        public bool AnySelected
        {
            get { return _data.Any(x => x.IsSelected); }
        }

        public IEnumerable<CustomGallery> Selected
        {
            get
            {
                return
                    _data.
                        Where(x => x.IsSelected).
                        ToList();
            }
        }

        public void AddAll(IEnumerable<CustomGallery> files)
        {

            try
            {
                _data.Clear();
                _data.AddRange(files);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e.Message);
                throw;
            }

            NotifyDataSetChanged();
        }

        public void ChangeSelection(View v, int position)
        {

            _data[position].IsSelected = !_data[position].IsSelected;

            ((ViewHolder) v.Tag).ImgQueueMultiSelected.Selected = _data[position].IsSelected;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder holder;

            if (convertView == null)
            {

                convertView = _inflater.Inflate(Resource.Layout.gallery_item, null);

                holder = new ViewHolder
                {
                    ImgQueue = (ImageView) convertView.FindViewById(Resource.Id.imgQueue),
                    ImgQueueMultiSelected = (ImageView) convertView.FindViewById(Resource.Id.imgQueueMultiSelected)
                };



                holder.ImgQueueMultiSelected.Visibility = (IsMultiplePick) ? ViewStates.Visible : ViewStates.Gone;

                convertView.Tag = holder;

            }
            else
            {
                holder = (ViewHolder) convertView.Tag;
            }

            holder.ImgQueue.Tag = position;

            try
            {

                _imageLoader.DisplayImage(
                    "file://" + _data[position].SdCardPath,
                    holder.ImgQueue,
                    new SimpleImageLoadingListenerImpl(holder));

                if (IsMultiplePick)
                {

                    holder.ImgQueueMultiSelected.Selected = _data[position].IsSelected;
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write(e.Message);
                throw;
            }

            return convertView;
        }


        public void ClearCache()
        {
            _imageLoader.ClearDiskCache();
            _imageLoader.ClearMemoryCache();
        }

        public void Clear()
        {
            _data.Clear();
            NotifyDataSetChanged();
        }
    }
}
