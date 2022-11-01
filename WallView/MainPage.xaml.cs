using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.Storage;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;
using Windows.Storage.Streams;
using Windows.System.UserProfile;

//Szablon elementu Pusta strona jest udokumentowany na stronie https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x415

namespace WallView
{
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        class img
        {
            public ImageSource Thumbnail { get; set; }
            public string path { get; set; }
            public StorageFile sf { get; set; }

        }
        public MainPage()
        {
            this.InitializeComponent();

            // Hide default title bar.
            CoreApplicationViewTitleBar coreTitleBar =
                CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            // Set caption buttons background to transparent.
            ApplicationViewTitleBar titleBar =
                ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;

            // Set XAML element as a drag region.
            Window.Current.SetTitleBar(AppTitleBar);

            // Register a handler for when the size of the overlaid caption control changes.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;

            // Register a handler for when the window activation changes.
            Window.Current.CoreWindow.Activated += CoreWindow_Activated;

        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            CoreApplicationViewTitleBar coreTitleBar =
                CoreApplication.GetCurrentView().TitleBar;
            // Get the size of the caption controls and set padding.
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            
        }

        private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");
            var file = await picker.PickSingleFolderAsync();
            if (file != null)
            {
                panel.Visibility = Visibility.Visible;
                empty.Visibility = Visibility.Collapsed;
                foreach (var selfile in await file.GetFilesAsync())
                {
                    string[] TypeArray = { ".jpg", ".png", ".bmp", ".jpeg" };
                    if (selfile.IsOfType(StorageItemTypes.File) & TypeArray.Any(selfile.FileType.Contains))
                    {
                        using (IRandomAccessStream fileStream = await selfile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                        {
                            BitmapImage bitmapImage = new BitmapImage();
                            await bitmapImage.SetSourceAsync(fileStream);
                            view.Items.Add(new img(){ Thumbnail = bitmapImage, sf = selfile, path = selfile.Path });

                        }

                    }
                }
            }
            else
            {
                await (new ContentDialog() { Title = "There was something wrong with your selection.", PrimaryButtonText = "OK"}).ShowAsync();
            }
        }

        private void view_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (view.SelectedItem == null)
            {
                set.IsEnabled = true;
            }
            else
            {
                set.IsEnabled = false;
            }
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (view.SelectedItem != null)
            {
                Debug.WriteLine(((img)view.SelectedItem).path);
                await ((img)view.SelectedItem).sf.CopyAsync(ApplicationData.Current.LocalFolder, "wp.jpg", NameCollisionOption.ReplaceExisting);
                UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                var success = await profileSettings.TrySetWallpaperImageAsync(await ApplicationData.Current.LocalFolder.GetFileAsync("wp.jpg"));
            }
        }
    }
}
