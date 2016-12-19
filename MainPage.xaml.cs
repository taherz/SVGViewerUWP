using Microsoft.Advertising.Shared.WinRT;
using Microsoft.Advertising.WinRT.UI;
using Mntone.SvgForXaml;
using Mntone.SvgForXaml.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SVGViewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Count.Text = "0";
        }

        ContentDialog MessageBox = new ContentDialog();
        //ad


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

           //ad

            if (e.Parameter as StorageFile != null)
                LunchSvgWithSVGViewer(e.Parameter as StorageFile);
        }

        async void LunchSvgWithSVGViewer(StorageFile file)
        {
            try
            {
                try
                {
                    if (file.Path.EndsWith(".svg"))
                    {
                        SvgFileModel svg = new SvgFileModel();
                        svg.SvgFile = file;
                        var fileAsBytes = await FileToByteArray(file);
                        svg.Svg = BytesToSvgDoc(fileAsBytes);
                        svg.TransparentBgVisibility = Visibility.Collapsed;
                        svg.Size = double.Parse(string.Format("{0:N}", (fileAsBytes.Length / 1024.00)));
                        SvgList.Add(svg);
                    }
                    //else
                    //    await ShowOkMessageAsync("error", "dropped item is not Svg!!");
                }
                catch (Exception ex)
                {
                    //await ShowOkMessageAsync("error", "dropped item is recognized!!");
                }

                if (SvgList.Count > 0)
                {
                    svgGrid.ItemsSource = SvgList;
                    Count.Text = SvgList.Count.ToString();
                    DragAndDrop.Visibility = Visibility.Collapsed;
                    await Task.Delay(100);
                    await ShowSelectedSVG(SvgList[0]);
                }
                else
                {
                    await ShowOkMessageAsync("error", "the file is not Svg!!");
                    DragAndDrop.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                await ShowOkMessageAsync("error", ex.Message);
            }
            finally
            {
                Preogress.Visibility = Visibility.Collapsed;
            }
        }

        async Task ShowSelectedSVG(SvgFileModel svgModel)
        {
            SelectedSvgGrid.Visibility = Visibility.Visible;
            controlPanel.Visibility = Visibility.Collapsed;
            svgGrid.Visibility = Visibility.Collapsed;
            LoadFolder.Visibility = Visibility.Collapsed;
            LoadFiles.Visibility = Visibility.Collapsed;
            noBg.Visibility = Visibility.Collapsed;

            SelectedSvg.Visibility = Visibility.Visible;
            SelectedSvg.Visibility = Visibility.Collapsed;
            await Task.Delay(100);
            SelectedSvg.Content = svgModel.Svg;
            SelectedSvg.Visibility = Visibility.Visible;

            FileName.Text = svgModel.SvgFile.DisplayName;
            FileSize.Text = svgModel.Size.ToString();
            backButton.Visibility = Visibility.Visible;
        }

        void HideSelectedSVG()
        {
            SelectedSvgGrid.Background = new SolidColorBrush(Colors.White);
            backButton.Visibility = Visibility.Collapsed;
            SelectedSvg.Visibility = Visibility.Collapsed;
            controlPanel.Visibility = Visibility.Visible;
            svgGrid.Visibility = Visibility.Visible;
            FileName.Text = "";
            FileSize.Text = "";
            LoadFolder.Visibility = Visibility.Visible;
            LoadFiles.Visibility = Visibility.Visible;
            noBg.Visibility = Visibility.Visible;
        }

       //ad

        List<SvgFileModel> SvgList = new List<SvgFileModel>();

        private async Task LoadSvgFromPicker(bool isLoadFile)
        {
            Count.Text = "0";
            DragAndDrop.Visibility = Visibility.Collapsed;
            Preogress.Visibility = Visibility.Visible;
            StorageFolder folder = null;
            IReadOnlyList<StorageFile> files = null;

            FolderPicker folderPicker = new FolderPicker() { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
            folderPicker.FileTypeFilter.Add(".svg");

            FileOpenPicker filePicker = new FileOpenPicker() { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
            filePicker.FileTypeFilter.Add(".svg");

            SvgList = new List<SvgFileModel>();

            try
            {
                try
                {
                    if (isLoadFile)
                    {
                        var pickedFile = await filePicker.PickSingleFileAsync();

                        if (pickedFile.Path.EndsWith(".svg"))
                        {
                            SvgFileModel svg = new SvgFileModel();
                            svg.SvgFile = pickedFile;
                            var fileAsBytes = await FileToByteArray(pickedFile);
                            svg.Svg = BytesToSvgDoc(fileAsBytes);
                            svg.TransparentBgVisibility = Visibility.Collapsed;
                            svg.Size = double.Parse(string.Format("{0:N}", (fileAsBytes.Length / 1024.00)));
                            SvgList.Add(svg);

                            if (SvgList.Count > 0)
                            {
                                svgGrid.ItemsSource = SvgList;
                            }
                            else
                            {
                                DragAndDrop.Visibility = Visibility.Visible;
                            }

                        }

                        Preogress.Visibility = Visibility.Collapsed;

                        //ad

                        return;
                    }

                    folder = await folderPicker.PickSingleFolderAsync();
                    files = await folder.GetFilesAsync();
                }
                catch (Exception ex)
                {
                    Preogress.Visibility = Visibility.Collapsed;
                }

                if (folder == null || files == null)
                {
                    return;
                }

                foreach (var file in files)
                {
                    try
                    {
                        if (file.Path.EndsWith(".svg"))
                        {
                            SvgFileModel svg = new SvgFileModel();
                            svg.SvgFile = file;
                            var fileAsBytes = await FileToByteArray(file);
                            svg.Svg = BytesToSvgDoc(fileAsBytes);
                            svg.TransparentBgVisibility = Visibility.Collapsed;
                            svg.Size = double.Parse(string.Format("{0:N}", (fileAsBytes.Length / 1024.00)));
                            SvgList.Add(svg);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (SvgList.Count > 0)
                {
                    svgGrid.ItemsSource = SvgList;
                    Count.Text = SvgList.Count.ToString();
                }
                else
                {
                    await ShowOkMessageAsync("error", "path folder does not contain Svg!!");
                    DragAndDrop.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                await ShowOkMessageAsync("error", ex.Message);
            }
            finally
            {
                Preogress.Visibility = Visibility.Collapsed;
                if (SvgList.Count < 1)
                    DragAndDrop.Visibility = Visibility.Visible;
            }


            //ad

        }

        private async void LoadFolder_Click(object sender, RoutedEventArgs e)
        {
            svgGrid.ItemsSource = null;
            await LoadSvgFromPicker(false);
        }

        private async void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            svgGrid.ItemsSource = null;
            await LoadSvgFromPicker(true);
        }

        private void svgGrid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        }

        private async void svgGrid_DropAsync(object sender, DragEventArgs e)
        {
            svgGrid.ItemsSource = null;
            DragAndDrop.Visibility = Visibility.Collapsed;
            Preogress.Visibility = Visibility.Visible;
            try
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    List<StorageFile> files = new List<StorageFile>();
                    List<StorageFolder> folders = new List<StorageFolder>();
                    SvgList = new List<SvgFileModel>();
                    Count.Text = "0";

                    var storageItems = await e.DataView.GetStorageItemsAsync();

                    foreach (var item in storageItems)
                    {
                        if (item.IsOfType(StorageItemTypes.Folder))
                        {
                            folders.Add((item as StorageFolder));
                        }
                        else
                        {
                            files.Add((item as StorageFile));
                        }
                    }

                    if (folders.Count > 0)
                    {
                        foreach (var folder in folders)
                        {
                            var items = await folder.GetFilesAsync();
                            if (items.Count > 0)
                            {
                                foreach (var file in items)
                                {
                                    try
                                    {
                                        if (file.Path.EndsWith(".svg"))
                                        {
                                            SvgFileModel svg = new SvgFileModel();
                                            svg.SvgFile = file;
                                            var fileAsBytes = await FileToByteArray(file);
                                            svg.Svg = BytesToSvgDoc(fileAsBytes);
                                            svg.TransparentBgVisibility = Visibility.Collapsed;
                                            svg.Size = double.Parse(string.Format("{0:N}", (fileAsBytes.Length / 1024.00)));
                                            SvgList.Add(svg);
                                        }
                                        //else
                                        //    await ShowOkMessageAsync("error", "dropped item is not Svg!!");
                                    }
                                    catch (Exception ex)
                                    {
                                        //await ShowOkMessageAsync("error", "dropped item is recognized!!");
                                    }
                                }
                            }
                        }
                    }

                    if (files.Count > 0)
                    {
                        foreach (StorageFile file in files)
                        {
                            try
                            {
                                if (file.Path.EndsWith(".svg"))
                                {
                                    SvgFileModel svg = new SvgFileModel();
                                    svg.SvgFile = file;
                                    var fileAsBytes = await FileToByteArray(file);
                                    svg.Svg = BytesToSvgDoc(fileAsBytes);
                                    svg.TransparentBgVisibility = Visibility.Collapsed;
                                    svg.Size = double.Parse(string.Format("{0:N}", (fileAsBytes.Length / 1024.00)));
                                    SvgList.Add(svg);
                                }
                                //else
                                //    await ShowOkMessageAsync("error", "dropped item is not Svg!!");
                            }
                            catch (Exception ex)
                            {
                                //await ShowOkMessageAsync("error", "dropped item is recognized!!");
                            }
                        }
                    }

                    if (SvgList.Count > 0)
                    {
                        svgGrid.ItemsSource = SvgList;
                        Count.Text = SvgList.Count.ToString();
                    }
                    else
                    {
                        await ShowOkMessageAsync("error", "the dropped folder/files do not contain Svg!!");
                        DragAndDrop.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowOkMessageAsync("error", ex.Message);
            }
            finally
            {
                Preogress.Visibility = Visibility.Collapsed;
            }

            //ad
        }

        public XmlDocument FillSvg(XmlDocument xDoc, string color)
        {
            string xmlAsString = xDoc.GetXml().Replace("000000", color);
            xDoc.LoadXml(xmlAsString);
            return xDoc;
        }

        public async Task ShowOkMessageAsync(string title, string msg)
        {
            MessageBox.Title = title;
            MessageBox.Content = msg;
            MessageBox.PrimaryButtonText = "OK";
            MessageBox.PrimaryButtonClick += Cd_PrimaryButtonClick;
            await MessageBox.ShowAsync();
        }

        private void Cd_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MessageBox.Hide();
        }

        private async void SvgImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var file = ((sender as SvgImage).DataContext as SvgFileModel).SvgFile;

                FileName.Text = file.DisplayName;
                FileSize.Text = ((sender as SvgImage).DataContext as SvgFileModel).Size.ToString();
            }
            catch (Exception ex)
            {
                await ShowOkMessageAsync("error", ex.Message);
            }

        }

        private async void Email_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText("taher.alzahrani@outlook.com");
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            Clipboard.SetContent(dataPackage);
            await ShowOkMessageAsync("Copy", "my email copied to clipboard");
        }

        //private async void GitHub_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    var dataPackage = new DataPackage();
        //    dataPackage.SetText("https://github.com/taherz/SVGViewerUWP/");
        //    dataPackage.RequestedOperation = DataPackageOperation.Copy;
        //    Clipboard.SetContent(dataPackage);
        //    await ShowOkMessageAsync("Copy", "App repo link copied to clipboard");
        //}

        public async Task<byte[]> FileToByteArray(StorageFile file)
        {
            Stream stream;
            byte[] bytes = null;
            if (file != null)
            {
                stream = await file.OpenStreamForReadAsync();
                bytes = new byte[(int)stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
            }

            return bytes;
        }

        public SvgDocument BytesToSvgDoc(byte[] fileAsArray)
        {
            SvgDocument svgDoc = SvgDocument.Parse(fileAsArray);
            return svgDoc;
        }

        private void OnAdRefreshed(object sender, RoutedEventArgs e)
        {

        }

        private void OnErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        {

        }

        private void Minus_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Slider.Value = Slider.Value - 50;
        }

        private void Plus_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Slider.Value = Slider.Value + 50;
        }

        private void CloseSelectedSVG_Click(object sender, RoutedEventArgs e)
        {
            HideSelectedSVG();
        }

        private async void SvgImage_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await ShowSelectedSVG(((sender as SvgImage).DataContext as SvgFileModel));
        }

        private void SvgImage_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void FlyoutOpen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //((sender as MenuFlyoutItem).Parent as MenuFlyout).Hide();
            await ShowSelectedSVG(((sender as MenuFlyoutItem).CommandParameter as SvgFileModel));
        }

        private async void FlyoutCopy_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //((sender as MenuFlyoutItem).Parent as MenuFlyout).Hide();
            var dataPackage = new DataPackage();
            var file = ((sender as MenuFlyoutItem).CommandParameter as SvgFileModel).SvgFile;
            List<IStorageItem> storageItems = new List<IStorageItem>();
            storageItems.Add(file);
            dataPackage.SetStorageItems(storageItems);
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            Clipboard.SetContent(dataPackage);
            await ShowOkMessageAsync("Copy Svg", file.DisplayName + " has been coppied to clipboard successfully.");
        }

        private void whiteBg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (SelectedSvgGrid.Visibility == Visibility.Visible)
            {
                if (SelectedSvg.Content == null)
                    return;

                SelectedSvgGrid.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                SvgList = new List<SvgFileModel>();
                foreach (SvgFileModel item in svgGrid.Items)
                {
                    item.SVGBackground = new SolidColorBrush(Colors.White);
                    item.TransparentBgVisibility = Visibility.Collapsed;
                    SvgList.Add(item);
                }
                svgGrid.ItemsSource = SvgList;
            }
        }

        private void blackBg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (SelectedSvgGrid.Visibility == Visibility.Visible)
            {
                if (SelectedSvg.Content == null)
                    return;

                SelectedSvgGrid.Background = new SolidColorBrush(Colors.Black);
            }
            else
            {
                SvgList = new List<SvgFileModel>();
                foreach (SvgFileModel item in svgGrid.Items)
                {
                    item.SVGBackground = new SolidColorBrush(Colors.Black);
                    item.TransparentBgVisibility = Visibility.Collapsed;
                    SvgList.Add(item);
                }
                svgGrid.ItemsSource = SvgList;
            }
        }

        private void grayBg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (SelectedSvgGrid.Visibility == Visibility.Visible)
            {
                if (SelectedSvg.Content == null)
                    return;

                SelectedSvgGrid.Background = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                SvgList = new List<SvgFileModel>();
                foreach (SvgFileModel item in svgGrid.Items)
                {
                    item.SVGBackground = new SolidColorBrush(Colors.LightGray);
                    item.TransparentBgVisibility = Visibility.Collapsed;
                    SvgList.Add(item);
                }
                svgGrid.ItemsSource = SvgList;
            }
            
        }

        private void noBg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SvgList = new List<SvgFileModel>();
            foreach (SvgFileModel item in svgGrid.Items)
            {
                item.SVGBackground = new SolidColorBrush(Colors.White);
                item.TransparentBgVisibility = Visibility.Visible;
                SvgList.Add(item);
            }
            svgGrid.ItemsSource = SvgList;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HideSelectedSVG();
        }
    }

    public class SvgFileModel
    {
        public StorageFile SvgFile { get; set; }
        public SvgDocument Svg { get; set; }
        public double Size { get; set; }
        public SolidColorBrush SVGBackground { get; set; }
        public Visibility TransparentBgVisibility { get; set; }
    }
}
