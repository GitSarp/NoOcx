using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NOocx
{
    /// <summary>
    /// Mix.xaml 的交互逻辑
    /// </summary>
    public partial class Mix : Window
    {
        static FilterInfoCollection videoDevices;//所有可用摄像头
        private string currVideoName = "不可用";//当前摄像头名称，初始化不可用。
        private VideoCaptureDevice videoSource;//当前摄像头
        private int videoIndex=0;//摄像头索引,用于选中菜单项
        private int resolutionIndex = 0;//分辨率索引
        private Dictionary<string, VideoCaptureDevice> videodatasource = new Dictionary<string, VideoCaptureDevice>();//存储所有摄像头实例
        
        private List<ScannningFile> fileList = new List<ScannningFile>();//采集项列表
        private int currFileIndex = 0;//选中的采集项索引，默认第一项
        private List<System.Windows.Controls.Image> thumbnaillist = new List<System.Windows.Controls.Image>();//底部缩略图列表
        private List<Label> lablelist = new List<Label>();//底部缩略图列表
        private Dictionary<int, List<Bitmap>> imageMemorys = new Dictionary<int, List<Bitmap>>();//所有图像缓存，<image_no,bitmaplist>
        private BitmapImage myBitmapImage = new BitmapImage();//选中的图像缓存

        public Mix()
        {
            InitializeComponent();
            //初始化扫描文件
            fileList.Add(new ScannningFile(11, "证券账户申请表",0,1, "Integrated Webcam", 0));
            fileList.Add(new ScannningFile(11, "注销证券账户申请表",1,1,"Integrated Webcam", 1));
            fileList.Add(new ScannningFile(11, "test", 2, 0, "Integrated Webcam", 2));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("无可用视频输入设备，程序自动结束...");
                //
            }
            //绘制底部缩略图
            foreach(ScannningFile scanfile in fileList)
            {
                Grid smallPic = new Grid();
                System.Windows.Controls.Image tmpImg = new System.Windows.Controls.Image();
                tmpImg.Name = scanfile.fileName;
                tmpImg.Height = 40;
                tmpImg.MouseLeftButtonDown += image_MouseLeftButtonDown;
                tmpImg.ToolTip = scanfile.fileName;
                if (scanfile.videorpic == 0)
                    tmpImg.Visibility = Visibility.Hidden;
                else
                {
                    tmpImg.ToolTip += "100KB";//测试写死
                }
                thumbnaillist.Add(tmpImg);//添加到list中，鼠标滚动时访问
                smallPic.Children.Add(tmpImg);
                Label tmpLb = new Label();
                tmpLb.Content = scanfile.fileName;
                tmpLb.BorderBrush = System.Windows.Media.Brushes.Black;
                tmpLb.BorderThickness = new Thickness(1);
                if (tmpImg.Visibility == Visibility.Visible)
                    tmpLb.Visibility = Visibility.Hidden;
                lablelist.Add(tmpLb);
                smallPic.Children.Add(tmpLb);
                bottom_image.Children.Add(smallPic);
            }
            //加载输入设备名称
            //for (int i = 0; i < videoDevices.Count; i++)
            foreach (FilterInfo device in videoDevices)
            {
                //FilterInfo device = videoDevices[i];
                videoMenu.Items.Add(device.Name);
                //MenuItem tmpMI = new MenuItem();
                //tmpMI.Header = device.Name;
                //videoMenu.Items.Add(tmpMI);
            }
            getVideoIndex(fileList[currFileIndex].videosourceName);
            if (videoIndex == -1)
                videoIndex = 0;
            videoMenu.SelectedIndex = videoIndex;
        }

        //取摄像头实例、摄像头索引和分辨率索引
        private void getDevice(int fileIndex,string videoName="")
        {
            if(videoName=="")
                videoName = fileList[fileIndex].videosourceName;//查找设备名
            if (videoName == currVideoName)
            {
                resolutionIndex = fileList[fileIndex].resolution;//使用当前设备
            }else
            {
                if (videodatasource.ContainsKey(videoName))//摄像头池中有，直接取
                {
                    CloseCurrentVideoSource();
                    videoSource=videodatasource[videoName];//返回摄像头实例
                    getVideoIndex(videoName);
                    resolutionIndex = fileList[fileIndex].resolution;
                    currVideoName = videoName;//置摄像头名
                }
                else
                {
                    getVideoIndex(videoName);
                    if ( (videoIndex != -1) ||//设备可用,池中没有
                         ((videoIndex == -1)&& (videoSource == null)) )//设备不可用，且池为空（当前设备为空，池一定空）需要创建，否则使用当前设备
                    {
                        if (videoIndex == -1)//不可用池为空创建默认设备（第一个摄像头）。
                        {
                            videoName = videoDevices[0].Name;
                            videoIndex = 0;
                            resolutionIndex = 0;
                        }else
                        {
                            resolutionIndex = fileList[fileIndex].resolution;
                        }
                        CloseCurrentVideoSource();
                        videoSource = new VideoCaptureDevice(videoDevices[videoIndex].MonikerString);
                        videodatasource.Add(videoName, videoSource);
                        currVideoName = videoName;//置摄像头名
                    }
                }
            }
        }
        //查找摄像头索引
        private void getVideoIndex(string videoName)
        {
            //if (videoName == "")
            //    videoName = fileList[fileIndex].videosourceName;
            for (int i = 0; i < videoDevices.Count; i++)
            {
                if (videoDevices[i].Name == videoName)
                {
                    videoIndex = i; return;
                }
            }
            videoIndex = -1;
            return;
        }


        //切换视频源
        private void videoMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getDevice(currFileIndex, videoMenu.SelectedItem.ToString());
            goPikachu();
        }

        //开始拍摄
        private void goPikachu()
        {
            //不打开摄像头的话不知道能否准确获取摄像头的所有分辨率 
            //videoPlayer.VideoSource = videoSource;
            //videoPlayer.Start();
            // set NewFrame event handler
            //videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            if(mediaType.Items.Count>0)
                mediaType.Items.Clear();//先清除数据
            foreach (VideoCapabilities videoCap in videoSource.VideoCapabilities)
            {
                mediaType.Items.Add(string.Format("{0}X{1}", videoCap.FrameSize.Width, videoCap.FrameSize.Height));
            }
            mediaType.SelectedIndex = resolutionIndex;
        }

        //切换分辨率
        private void mediaType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mediaType.Items.Count == 0)
                return;//清除时直接退出
            CloseCurrentVideoSource();
            videoSource.VideoResolution = videoSource.VideoCapabilities[mediaType.SelectedIndex];
            videoPlayer.VideoSource = videoSource;
            videoPlayer.Start();
        }

        //获取图片
        private void Capture_Click(object sender, RoutedEventArgs e)
        {
            if (videoSource.IsRunning)
            {
                Bitmap bm = videoPlayer.GetCurrentVideoFrame();
                bm.Save("test.jpg");

                BitmapImage myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri(System.Windows.Forms.Application.StartupPath + "\\test.jpg");
                myBitmapImage.DecodePixelWidth = 40;
                myBitmapImage.EndInit();
                System.Windows.Controls.Image tmpImg = thumbnaillist[currFileIndex] as System.Windows.Controls.Image;
                tmpImg.Source = myBitmapImage;
                tmpImg.Visibility = Visibility.Visible;
                Label tmpLb = lablelist[currFileIndex] as Label;
                tmpLb.Visibility = Visibility.Hidden;
                currfileName.Content = tmpLb.Content;
            }
        }

        //缩略图点击事件
        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canvas.Visibility == Visibility.Visible)
            {
                canvas.Visibility = Visibility.Collapsed;
                wfhost.Visibility = Visibility.Visible;
            }
            else
            {
                
                wfhost.Visibility = Visibility.Collapsed;
                canvas.Visibility = Visibility.Visible;
                BitmapImage myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri(System.Windows.Forms.Application.StartupPath + "\\test.jpg");
                myBitmapImage.EndInit();
                canvas.Source = myBitmapImage;
                currfileName.Content = (sender as System.Windows.Controls.Image).Name;//显示文件名
            }
        }

        //安全关闭视频
        private void CloseCurrentVideoSource()
        {
            if (videoPlayer.VideoSource != null)
            {
                videoPlayer.SignalToStop();

                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!videoPlayer.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (videoPlayer.IsRunning)
                {
                    videoPlayer.Stop();
                }

                videoPlayer.VideoSource = null;
            }
        }

        //private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        //{
        //    // get new frame
        //    Bitmap bitmap = eventArgs.Frame;
        //    // process the frame
        //}

        //定义采集项信息
        private class ScannningFile
        {
            public int image_no;
            public string fileName;
            public int index;
            public int videorpic;//视频(0)还是图像(1)
            public string videosourceName;
            public int resolution;

            public ScannningFile(int image_no,string fileName,int index,int videorpic, string videosourceName, int resolution)
            {
                this.image_no = image_no;
                this.fileName = fileName;
                this.index = index;
                this.videorpic = videorpic;
                this.videosourceName = videosourceName;
                this.resolution = resolution;
            }
        }

        //滑轮响应事件
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //int next = (currFileIndex + 1) % fileList.Count();
            //ScannningFile nextFile = fileList[next];
            //if ((nextFile.videorpic == 0)&&(!imageMemorys.ContainsKey(nextFile.image_no)))//切换到视频
            //{
            //    canvas.Visibility = Visibility.Collapsed;
            //    wfhost.Visibility = Visibility.Visible;
            //    currfileName.Content = nextFile.fileName;
            //}else
            //{
            //    System.Windows.Controls.Image tmpImg = bottomlist[next] as System.Windows.Controls.Image;
            //    image_MouseLeftButtonDown(tmpImg, null);//触发点击事件
            //}
        }

    }
}
