using AForge.Controls;
using AForge.Imaging.Filters;
using AForge.Imaging.Formats;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using System.Xml;
using System.Xml.Schema;

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
        
        private List<ScanImage> fileList = new List<ScanImage>();//采集项列表
        private int currFileIndex = 0;//选中的采集项索引，默认第一项
        private List<System.Windows.Controls.Image> thumbnaillist = new List<System.Windows.Controls.Image>();//底部缩略图列表
        private List<Label> lablelist = new List<Label>();//底部缩略图列表
        private Dictionary<int, List<Bitmap>> imageMemorys = new Dictionary<int, List<Bitmap>>();//所有图像缓存，<image_no,bitmaplist>
        private BitmapImage myBitmapImage = new BitmapImage();//选中的图像缓存

        ScanIni scanIni;
        string url;//请求xml的主机
        string token;//获取xml的key
        string singlePath = System.Windows.Forms.Application.StartupPath + "\\ScanSinglePr";
        ScanInfo scanInfo = new ScanInfo(); //xml对应实体类
        ScanDownload scanDownload = new ScanDownload();//下载类
        Boolean errorOccurs=false;//发生异常标志

        public Mix()
        {
            InitializeComponent();
            //初始化扫描文件
            //fileList.Add(new ScanImage(11, "证券账户申请表",0,1, "Integrated Webcam", 0));
            //fileList.Add(new ScanImage(11, "注销证券账户申请表",1,1,"Integrated Webcam", 1));
            //fileList.Add(new ScanImage(11, "test", 2, 0, "Integrated Webcam", 2));

            this.url = url.Substring(1, url.Length - 2);//去掉前后的双引号
            this.token = token.Substring(1, token.Length - 2);//去掉前后的双引号
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //下载并加载xml数据
            string xmlstr = DownloadFile(scanIni, url + "/arch/archentry/getXmlByKey.json?key=" + token, @"d:\1.xml", 's');//http://192.168.54.90:8080/HSEAP/arch/archentry/getXmlByKey.json?key=611898230
            //string xmlstr = "<ScanImageList count=\"2\" branchNo=\"8888\" custCode=\"01831922935\" businessArchId=\"201803190000000008\"><TImage index = \"0\" archImageNo = \"03\" imageName =\"休眠账户激活申请表\" imageSet = \"2\" imageDpi = \"200\" imageType = \"1\" haveScan = \"1\" fileSize = \"16\" lowFileSize = \"0\" filePath = \" \" importLocalFlag = \"0\" datewaterFlag = \"0\" pageNum = \"1\" /><TImage index = \"1\" archImageNo = \"05\" imageName = \"注销证券账户申请表\" imageSet = \"3\" imageDpi = \"200\" imageType = \"1\" haveScan = \"1\" fileSize = \"60\" lowFileSize = \"0\" filePath = \" \" importLocalFlag = \"1\" datewaterFlag = \"0\" pageNum = \"1\" /></ScanImageList > ";
            if (!LoadXMLData(xmlstr) || (fileList.Count == 0))
            {
                errorOccurs = true;//置异常标志
                Close();
            }

            //下载图片，可能需要完善
            scanDownload.scanInfo = scanInfo;
            scanDownload.scanIni = scanIni;
            if (scanDownload.needDown())
                scanDownload.downImage();

            init();//初始化
            getVideoIndex(fileList[currFileIndex].videosourceName);
            if (videoIndex == -1)
                videoIndex = 0;
            videoMenu.SelectedIndex = videoIndex;
        }

        //初始化界面
        private void init()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
            {
                System.Windows.MessageBox.Show("无可用视频输入设备，程序自动结束...");
                //
            }
            //枚举所有输入设备名称
            //for (int i = 0; i < videoDevices.Count; i++)
            foreach (FilterInfo device in videoDevices)
            {
                //FilterInfo device = videoDevices[i];
                videoMenu.Items.Add(device.Name);
                //MenuItem tmpMI = new MenuItem();
                //tmpMI.Header = device.Name;
                //videoMenu.Items.Add(tmpMI);
            }
            //绘制底部界面
            foreach (ScanImage scanfile in fileList)
            {
                Grid smallPic = new Grid();
                System.Windows.Controls.Image tmpImg = new System.Windows.Controls.Image();
                tmpImg.Name = scanfile.fileName;
                tmpImg.Height = 40;
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
                smallPic.MouseLeftButtonDown += image_MouseLeftButtonDown;
                smallPic.Name = scanfile.fileName;
                smallPic.Children.Add(tmpLb);
                bottom_image.Children.Add(smallPic);
            }
        }

        //切换视频源
        private void videoMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            getDevice(currFileIndex, videoMenu.SelectedItem.ToString());
            goPikachu();
            fileList[currFileIndex].videosourceName = videoMenu.SelectedItem.ToString();//置摄入设备名称，切换和保存 
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
                //缩略图
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
                //显示图片
                showImage(System.Windows.Forms.Application.StartupPath + "\\test.jpg");
                //切换界面
                switchwindow(0);
            }
        }
        //显示图片及信息
        private void showImage(string filepath)
        {
            try
            {
                ImageInfo imageInfo = null;//图片信息
                canvas.Image = ImageDecoder.DecodeFromFile(filepath, out imageInfo);
            }
            catch (NotSupportedException ex)
            {
                System.Windows.MessageBox.Show("Image format is not supported: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentException ex)
            {
                System.Windows.MessageBox.Show("Invalid image: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                System.Windows.MessageBox.Show("Failed loading the image", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //切换界面，0：从视频切换到图像，1：从图像切换到视频
        private void switchwindow(int action)
        {
            if (action == 0)
            {
                wfhost.Visibility = Visibility.Hidden;
                canvashost.Visibility = Visibility.Visible;
                zoomasc.Visibility = Visibility.Visible;
                zoomdesc.Visibility = Visibility.Visible;
            }else
            {
                wfhost.Visibility = Visibility.Visible;
                canvashost.Visibility = Visibility.Hidden;
                zoomasc.Visibility = Visibility.Hidden;
                zoomdesc.Visibility = Visibility.Hidden;
            }
        }

        //缩略图点击事件
        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Grid).Name == currfileName.Content.ToString())//自我切换
            {
                if (canvashost.Visibility == Visibility.Visible)
                    switchwindow(1);
                else
                {
                    showImage(System.Windows.Forms.Application.StartupPath + "\\test.jpg");
                    //currfileName.Content = (sender as System.Windows.Controls.Image).Name;//显示文件名
                    switchwindow(0);
                }
            }else
            {
                if((nextFile.videorpic == 0) && (!imageMemorys.ContainsKey(nextFile.image_no))
            }
            //if()
            if (canvashost.Visibility == Visibility.Visible)
            {
                
                switchwindow(1);
            }
            else
            {
                showImage(System.Windows.Forms.Application.StartupPath + "\\test.jpg");
                //currfileName.Content = (sender as System.Windows.Controls.Image).Name;//显示文件名
                switchwindow(0);               
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


        //滑轮响应事件
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //int next = (currFileIndex + 1) % fileList.Count();
            //ScanImage nextFile = fileList[next];
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

        // 过滤器
        private void ApplyFilter(IFilter filter)
        {
            //ClearCurrentImage();
            // apply filter
            Bitmap sourceimage = new Bitmap(System.Windows.Forms.Application.StartupPath + "\\test.jpg");
            Bitmap filteredImage = filter.Apply(sourceimage);
            //canvas.Source =BitmapToBitmapImage(filteredImage);
            // display filtered image
            //pictureBox.Image = filteredImage;
        }

        int zoom = 0;
        BrightnessCorrection a = new BrightnessCorrection();
        private void zoomasc_Click(object sender, RoutedEventArgs e)
        {
            a.AdjustValue = (++zoom);
            ApplyFilter(a);
        }

        private void zoomdesc_Click(object sender, RoutedEventArgs e)
        {

        }

        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            Bitmap bitmapSource = new Bitmap(bitmap.Width, bitmap.Height);
            int i, j;
            for (i = 0; i < bitmap.Width; i++)
                for (j = 0; j < bitmap.Height; j++)
                {
                    System.Drawing.Color pixelColor = bitmap.GetPixel(i, j);
                    System.Drawing.Color newColor = System.Drawing.Color.FromArgb(pixelColor.R, pixelColor.G, pixelColor.B);
                    bitmapSource.SetPixel(i, j, newColor);
                }
            MemoryStream ms = new MemoryStream();
            bitmapSource.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(ms.ToArray());
            bitmapImage.EndInit();

            return bitmapImage;
        }

        //业务数据------------------------------------------------------------------------------------------------------------------------
        //配置文件类
        public class ScanIni
        {
            public IniFile iniFile;
            public string IniPath;
            public Boolean VideoCutFrame;
            public int caRight;
            public int XDotsPerinch;
            public int BFParam;
            public int ScanType;
            public int caTop;
            public int Compression;
            public int MaxImgWidth;
            public int Rotary;
            public int MaxImgHeight;
            public int ColorFormat;
            public int caBottom;
            public int caLeft;
            public int Zoom;
            public int YDotsPerinch;
            public string Image_no;
            public string VideoDevice;
            public string FileCaption;
            public string FileHint;
            public int ImageSource;
            public Boolean SaveParams;
            public int CaPercent;
            public int CutFreamKind;
            public Double YZoomNum;
            public Double XZoomNum;
            public int FrameTop;
            public int FrameBottom;
            public int FrameLeft;
            public int FrameRight;
            public string UpLoadUrl;
            public string DownLoadUrl;
            public string SnapUploadUrl;
            public string DownImageUrl;
            public string VideoMediaType;
            public Boolean ShowScanUI;
            public Boolean IsDebug;
            public Boolean IsCheckPageCount;
            public string AppPath;
            //日志根目录
            public string LogPath;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="appPath">基本路径</param>
            public ScanIni(string appPath)
            {
                this.AppPath = appPath;
                LogPath = appPath + "\\log\\";
                IniPath = appPath + "\\ScanConfig.ini";
                this.iniFile = new IniFile(IniPath);//创建工具类

                //读取公用配置
                this.IsDebug = this.iniFile.ReadBool("SYS", "Debug", true);
                if (this.IsDebug)
                {
                    if (!Directory.Exists(LogPath))
                    {
                        try
                        {
                            Directory.CreateDirectory(LogPath);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("创建日志目录失败：", "提示", MessageBoxButton.OK);
                        }
                    }
                }
                this.IsCheckPageCount = this.iniFile.ReadBool("SYS", "IsCheckPageCount", false);
            }

            public ScanIni()
            {
            }

            public void writeLog(string strLog)
            {
                if (IsDebug)
                {
                    string logFile = AppPath + "\\log\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    FileStream fs;
                    StreamWriter sw;
                    if (File.Exists(logFile))
                    //验证文件是否存在，有则追加，无则创建
                    {
                        fs = new FileStream(logFile, FileMode.Append, FileAccess.Write);
                    }
                    else
                    {
                        fs = new FileStream(logFile, FileMode.Create, FileAccess.Write);
                    }
                    sw = new StreamWriter(fs);
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + strLog);
                    sw.Close();
                    fs.Close();
                }
            }
            public void setIniFile(IniFile iniFile)
            {
                this.iniFile = iniFile;
            }
            public void ReadConfig(string Image_no)
            {
                ImageSource = iniFile.ReadInt(Image_no, "ImageSource", 1);
                VideoCutFrame = iniFile.ReadBool(Image_no, "VideoCutFrame", true);
                CutFreamKind = iniFile.ReadInt(Image_no, "CutFreamKind", 0);
                FileCaption = iniFile.ReadString(Image_no, "FileCaption", "未知图片类型:" + Image_no);
                FileHint = iniFile.ReadString(Image_no, "FileHint", "未知图片类型:" + Image_no);
                //蓝色取景框大小
                caLeft = iniFile.ReadInt(Image_no, "caLeft", 49);
                caTop = iniFile.ReadInt(Image_no, "caTop", 10);
                caRight = iniFile.ReadInt(Image_no, "caRight", 631);
                caBottom = iniFile.ReadInt(Image_no, "caBottom", 441);
                Rotary = iniFile.ReadInt(Image_no, "Rotary", 0);
                Zoom = iniFile.ReadInt(Image_no, "Zoom", 30);//默认显示缩放比率  80-->30
                VideoDevice = iniFile.ReadString(Image_no, "VideoDevice", "NewImage SuperCam");//设置默认的采集设备为多易拍
                VideoMediaType = iniFile.ReadString(Image_no, "VideoMediaType", "");
                BFParam = iniFile.ReadInt(Image_no, "BFParam", 87);
                Compression = iniFile.ReadInt(Image_no, "Compression", 1024);
                MaxImgWidth = iniFile.ReadInt(Image_no, "MaxImgWidth", 9999);
                MaxImgHeight = iniFile.ReadInt(Image_no, "MaxImgHeight", 9999);
                SaveParams = iniFile.ReadBool(Image_no, "SaveParams", false);
                CaPercent = iniFile.ReadInt(Image_no, "CaPercent", 75);
                ShowScanUI = iniFile.ReadBool(Image_no, "ShowScanUI", true);//默认显示扫描仪UI页面
            }
            public void ReadDocConfig()
            {
                FrameLeft = iniFile.ReadInt("DocExpress", "FrameLeft", 40);
                FrameTop = iniFile.ReadInt("DocExpress", "FrameTop", 19);
                FrameRight = iniFile.ReadInt("DocExpress", "FrameRight", 631);
                FrameBottom = iniFile.ReadInt("DocExpress", "FrameBottom", 441);
            }
            public void SaveConfig(string Image_no)
            {
                iniFile.WriteInt(Image_no, "ImageSource", ImageSource);
                iniFile.WriteBool(Image_no, "VideoCutFrame", VideoCutFrame);
                iniFile.WriteInt(Image_no, "CutFreamKind", CutFreamKind);
                iniFile.WriteString(Image_no, "FileCaption", FileCaption);
                iniFile.WriteString(Image_no, "FileHint", FileHint);
                iniFile.WriteInt(Image_no, "caLeft", caLeft);
                iniFile.WriteInt(Image_no, "caTop", caTop);
                iniFile.WriteInt(Image_no, "caRight", caRight);
                iniFile.WriteInt(Image_no, "caBottom", caBottom);
                iniFile.WriteInt(Image_no, "Rotary", Rotary);
                iniFile.WriteInt(Image_no, "Zoom", Zoom);
                iniFile.WriteString(Image_no, "VideoDevice", VideoDevice);
                iniFile.WriteString(Image_no, "VideoMediaType", VideoMediaType);
                iniFile.WriteInt(Image_no, "BFParam", BFParam);
                iniFile.WriteInt(Image_no, "Compression", Compression);
                iniFile.WriteInt(Image_no, "MaxImgWidth", MaxImgWidth);
                iniFile.WriteInt(Image_no, "MaxImgHeight", MaxImgHeight);
                iniFile.WriteBool(Image_no, "SaveParams", SaveParams);
                iniFile.WriteInt(Image_no, "CaPercent", CaPercent);
            }
            public void SaveDocConfig()
            {
                iniFile.WriteInt("DocExpress", "FrameLeft", FrameLeft);
                iniFile.WriteInt("DocExpress", "FrameTop", FrameTop);
                iniFile.WriteInt("DocExpress", "FrameRight", FrameRight);
                iniFile.WriteInt("DocExpress", "FrameBottom", FrameBottom);
            }
        }
        //读取配置文件到类
        private Boolean LoadXMLData(string xmlstr)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlstr);
            //通过defaul.xsd验证验证xml格式是否正确  
            string error = "";
            XmlSchemaSet schemas = new XmlSchemaSet();//声明XmlSchema
            schemas.Add("", XmlReader.Create(singlePath + "\\defaul.xsd"));
            ValidationEventHandler eventHandler = new ValidationEventHandler(delegate (object sender, ValidationEventArgs e) {//声明事件处理方法 
                switch (e.Severity)
                {
                    case XmlSeverityType.Error:
                        error += e.Message;
                        break;
                    case XmlSeverityType.Warning:
                        break;
                }
            });
            xmldoc.Schemas = schemas;
            //验证xml 
            xmldoc.Validate(eventHandler);
            if (!"".Equals(error))
            {
                MessageBox.Show("XML验证失败:" + error, "提示", MessageBoxButton.OK);
            }
            try
            {
                xmldoc.LoadXml(xmlstr);//加载xml文档
            }
            catch (Exception e)
            {
                MessageBox.Show("加载xml数据错误：", "提示", MessageBoxButton.OK);
                return false;
            }
            //HsMessageBox.Show(xmlstr);
            //begin 解析xml,转为对象列表--------------------------------------------------------------------------------
            int count = int.Parse(xmldoc.DocumentElement.Attributes["count"].Value);//采集项个数
            scanInfo.branchNo = int.Parse(xmldoc.DocumentElement.Attributes["branchNo"].Value);//分支机构
            scanInfo.clientId = "8888";//客户号
            if (xmldoc.DocumentElement.Attributes.GetNamedItem("custCode") != null)
            {
                scanInfo.clientId = xmldoc.DocumentElement.Attributes["custCode"].Value;
            }
            scanInfo.businessArchId = "";//档案编号
            if (xmldoc.DocumentElement.Attributes.GetNamedItem("businessArchId") != null)
            {
                scanInfo.businessArchId = xmldoc.DocumentElement.Attributes["businessArchId"].Value;
            }
            scanInfo.DirPath = singlePath + "\\image\\" + scanInfo.branchNo + "\\" + scanInfo.clientId + "\\";
            //加载到list
            for (int i = 0; i < count; i++)
            {
                XmlNode node = xmldoc.DocumentElement.ChildNodes.Item(i);
                ScanImage scanImage = new ScanImage();
                scanImage.image_no = node.Attributes["archImageNo"].Value;//文件编号
                if (scanImage.image_no == null) { scanImage.image_no = ""; }
                scanImage.fileName = node.Attributes["imageName"].Value;//文件名称
                if (scanImage.fileName == null) { continue; }
                scanImage.image_set = node.Attributes["imageSet"].Value[0];                //图片处理类型:image_set
                scanImage.image_type = node.Attributes["imageType"].Value[0];                //影像类型:image_type
                scanImage.image_dpi = int.Parse(node.Attributes["imageDpi"].Value);                //图像DPI:image_dpi
                scanImage.file_size = int.Parse(node.Attributes["fileSize"].Value);                //文件大小:file_size
                scanImage.lower_file_size = int.Parse(node.Attributes["lowFileSize"].Value);                //文件大小下限:low_file_size
                scanImage.have_scan = node.Attributes["haveScan"].Value[0];                //是否必扫:have_scan
                scanImage.file_path = node.Attributes["filePath"].Value;                //file_path
                if (scanImage.file_path == null) { scanImage.file_path = ""; }
                scanImage.import_local_flag = node.Attributes["importLocalFlag"].Value[0];                //导入本地图片标志
                scanImage.datewater_flag = node.Attributes["datewaterFlag"].Value[0];                //是否加时间水印戳
                scanImage.page_num = int.Parse(node.Attributes[("pageNum")].Value);                //采集页码数
                scanImage.index = int.Parse(node.Attributes["index"].Value);                //index
                string ext = "";
                if (scanImage.image_type != '2')
                    ext = ".tif";
                else
                    ext = ".jpg";
                if (scanImage.file_path == " ")
                {
                    scanImage.Local_path = scanInfo.DirPath + scanInfo.businessArchId + scanImage.image_no + ext;
                    if (File.Exists(scanImage.Local_path))
                    {
                        try
                        {
                            File.Delete(scanImage.Local_path);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("删除本地图片错误：" + scanImage.Local_path, "提示", MessageBoxButton.OK);
                            return false;
                        }
                    }
                }
                else
                {
                    scanImage.Local_path = scanImage.file_path;
                }
                scanImage.postfix = ext;

                //路径为空创建目录
                if (!Directory.Exists(singlePath + "\\image\\" + scanInfo.branchNo + "\\" + scanInfo.clientId + "\\"))
                {
                    try
                    {
                        Directory.CreateDirectory(singlePath + "\\image\\" + scanInfo.branchNo + "\\" + scanInfo.clientId + "\\");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("创建本地目录失败：" + singlePath + "\\image\\" + scanInfo.branchNo + "\\" + scanInfo.clientId + "\\", "提示", MessageBoxButton.OK);
                        return false;
                    }
                }
                //scanImage.IsUpload = true;
                fileList.Add(scanImage);
            }
            scanInfo.scanningFileList = fileList;//注入scanInfo中
            return true;
        }
        //xml对应实体类
        public class ScanInfo
        {
            //老档案可能
            public string AuthCode;
            public string scanFlag;
            public string postfix;
            public string positionStr;
            public string scantaskId;
            //新档案属性
            //public string imageNo;
            public int branchNo;//用于创建文件夹
            public string clientId;//用于创建文件夹
            public string businessArchId;//用于下载文件
            public List<ScanImage> scanningFileList;
            public string DirPath;//图片保存的存储目录,appPath\image\branchNo\clientId\
        }
        //具体采集项/扫描文件类
        public class ScanImage
        {
            public string image_no;
            public string fileName;
            public int index;
            public int videorpic;//视频(0)还是图像(1)
            public string videosourceName;
            public int resolution;

            public char image_set;//颜色
            public char image_type;//采集项类型
            public int image_dpi;
            public int file_size;//质检
            public int lower_file_size;
            public Char import_local_flag;
            public Char datewater_flag;
            public Char have_scan;
            public int page_num;
            public string postfix;//图片后缀名
            public string file_path;//下载路径，图片在服务器上的完整URL
            public string Local_path;//本地存储绝对路径，=scaninfo.DirPath\scanInfo.businessArchId + scanImage.image_no+".ext"
            //public Boolean IsUpload;//可以利用这个判断有没有图片

            public ScanImage()
            {

            }
            //public ScanImage(string image_no, string fileName, int index, int videorpic, string videosourceName, int resolution)
            //{
            //    this.image_no = image_no;
            //    this.fileName = fileName;
            //    this.index = index;
            //    this.videorpic = videorpic;
            //    this.videosourceName = videosourceName;
            //    this.resolution = resolution;
            //}
        }
        public class ScanDownload
        {
            //不明curUploadIndex
            public ScanInfo scanInfo;
            public ScanIni scanIni;
            public ScanDownload()
            {
            }
            public string getImageID(string filePath)
            {
                int i = filePath.LastIndexOf("/");
                return filePath.Substring(i + 1, filePath.Length - i - 1);
            }
            public Boolean downImage()
            {
                Boolean result = true;//默认下载完成
                //临时变量
                ScanImage tmpScanImage = null;
                string imageID = "";
                string fileExt = "";//图片后缀
                string fileName = "";//本地路径
                string filePath = "";
                string url = "";
                
                for (int i = 0; i < scanInfo.scanningFileList.Count; i++)
                {
                    tmpScanImage = scanInfo.scanningFileList[i];
                    filePath = tmpScanImage.file_path;
                    //本地有不下载
                    if (File.Exists(filePath)) continue;
                        if ((filePath.ToUpper().IndexOf(".JPG") > -1) || (filePath.ToUpper().IndexOf(".TIF") > -1))
                        {
                            imageID = getImageID(filePath);
                            if (imageID.Trim() != "")
                            {
                                fileExt = filePath.Substring(filePath.Length - 4);
                                fileName = scanInfo.DirPath + scanInfo.businessArchId + tmpScanImage.image_no + fileExt;
                                tmpScanImage.Local_path = fileName;
                                scanIni.writeLog("DownImage.image_name=" + tmpScanImage.fileName + ";FileName=" + fileName);
                                try
                                {
                                    if (File.Exists(fileName))
                                        File.Delete(fileName);
                                }
                                catch (Exception e)
                                {
                                    result = false;
                                    //不明 btnReset.Visible := True;
                                    //pnlFilesTitle.Caption := '下载文件[' + ASourceImage.image_name + '] 失败';
                                    MessageBox.Show("下载图片时删除本地空图片发生异常:" + e.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Warning, new MessageBoxResult());
                                }
                                if (!File.Exists(fileName))
                                {
                                    try
                                    {
                                        if (!Directory.Exists(scanInfo.DirPath))
                                        {
                                            Directory.CreateDirectory(scanInfo.DirPath);
                                        }
                                        url = filePath;
                                        scanIni.writeLog("DownImage: url=" + url);
                                    }
                                    catch (Exception e)
                                    {
                                        result = false;
                                        MessageBox.Show("下载文件[" + tmpScanImage.fileName + "] 失败(创建目录失败,路径:" + scanInfo.DirPath + ")", "异常", MessageBoxButton.OK, MessageBoxImage.Warning, new MessageBoxResult());
                                    }
                                    //下载图片
                                    DownloadFile(scanIni, url, fileName, 'f');
                                }
                            }
                        }
                }
                return result;
            }
            public Boolean needDown()
            {
                ScanImage tmpScanImage = null;
                string imageID = "";
                string fileExt = "";//图片后缀
                string fileName = "";//本地路径
                string filePath = "";
                for (int i = 0; i < scanInfo.scanningFileList.Count; i++)
                {
                    tmpScanImage = scanInfo.scanningFileList[i];
                    filePath = tmpScanImage.file_path;
                        if ((filePath.ToUpper().IndexOf(".JPG") > -1) || (filePath.ToUpper().IndexOf(".TIF") > -1))
                        {
                            imageID = getImageID(filePath);
                            if (imageID.Trim() != "")
                            {
                                fileExt = filePath.Substring(filePath.Length - 4);
                                fileName = scanInfo.DirPath + scanInfo.businessArchId + tmpScanImage.image_no + fileExt;
                                tmpScanImage.Local_path = fileName;
                                scanIni.writeLog("NeedDown: FileName=" + fileName);
                                //本地有需更新
                                if ((!File.Exists(tmpScanImage.file_path)) && (File.Exists(fileName)))
                                    return true;
                                if ((!File.Exists(fileName)) || (GetFileSize(fileName) == "0"))
                                    return true;
                            }
                        }
                }
                return false;
            }
        }
        //下载文件
        public static string DownloadFile(ScanIni scanIni, string URL, string filename, char flag)
        {
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                Stream responseStream = myrp.GetResponseStream();
                //下载xmlstr
                if (flag == 's')
                {
                    scanIni.writeLog("GetFileXMl.DownLoadUrl:" + URL);
                    //StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
                    StreamReader reader = new StreamReader(responseStream);
                    string strReturn = reader.ReadToEnd();
                    scanIni.writeLog("GetFileXMl.xmlstr =" + strReturn);
                    return strReturn;
                }
                //下载文件
                else if (flag == 'f')
                {
                    Stream so = new FileStream(filename, System.IO.FileMode.Create);
                    long totalDownloadedByte = 0;
                    byte[] by = new byte[1024];
                    int osize = responseStream.Read(by, 0, (int)by.Length);
                    while (osize > 0)
                    {
                        totalDownloadedByte = osize + totalDownloadedByte;
                        so.Write(by, 0, osize);
                        osize = responseStream.Read(by, 0, (int)by.Length);
                    }
                    so.Close();
                    responseStream.Close();
                    return "success";
                }
                return "failed";
            }
            catch (System.Exception)
            {
                MessageBox.Show("下载文件失败", "警告", MessageBoxButton.OK);
                return "";
            }
        }
        //获取文件大小
        public static string GetFileSize(string fileName)
        {
            if (!File.Exists(fileName))
            {
                MessageBox.Show("获取图片大小：" + fileName + "文件不存在", "提示", MessageBoxButton.OK);
                return "0";
            }
            FileInfo fi = new FileInfo(fileName);
            return fi.Length.ToString();
        }
    }
}
