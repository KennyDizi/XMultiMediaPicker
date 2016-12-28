using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using multimediachooser.Droid;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using Xamarin.Forms;

[assembly: Dependency(typeof(MultiMediaChooserPickerImplementation))]

namespace multimediachooser.Droid
{
    [Android.Runtime.Preserve(AllMembers = true)]
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

        public async Task<List<ImageSource>> PickMultiImage()
        {
            if (!await RequestStoragePermission())
            {
                return null;
            }

            var result = await ExecutePickMultiImage();
            return result;
        }

        public Task<List<ImageSource>> ExecutePickMultiImage()
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

            var intent = new Intent(Action.ActionPickMultiple);
            //event
            EventHandler<XViewEventArgs> handler = null;
            handler = (s, e) =>
            {
                var tcs = Interlocked.Exchange(ref _completionSource, null);

                CustomGalleryActivity.MediaSelected -= handler;
                var result = e.CastObject as List<ImageSource>;
                tcs.SetResult(result);
            };

            CustomGalleryActivity.MediaSelected += handler;
            CrossCurrentActivity.Current.Activity.StartActivityForResult(intent, 200);

            return _completionSource.Task;
        }

        private static async Task<bool> RequestStoragePermission()
        {
            //We always have permission on anything lower than marshmallow.
            if ((int) Build.VERSION.SdkInt < 23) return true;

            var status =
                await CrossPermissions.Current.CheckPermissionStatusAsync(
                    Plugin.Permissions.Abstractions.Permission.Storage);
            if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                System.Diagnostics.Debug.WriteLine("Does not have storage permission granted, requesting.");
                var results =
                    await CrossPermissions.Current.RequestPermissionsAsync(
                        Plugin.Permissions.Abstractions.Permission.Storage);
                if (results.ContainsKey(Plugin.Permissions.Abstractions.Permission.Storage) &&
                    results[Plugin.Permissions.Abstractions.Permission.Storage] !=
                    Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("Storage permission Denied.");
                    return false;
                }
            }

            return true;
        }
    }
}