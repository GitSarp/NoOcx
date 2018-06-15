using AForge.Video.DirectShow;
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NOocx
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //static FilterInfoCollection videoDevices;//所有可用摄像头
        //private string currVideoName = "不可用";//当前摄像头名称，初始化不可用。
        //private VideoCaptureDevice videoSource;//当前摄像头
        //private int videoIndex = 0;//摄像头索引,用于选中菜单项
        //private int resolutionIndex = 0;//分辨率索引
        //private Dictionary<string, VideoCaptureDevice> videodatasource = new Dictionary<string, VideoCaptureDevice>();//存储所有摄像头实例

        //private List<ScanImage> fileList = new List<ScanImage>();//采集项列表
        //private int currFileIndex = 0;//选中的采集项索引，默认第一项
        //private List<System.Windows.Controls.Image> thumbnaillist = new List<System.Windows.Controls.Image>();//底部缩略图列表
        //private List<Label> lablelist = new List<Label>();//底部缩略图列表
        //private Dictionary<int, List<Bitmap>> imageMemorys = new Dictionary<int, List<Bitmap>>();//所有图像缓存，<image_no,bitmaplist>
        //private BitmapImage myBitmapImage = new BitmapImage();//选中的图像缓存

        //ScanIni scanIni;
        //string url;//请求xml的主机
        //string token;//获取xml的key
        //string singlePath = System.Windows.Forms.Application.StartupPath + "\\ScanSinglePr";
        //ScanInfo scanInfo = new ScanInfo(); //xml对应实体类
        //ScanDownload scanDownload = new ScanDownload();//下载类
        //Boolean errorOccurs = false;//发生异常标志

        public MainWindow()
        {
            InitializeComponent();

            //和业务太紧密了，一个简单的，应该是一个可以对接标准接口的工具
            //Mix mix = new Mix();
            //mix.Show();


            allImages.Add("D:\\2.jpg"); allImages.Add("D:\\2.jpg"); allImages.Add("D:\\2.jpg"); allImages.Add("D:\\2.jpg");
            allImages.Add("D:\\2.jpg"); allImages.Add("D:\\2.jpg"); allImages.Add("D:\\2.jpg"); allImages.Add("D:\\2.jpg");
            foreach (string tmp in allImages)
            {
                AddImg(tmp);
            }
        }

        public List<string> allImages = new List<string>();
        public void AddImg(string szPath)
        {
            System.Windows.Controls.Image listImage = new System.Windows.Controls.Image(); //创建一个Image控件
            Border br = new Border();
            BitmapImage bitmapImage = new BitmapImage();
            ListBoxItem newListBox = new ListBoxItem(); //创建一个ListBoxItem，作为ListBox的元素
            bitmapImage.BeginInit();
            //bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(szPath);
            bitmapImage.EndInit();
            //bitmapImage.Freeze();
            listImage.Source = bitmapImage;
            //设置border的高、宽、圆角
            br.Width = 106;
            br.Height = 106;
            br.CornerRadius = new CornerRadius(10);
            //Label PicLabel = new Label();//鼠标移到图片上显示图片的名称 
            //Image添加到Border中 
            br.Child = listImage;
            br.Padding = new System.Windows.Thickness((float)1.1f);
            br.Background = System.Windows.Media.Brushes.White;
            br.BorderThickness = new System.Windows.Thickness((int)3);

            newListBox.Content = br;
            newListBox.Margin = new System.Windows.Thickness((int)10);
            newListBox.DataContext = szPath;
            list1.Items.Add(newListBox); //list1为界面上ListBox控件的名称
            list1.SelectedIndex = list1.Items.Count - 1;
            scrolls.ScrollToRightEnd(); //使得滚动条 滚到最后， scrolls为ScrollViewer控件的名称
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
