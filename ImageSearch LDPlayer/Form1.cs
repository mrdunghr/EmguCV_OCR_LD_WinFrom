using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ImageSearch_LDPlayer
{
    public partial class Form1 : Form
    {
        private bool isCapturing = false;
        private Thread captureThread;
        private WindowCapture windowCapture;
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox2.Image = new Bitmap(openFileDialog.FileName);

                    textBox1.Text = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra xem TextBox có chứa đường dẫn hợp lệ không
                if (System.IO.File.Exists(textBox1.Text))
                {
                    // Load the image from the path in the TextBox into PictureBox
                    pictureBox2.Image = new Bitmap(textBox1.Text);
                }
                else
                {
                    // Nếu đường dẫn không hợp lệ, xóa hình ảnh trong PictureBox
                    pictureBox2.Image = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!isCapturing)
            {
                // Bắt đầu vòng lặp chụp ảnh
                isCapturing = true;
                button1.Text = "Stop";
                captureThread = new Thread(CaptureLoop);
                captureThread.Start();
            }
            else
            {
                // Dừng vòng lặp chụp ảnh
                isCapturing = false;
                button1.Text = "Start";
                captureThread.Join(); // Chờ vòng lặp chụp ảnh kết thúc
            }
        }

        private void CaptureLoop()
        {
            windowCapture = new WindowCapture(textBox2.Text);
            while (isCapturing)
            {
                // Thực hiện chụp ảnh ở đây
                Mat mat = windowCapture.CaptureWindow();

                // Lấy đường dẫn từ TextBox
                string templatePath = textBox1.Text;

                // Kiểm tra xem đường dẫn có hợp lệ không
                if (!File.Exists(templatePath))
                {
                    MessageBox.Show("Đường dẫn không tồn tại!");
                    return;
                }

                // Xử lý ảnh đã chụp
                Mat source = CvInvoke.Imread("captured_image.png", ImreadModes.Color);
                Mat template = CvInvoke.Imread(templatePath, ImreadModes.Color);

                // Thực hiện xử lý ảnh
                Mat processedImage = windowCapture.MathTemplateByThreshold(source, template);


                // Chuyển đổi Mat thành Bitmap
                Bitmap bitmap = processedImage.ToBitmap();

                pictureBox1.Image = new Bitmap(bitmap);


                Thread.Sleep(200);
            }
        }

        // đảm bảo giái phóng dung lượng khi đóng form
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (isCapturing)
            {
                isCapturing = false;
                captureThread.Join();
            }
        }
    }
}
