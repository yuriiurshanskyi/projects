using Puzzles;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Puzzles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Path { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Reset()
        {
            imageNet.Children.Clear();
            imageNet.RowDefinitions.Clear();
            imageNet.ColumnDefinitions.Clear();

            sideBar.Children.Clear();

            Path = null;
        }
        //receiving name of catalog 
        public void FormatCatalog(string path)
        {
            Path = path.Replace("\\\\", "\\");
        }

        //format destination path 
        private string DefineCatalogName(string destinationPath)
        {
            destinationPath = destinationPath.Replace("\\\\", "\\");

            destinationPath += "\\images";

            int a = 1;

            while (Directory.Exists(destinationPath))
            {
                if (Directory.Exists(destinationPath + $"({a})"))
                {
                    a++;
                }
                else
                {
                    destinationPath += $"({a})";
                    break;
                }
            }

            return destinationPath;
        }

        //getting an image name
        private string GetFileName(string sourcePath)
        {
            sourcePath = sourcePath.Remove(sourcePath.LastIndexOf('.'));
            sourcePath = sourcePath.Substring(sourcePath.LastIndexOf("\\") + 1);
            sourcePath = Regex.Replace(sourcePath, "_", "");

            return sourcePath;
        }

        private void SavePiece(CroppedBitmap croppedBitmap, string filename)
        {
            Image image = new Image { Source = croppedBitmap };

            var encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.Source));

            try
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    encoder.Save(stream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Вказаний невірний шлях");
            }
        }

        //cropping image into pieces
        public void CutImage(int nwidth, int nheight, string sourcePath, string destinationPath)
        {
            Reset();

            destinationPath = DefineCatalogName(destinationPath);

            Directory.CreateDirectory(destinationPath);

            sourcePath = sourcePath.Replace("\\\\", "\\");
            BitmapImage bitmap = new BitmapImage(new Uri(sourcePath));

            sourcePath = GetFileName(sourcePath);

            for (int y = 0, i = 0; y < bitmap.PixelHeight; y += nheight, i++)
            {
                for (int x = 0, j = 0; x < bitmap.PixelWidth; x += nwidth, j++)
                {
                    try
                    {
                        CroppedBitmap cb = new CroppedBitmap((BitmapSource)bitmap, new Int32Rect(x, y, nwidth, nheight));

                        string filename = destinationPath + "\\" + sourcePath + $"_{i}_{j}.jpg";

                        SavePiece(cb, filename);
                    }
                    catch (ArgumentException ex)
                    {
                        break;
                    }
                }
            }

            AddToStack(destinationPath);
        }

        //defining imageNet
        private void DefineimageNet(int row, int col, GridUnitType type, bool showBorders)
        {
            for (int i = 0; i < row; i++)
            {
                imageNet.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, type) });
            }

            for (int i = 0; i < col; i++)
            {
                imageNet.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, type) });
            }

            imageNet.ShowGridLines = showBorders;
        }

        private (int, int) ConvertIndexes(string file)
        {
            int row = Int32.Parse(file.Substring(file.IndexOf("_") + 1, file.LastIndexOf("_") - file.IndexOf("_") - 1));

            int col = Int32.Parse(file.Substring(file.LastIndexOf("_") + 1, file.LastIndexOf(".") - file.LastIndexOf("_") - 1));

            (int, int) tuple = (row, col);

            return tuple;
        }

        //getting sizes of imageNet
        private (int, int) GetSizes((int Row, int Col) imageNetSize, string file)
        {
            var tuple = ConvertIndexes(file);

            if (imageNetSize.Row < tuple.Item1 + 1)
            {
                imageNetSize.Row = tuple.Item1 + 1;
            }

            if (imageNetSize.Col < tuple.Item2 + 1)
            {
                imageNetSize.Col = tuple.Item2 + 1;
            }

            return imageNetSize;
        }

        //adding pieces to the side bar
        private void AddToStack(string destinationPath)
        {
            try
            {
                string[] files;

                files = Directory.GetFiles(destinationPath);

                imageNet.AllowDrop = true;

                (int Row, int Col) imageNetSizes = (0, 0);

                Random rnd = new Random();
                files = files.OrderBy(x => rnd.Next()).ToArray();

                for (int i = 0; i < files.Length; i++)
                {

                    imageNetSizes = GetSizes(imageNetSizes, files[i]);

                    BitmapImage bmp = new BitmapImage(new Uri(files[i]));

                    Image img = new Image();
                    img.Source = bmp;
                    img.Stretch = Stretch.Uniform;
                    img.Margin = new Thickness(5);

                    img.MouseLeftButtonDown += img_MouseLeftButtonDown;


                    sideBar.Children.Add(img);
                }

                DefineimageNet(imageNetSizes.Row, imageNetSizes.Col, GridUnitType.Star, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //checking, if puzzle is complete
        private bool IsPuzzlesComplete()
        {
            bool complete = true;

            for (int i = 0; i < imageNet.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < imageNet.ColumnDefinitions.Count; j++)
                {
                    try
                    {
                        var control = imageNet.Children
                                          .Cast<Image>()
                                          .First(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j);   //getting an element with its position in imageNet

                        string filename = ((BitmapImage)control.Source).UriSource.ToString();

                        (int Row, int Col) indexes = ConvertIndexes(filename);

                        if (indexes.Col != j || indexes.Row != i)
                        {
                            throw new Exception();
                        }
                    }
                    catch
                    {
                        complete = false;
                    }
                }
            }
            return complete;
        }

        private void PlaceImage(Image image, DragEventArgs e)
        {
            double y = e.GetPosition(imageNet).Y;
            int row = (int)(y / imageNet.RowDefinitions[0].ActualHeight);

            double x = e.GetPosition(imageNet).X;
            int col = (int)(x / imageNet.ColumnDefinitions[0].ActualWidth);

            MoveFromimageNetToStack(row, col);

            Grid.SetColumn(image, col);
            Grid.SetRow(image, row);

            (imageNet).Children.Add(image);
        }

        private Image GetImageFromData(DragEventArgs e)
        {
            if ((e.Data.GetData(typeof(ImageSource)) != null))
            {
                ImageSource image = e.Data.GetData(typeof(ImageSource)) as ImageSource;
                Image imageControl = new Image() { Stretch = Stretch.Fill, Source = image };

                return imageControl;
            }

            throw new Exception("Зображення не виявлено");
        }

        //moving an element to stack
        private void MoveFromimageNetToStack(int row, int col)
        {
            try
            {
                var control = imageNet.Children
                                                  .Cast<Image>()
                                                  .First(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);

                imageNet.Children.Remove(control);

                control.MouseLeftButtonDown += img_MouseLeftButtonDown;

                control.Stretch = Stretch.Uniform;
                control.Margin = new Thickness(5);

                sideBar.Children.Add(control);
            }
            catch
            {
                return;
            }
        }

        //removing a duplicate from imageNet, while moving
        private void RemoveExampleFromContainer(Image image, Panel panel)
        {
            try
            {
                var control = panel.Children
                                                  .Cast<Image>()
                                                  .First(e => e.Source == image.Source);

                panel.Children.Remove(control);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void RemoveDuplicates(Image image)
        {
            if (image != null)
            {
                RemoveExampleFromContainer(image, sideBar);
                RemoveExampleFromContainer(image, imageNet);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        private void LocateImage(int row, int col, List<BitmapSource> temp)
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Image img = new Image { Stretch = Stretch.None };

                    img.Source = temp[i * col + j];

                    Grid.SetRow(img, i);
                    Grid.SetColumn(img, j);

                    imageNet.Children.Add(img);
                }
            }
        }


        private void CutImage_Click(object sender, RoutedEventArgs e)
        {
            ConfigureSize configureSize = new ConfigureSize(this);
            configureSize.ShowDialog();
        }

        private void OpenCatalog_Click(object sender, RoutedEventArgs e)
        {
            Reset();

            ChooseFile chooseFile = new ChooseFile(this);
            chooseFile.ShowDialog();

            AddToStack(Path);
        }

        private void img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image img = (Image)sender;

            DataObject dataObject = new DataObject(typeof(ImageSource), img.Source);
            DragDrop.DoDragDrop(img, dataObject, DragDropEffects.Move);
        }

        private void imageNet_Drop(object sender, DragEventArgs e)
        {
            Image imageControl = GetImageFromData(e);

            imageControl.MouseLeftButtonDown += img_MouseLeftButtonDown;

            try
            {
                RemoveDuplicates(imageControl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //place an image
            PlaceImage(imageControl, e);

            if (IsPuzzlesComplete())
            {
                MessageBox.Show("Картинка складена");
            }
        }

        private void sideBar_Drop(object sender, DragEventArgs e)
        {
            Image imageControl = GetImageFromData(e);

            try
            {
                RemoveDuplicates(imageControl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            imageControl.MouseLeftButtonDown += img_MouseLeftButtonDown;
            imageControl.Margin = new Thickness(5);

            sideBar.Children.Add(imageControl);
        }

        private void Alghoritm_Click(object sender, RoutedEventArgs e)
        {
            ChooseFile chooseFile = new ChooseFile(this);
            chooseFile.ShowDialog();

            List<BitmapImage> bmp = new List<BitmapImage>();

            try
            {
                string[] files;

                files = Directory.GetFiles(Path);

                for (int i = 0; i < files.Length; i++)
                {
                    bmp.Add(new BitmapImage(new Uri(files[i])));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            Alghoritm alghoritm = new Alghoritm();
            Reset();

            try
            {
                if (bmp.Count == 0)
                {
                    throw new Exception(message: "Обрано невірний шлях");
                }
                var arr = (BitmapSource[,])alghoritm.GetBestPuzzleImage(bmp).Clone();

                int col = arr.GetLength(0);

                int row = arr.GetLength(1);

                //визначаємо сітку
                DefineimageNet(row, col, GridUnitType.Auto, false);

                //перевертаємо масив
                List<BitmapSource> temp = new List<BitmapSource>();

                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        temp.Add(arr[j, i]);
                    }
                }

                //додаємо зображення
                LocateImage(row, col, temp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
