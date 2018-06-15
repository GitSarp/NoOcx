using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOocx
{
    class reserveCode
    {
        //private VideoCapture _capture = null;
        //private Mat _frame;
        //private List<DsDevice> deviceList = new List<DsDevice>();//视频设备列表
        //private int deviceIndex = 0;//当前设备索引
        //private int resolutionIndex = 0;//当前分辨率索引
        //private static List<Point> allResolution = new List<Point>();//所有分辨率

        public void gouzao()
        {
            //    CvInvoke.UseOpenCL = false;
            //    try
            //    {
            //        _capture = new VideoCapture();
            //        _capture.ImageGrabbed += ProcessFrame;
            //    }
            //    catch (NullReferenceException excpt)
            //    {
            //        MessageBox.Show(excpt.Message);
            //    }
            //    _frame = new Mat();//视频桢

            //    //开启摄像头
            //    if (_capture != null)
            //    {
            //        _capture.Start();
            //    }

            //    GetVideoInputDevice();//获取所有摄像设备
            //    for (int i=0;i< deviceList.Count;i++)
            //    {
            //        MenuItem mi = new MenuItem();
            //        mi.Header = deviceList[i].Name;
            //        //保存配置
            //        if(i==deviceIndex)
            //            mi.IsChecked = true;
            //        //mi.Click += test;
            //        videoSource.Items.Add(mi);
            //    }
            //    GetAllAvailableResolution(deviceList[deviceIndex]);
            //    for (int i = 0; i < allResolution.Count; i++)
            //    //foreach (var tmp in GetAllAvailableResolution(deviceList[resolutionIndex]))
            //    {
            //        MenuItem mi = new MenuItem();
            //        mi.Header = allResolution[i].X+"x"+ allResolution[i].Y;
            //        //保存配置
            //        if (i == resolutionIndex)
            //            mi.IsChecked = true;
            //        //mi.Click += test;
            //        mediaType.Items.Add(mi);
            //        //保存配置
            //    }
            //    //MessageBox.Show(string.Join(",", GetVideoInputDevice().ToArray()));
            //    //MessageBox.Show(_capture.GetCaptureProperty(CapProp.FrameHeight) +"");
        }

        ////获取所有视频设备
        //public ArrayList GetVideoInputDevice()
        //{ return GetDeviceCollection(FilterCategory.VideoInputDevice); }
        //private ArrayList GetDeviceCollection(Guid DeviceType)
        //{
        //    ArrayList returnString = new ArrayList();
        //    foreach (DsDevice ds in DsDevice.GetDevicesOfCat(DeviceType))
        //    {
        //        //returnString.Add(ds.Name);
        //        deviceList.Add(ds);
        //    }
        //    return returnString;
        //}
        ////获取设备所有分辨率
        //public static List<Point> GetAllAvailableResolution(DsDevice vidDev)
        //{
        //    try
        //    {
        //        int hr;
        //        int max = 0;
        //        int bitCount = 0;

        //        IBaseFilter sourceFilter = null;

        //        var m_FilterGraph2 = new FilterGraph() as IFilterGraph2;

        //        hr = m_FilterGraph2.AddSourceFilterForMoniker(vidDev.Mon, null, vidDev.Name, out sourceFilter);

        //        var pRaw2 = DsFindPin.ByCategory(sourceFilter, PinCategory.Capture, 0);

        //        var AvailableResolutions = new List<Point>();

        //        VideoInfoHeader v = new VideoInfoHeader();
        //        IEnumMediaTypes mediaTypeEnum;
        //        hr = pRaw2.EnumMediaTypes(out mediaTypeEnum);

        //        AMMediaType[] mediaTypes = new AMMediaType[1];
        //        IntPtr fetched = IntPtr.Zero;
        //        hr = mediaTypeEnum.Next(1, mediaTypes, fetched);

        //        while (fetched != null && mediaTypes[0] != null)
        //        {
        //            Marshal.PtrToStructure(mediaTypes[0].formatPtr, v);
        //            if (v.BmiHeader.Size != 0 && v.BmiHeader.BitCount != 0)
        //            {
        //                if (v.BmiHeader.BitCount > bitCount)
        //                {
        //                    AvailableResolutions.Clear();
        //                    //
        //                    allResolution.Clear();
        //                    max = 0;
        //                    bitCount = v.BmiHeader.BitCount;


        //                }
        //                AvailableResolutions.Add(new Point(v.BmiHeader.Width, v.BmiHeader.Height));
        //                //
        //                allResolution.Add(new Point(v.BmiHeader.Width, v.BmiHeader.Height));
        //                if (v.BmiHeader.Width > max || v.BmiHeader.Height > max)
        //                    max = (Math.Max(v.BmiHeader.Width, v.BmiHeader.Height));
        //            }
        //            hr = mediaTypeEnum.Next(1, mediaTypes, fetched);
        //        }
        //        return AvailableResolutions;
        //    }

        //    catch (Exception ex)
        //    {
        //        //Log(ex);
        //        return new List<Point>();
        //    }
        //}

        ////显示视频

        //private void ProcessFrame(object sender, EventArgs arg)
        //{
        //    if (_capture != null && _capture.Ptr != IntPtr.Zero)
        //    {
        //        _capture.Retrieve(_frame, 0);
        //        captureImageBox.Image = _frame;
        //        //CvInvoke.CvtColor(_frame, _grayFrame, ColorConversion.Bgr2Gray);
        //        //CvInvoke.PyrDown(_grayFrame, _smallGrayFrame);
        //        //CvInvoke.PyrUp(_smallGrayFrame, _smoothedGrayFrame);
        //        //CvInvoke.Canny(_smoothedGrayFrame, _cannyFrame, 100, 60);
        //    }
        //}


        ////隐藏/显示菜单栏
        //private void menu_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    menubar.Visibility = Visibility.Visible;
        //}

        //private void menu_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    menubar.Visibility = Visibility.Collapsed;
        //}

        //private void menu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        //{
        //    menubar.Visibility = Visibility.Visible;
        //}

        //private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
        //    {
        //        if (menubar.Visibility != Visibility.Visible) menubar.Visibility = Visibility.Visible;
        //    }
        //}

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{

        //}

        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    if (_capture != null)
        //    {
        //        _capture.Stop();
        //        _capture.Dispose();
        //    }
        //}

        ////切换视频设备
        //private void videoSource_Click(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
