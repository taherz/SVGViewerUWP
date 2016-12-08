using Mntone.SvgForXaml;
using Mntone.SvgForXaml.Primitives;
using Mntone.SvgForXaml.Shapes;
using Mntone.SvgForXaml.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
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
        }

        List<SvgFileModel> SvgList = new List<SvgFileModel>();

        private async Task LoadSvgFromFolder(bool isLoadFile)
        {
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
                            var xmlDoc = await XmlDocument.LoadFromFileAsync(pickedFile);
                            var svgDoc = SvgDocument.Parse(xmlDoc);
                            svg.Svg = svgDoc;
                            svg.SvgPath = pickedFile.Path;
                            SvgList.Add(svg);
                            FolderPath.Text = pickedFile.Path;

                            if (SvgList.Count > 0)
                                svgGrid.ItemsSource = SvgList;
                        }
                        return;
                    }

                    folder = await folderPicker.PickSingleFolderAsync();
                    files = await folder.GetFilesAsync();
                    FolderPath.Text = folder.Path;
                }
                catch (Exception ex)
                {

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
                            var xmlDoc = await XmlDocument.LoadFromFileAsync(file);
                            var svgDoc = SvgDocument.Parse(xmlDoc);
                            svg.Svg = svgDoc;
                            svg.SvgPath = file.Path;
                            SvgList.Add(svg);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (SvgList.Count > 0)
                    svgGrid.ItemsSource = SvgList;
                else
                    await ShowOkMessageAsync("error", "path folder does not contain Svg!!");
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

        private async void LoadFolder_Click(object sender, RoutedEventArgs e)
        {
            svgGrid.ItemsSource = null;
            await LoadSvgFromFolder(false);
        }

        private async void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            svgGrid.ItemsSource = null;
            await LoadSvgFromFolder(true);
        }

        private async void SvgImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine(((sender as SvgImage).DataContext as SvgFileModel).SvgPath);
                var dataPackage = new DataPackage();

                var file = ((sender as SvgImage).DataContext as SvgFileModel).SvgFile;

                List<IStorageItem> storageItems = new List<IStorageItem>();
                storageItems.Add(file);
                dataPackage.SetStorageItems(storageItems);
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                Clipboard.SetContent(dataPackage);
                await ShowOkMessageAsync("Copy Svg", file.DisplayName + " has been coppied to clipboard successfully.");
            }
            catch (Exception ex)
            {
                await ShowOkMessageAsync("error", ex.Message);
            }
        }

        private void svgGrid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        }

        private async void svgGrid_DropAsync(object sender, DragEventArgs e)
        {
            svgGrid.ItemsSource = null;
            Preogress.Visibility = Visibility.Visible;
            try
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    dynamic files;
                    var storageItems = await e.DataView.GetStorageItemsAsync();

                    if (storageItems[0].IsOfType(StorageItemTypes.Folder))
                    {
                        files = await (storageItems[0] as StorageFolder).GetFilesAsync();
                    }
                    else
                    {
                        files = storageItems;
                    }


                    SvgList = new List<SvgFileModel>();
                    foreach (StorageFile file in files)
                    {
                        try
                        {
                            if (file.Path.EndsWith(".svg"))
                            {
                                SvgFileModel svg = new SvgFileModel();
                                svg.SvgFile = file;
                                XmlDocument xmlDoc = await XmlDocument.LoadFromFileAsync(file);
                                //xmlDoc = FillSvg(xmlDoc, "AAAAAA");
                                SvgDocument svgDoc = SvgDocument.Parse(xmlDoc);

                                svg.Svg = svgDoc;
                                svg.SvgPath = file.Path;
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

                    if (SvgList.Count > 0)
                        svgGrid.ItemsSource = SvgList;
                    else
                        await ShowOkMessageAsync("error", "the dropped folder/files do not contain Svg!!");
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

        public XmlDocument FillSvg(XmlDocument xDoc, string color)
        {
            string xmlAsString = xDoc.GetXml().Replace("000000", color);
            xDoc.LoadXml(xmlAsString);
            return xDoc;
        }

        ContentDialog cd = new ContentDialog();
        public async Task ShowOkMessageAsync(string title, string msg)
        {
            cd.Title = title;
            cd.Content = msg;
            cd.PrimaryButtonText = "OK";
            cd.PrimaryButtonClick += Cd_PrimaryButtonClick;
            await cd.ShowAsync();
        }

        private void Cd_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            cd.Hide();
        }
    }

    public class SvgFileModel
    {
        public StorageFile SvgFile { get; set; }
        public SvgDocument Svg { get; set; }
        public string SvgPath { get; set; }
    }
}
