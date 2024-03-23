using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace ImageSearch_LDPlayer
{
    internal class WindowCapture : IDisposable
    {
        private IntPtr _windowHandle;
        public WindowCapture(string windowTitle)
        {
            while (_windowHandle == IntPtr.Zero)
            {
                _windowHandle = User32.FindWindow(null, windowTitle);
                if (_windowHandle == IntPtr.Zero)
                {
                    Console.WriteLine("Waiting for window to open...");
                    Thread.Sleep(1000);
                }
            }
        }

        public Mat MathTemplateByThreshold(Mat image, Mat template)
        {
            if (image.IsEmpty || template.IsEmpty)
            {
                Console.WriteLine("Không thể tải hình và mẫu, kiểm tra lại nguồn ảnh");
                return null;
            }
            else
            {
                Console.WriteLine("Tải hình ảnh thành công");
            }

            // lưu kết quả vào đối tượng mat
            Mat result = new Mat();

            //phương thức static tìm kiếm hình ảnh
            CvInvoke.MatchTemplate(image, template, result, TemplateMatchingType.CcoeffNormed);

            // tìm giá trị tương tự lớn nhất nhỏ nhất, vị trí lớn nhất nhỏ nhất
            double minVal = 0, maxVal = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            // maxval sau khi thực hiện hàm minmaxloc
            Console.WriteLine("Giá trị tương tự cao nhất (maxVal): " + maxVal);
            Console.WriteLine("Giá trị tương tự thấp nhất (minval): " + minVal);

            // Đặt ngưỡng để xác định kết quả
            double threshold = 1;
            bool isFound = maxVal < threshold || minVal < threshold;

            // Hiển thị kết quả
            if (isFound)
            {
                Console.WriteLine("Mẫu được tìm thấy trong hình ảnh đầu vào.");
                // Vẽ một hình chữ nhật xung quanh vị trí tốt nhất của mẫu trên hình ảnh đầu vào
                Rectangle rect = new Rectangle(maxLoc, template.Size);
                CvInvoke.Rectangle(image, rect, new MCvScalar(0, 0, 255), 1);

                // Hiển thị hình ảnh kết quả
                //CvInvoke.Imshow("Result", image);
                //CvInvoke.Imshow("Temp", template);
                //CvInvoke.WaitKey(0);
                return image;
            }
            else
            {
                Console.WriteLine("Mẫu không được tìm thấy trong hình ảnh đầu vào.");
                return null;
            }
        }

        public Mat CaptureWindow()
        {
            var rect = new User32.RECT();
            User32.GetWindowRect(_windowHandle, ref rect);

            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var gfxBmp = Graphics.FromImage(bmp))
            {
                var hdcBitmap = gfxBmp.GetHdc();
                User32.PrintWindow(_windowHandle, hdcBitmap, 0);
                gfxBmp.ReleaseHdc(hdcBitmap);
            }

            // Tạo mảng byte từ Bitmap
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            Int32 bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
            Int32 stride = bitmapData.Stride;
            IntPtr scan0 = bitmapData.Scan0;
            Int32 imageDataSize = stride * height;
            byte[] imageData = new byte[imageDataSize];

            // Copy dữ liệu hình ảnh vào mảng byte
            System.Runtime.InteropServices.Marshal.Copy(scan0, imageData, 0, imageDataSize);

            bmp.UnlockBits(bitmapData);

            // Tạo Mat từ mảng byte
            Mat capturedMat = new Image<Bgra, byte>(width, height, stride, scan0).Mat;

            // Lưu hình ảnh vào file
            SaveCapturedImage(capturedMat, "captured_image.png");

            return capturedMat;
        }

        private void SaveCapturedImage(Mat image, string filePath)
        {
            // Lưu hình ảnh từ đối tượng Mat ra file
            image.Save(filePath);
        }
        public void Dispose()
        {
            _windowHandle = IntPtr.Zero;
        }
    }
}
