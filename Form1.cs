using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace AmazonFaceRekognition_WF

{
    public partial class Form1 : Form
    {

        private string accessKey = "AKIA6ODUZCHYJDSZJ3WG";
        private string secretKey = "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";
        private static string bucket = "myrekognition1";
        PictureBox pictureBox;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var filePath = string.Empty;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\Users\\User\\OneDrive\\Pictures\\";
                openFileDialog.Filter = "jpg files (*.jpg)|*.jpg|png files (*.png)|*.png";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    pictureBox = new PictureBox
                    {
                        Image = System.Drawing.Image.FromFile(filePath), // replace with your image path
                        SizeMode = PictureBoxSizeMode.AutoSize
                    };
                    this.ClientSize = pictureBox.Size;
                    this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                    this.button1.Visible = false;
                    this.Controls.Add(pictureBox);
                }
            }
            string fileName = filePath.Split('\\')[filePath.Split('\\').Length - 1];

            try
            {

                // Upload the file to Amazon S3
                BasicAWSCredentials credentials = new BasicAWSCredentials(accessKey, secretKey);
                var s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.EUWest1);
                TransferUtility fileTransferUtility = new TransferUtility(s3Client);
                fileTransferUtility.Upload(filePath, bucket, fileName);

            }
            catch (AmazonS3Exception k)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", k.Message);
            }
            catch (Exception ee)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", ee.Message);
            }

            FaceDetection(filePath);
        }

        private async Task FaceDetection(string photo)
        {
            BasicAWSCredentials credentials = new BasicAWSCredentials(accessKey, secretKey);
            var rekognitionClient = new AmazonRekognitionClient(credentials, Amazon.RegionEndpoint.EUWest1);

            var detectFacesRequest = new DetectFacesRequest()
            {
                Image = new Amazon.Rekognition.Model.Image()
                {
                    S3Object = new S3Object()
                    {
                        Name = photo.Split('\\')[photo.Split('\\').Length - 1],
                        Bucket = bucket,
                    },
                },
                Attributes = new List<string>() { "ALL" },
            };

            try
            {
                DetectFacesResponse detectFacesResponse = await rekognitionClient.DetectFacesAsync(detectFacesRequest);
                detectFacesResponse.FaceDetails.ForEach(face =>
                {
                    ShowBoundingBoxPositions(face.BoundingBox, face.Emotions);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ShowBoundingBoxPositions(BoundingBox box, List<Emotion> emotions)
        {
            string emotion = emotions[emotions.LastIndexOf(emotions.Find(e=>e.Confidence== emotions.Max(testc => testc.Confidence)))].Type;
            Bitmap bmp = new Bitmap(pictureBox.Image);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                using (Pen pen2 = new Pen(Color.Red, 2))
                {
                    int left = (int)(box.Left * bmp.Width);
                    int top = (int)(box.Top * bmp.Height);
                    int width = (int)(box.Width * bmp.Width);
                    int height = (int)(box.Height * bmp.Height);
                    Rectangle rect = new Rectangle(left, top, width, height);
                    Brush brush = new SolidBrush(Color.Red);
                    StringFormat format = new StringFormat()
                    {
                        Alignment = StringAlignment.Center
                    };
                    g.DrawString(emotion, new Font("Arial", 10, FontStyle.Regular), brush, rect, format);
                    g.DrawRectangle(pen2, rect);
                }
                
            }
            pictureBox.Image = bmp;
        }
    }
}