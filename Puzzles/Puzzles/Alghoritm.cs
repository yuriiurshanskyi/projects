using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Puzzles
{
    struct PixelColor
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;
    }
    class Alghoritm
    {
        private List<int> GetNumbers(List<BitmapSource> list)
        {
            List<int> numbers = new List<int>();

            for (int i = 1; i <= list.Count; i++)
            {
                if (list.Count % i == 0)
                {
                    numbers.Add(i);
                }
            }

            return numbers;
        }
        private PixelColor[,] GetPixels(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
            {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            source.CopyPixels(result, width * 4, 0, true);

            return result;
        }

        private double GetDifference(PixelColor first, PixelColor second)
        {
            int dr = Math.Abs(first.Red - second.Red);
            int dg = Math.Abs(first.Green - second.Green);
            int db = Math.Abs(first.Blue - second.Blue);
            int da = Math.Abs(first.Alpha - second.Alpha);

            return Math.Sqrt(dr * dr + dg * dg + db * db);
        }

        private double GetRightDifference(PixelColor[,] left, PixelColor[,] right)
        {
            double rightDifference = 0;

            try
            {
                if (left.Length != right.Length)
                    throw new Exception("Різні параметри аргументів");

                for (int i = 0; i < left.GetLength(0); i++)
                {
                    rightDifference += GetDifference(left[i, left.GetLength(1) - 1], right[i, 0]);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return rightDifference;
        }

        private double GetBottomDifference(PixelColor[,] up, PixelColor[,] down)
        {
            double bottomDifference = 0;

            try
            {
                if (up.Length != down.Length)
                    throw new Exception("Різні параметри аргументів");

                for (int i = 0; i < up.GetLength(1); i++)
                {
                    bottomDifference += GetDifference(up[up.GetLength(0) - 1, i], down[0, i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return bottomDifference;
        }

        private BitmapSource GetRightImage(BitmapSource first, List<BitmapSource> list, ref double totalDifference)
        {
            double min = double.MaxValue;

            PixelColor[,] left = GetPixels(first);

            BitmapSource next = null;

            for (int i = 0; i < list.Count; i++)
            {
                PixelColor[,] right = GetPixels(list[i]);

                double value = GetRightDifference(left, right);

                if (min > value)
                {
                    next = list[i];
                    min = value;
                }
            }
            totalDifference += min;

            return next;
        }

        private BitmapSource GetBottomImage(BitmapSource first, List<BitmapSource> list, ref double totalDifference)
        {
            double min = double.MaxValue;

            PixelColor[,] up = GetPixels(first);

            BitmapSource next = null;


            for (int i = 0; i < list.Count; i++)
            {
                PixelColor[,] down = GetPixels(list[i]);


                double value = GetBottomDifference(up, down);

                if (min > value)
                {
                    next = list[i];
                    min = value;
                }
            }
            totalDifference += min;

            return next;
        }
        private double GetBestCurrentVariant(List<BitmapSource> list, int row, int col, ref BitmapSource[,] bestChoice)
        {
            BitmapSource bestPiece;

            double min = double.MaxValue;

            for (int j = 0; j < list.Count; j++)    //bust all elements for current arrangment
            {
                double total = 0; //showing total difference

                BitmapSource[,] elements = new BitmapSource[row, col];  //received result

                List<BitmapSource> cash = new List<BitmapSource>(list);     //collection of avaible elements

                elements[0, 0] = cash[j];

                cash.RemoveAt(j);

                //заповнюємо перший рядок
                for (int icol = 0; icol < col - 1; icol++)  // busting throgh the columns
                {
                    bestPiece = GetRightImage(elements[0, icol], cash, ref total);//getting best right image
                    elements[0, icol + 1] = bestPiece;//adding getting result to the puzzle

                    cash.Remove(bestPiece);//removing unavaible element
                }

                for (int irow = 1; irow < row; irow++)
                {
                    for (int icol = 0; icol < col; icol++)
                    {
                        bestPiece = GetBottomImage(elements[irow - 1, icol], cash, ref total); //getting best down image

                        elements[irow, icol] = bestPiece;//adding getting result to the puzzle

                        cash.Remove(bestPiece);//removing unavaible element
                    }
                }

                if (min > total)
                {
                    bestChoice = (BitmapSource[,])elements.Clone();
                    min = total;
                }
            }

            return min;
        }

        public BitmapSource[,] GetBestPuzzleImage(List<BitmapImage> list)
        {
            List<BitmapSource> bmp = new List<BitmapSource>();

            for (int i = 0; i < list.Count; i++)
            {
                BitmapSource temp = (BitmapSource)list[i];
                bmp.Add(temp);
            }

            List<int> numbers = new List<int>(GetNumbers(bmp));

            BitmapSource[,] bestChoice = null;

            double min = double.MaxValue;  //value of the totatl difference between borders of pieces

            for (int i = 0; i < numbers.Count; i++)
            {
                int row = numbers[i];
                int col = list.Count / row;

                BitmapSource[,] possibleChoice = new BitmapSource[row, col];

                double value = GetBestCurrentVariant(bmp, row, col, ref possibleChoice);

                if (min > value)
                {
                    bestChoice = (BitmapSource[,])possibleChoice.Clone();

                    min = value;
                }
            }

            return bestChoice;
        }
    }
}