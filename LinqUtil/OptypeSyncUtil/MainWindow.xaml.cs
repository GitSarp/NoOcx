using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace OptypeSyncUtil
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //string tmpfile = System.Windows.Forms.Application.StartupPath + "\\scanoptype.xml";//源文件
        string tmpfile = "";//源文件
        string targetfile = "";//目标文件
        string[] optypestr= { };//操作类型串
        string dateNow;//时间串

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if(optypes.Text=="" || moder.Text=="" || modor.Text == "")
            {
                System.Windows.MessageBox.Show("请填写完整...");
                return;
            }
            if(!File.Exists(tmpfile) || !File.Exists(targetfile))
            {
                System.Windows.MessageBox.Show("请选择正确的目录...");
                return;
            }

            optypestr = optypes.Text.Replace(' ', ',').Replace('、', ',').Split(new char[] { ',' });
            XDocument sdoc = XDocument.Load(tmpfile);
            File.SetAttributes(targetfile, FileAttributes.Normal);
            XDocument tdoc = XDocument.Load(targetfile);
            //复制操作类型
            for (int i = 0; i < optypestr.Length; i++)
            {      
                var itemNode = sdoc.Root.Elements("item").Where(p => p.Attribute("sub_op_type").Value.Equals(optypestr[i])).First();
                itemNode.Add(new XAttribute("user_defined_name", itemNode.Attribute("dict_prompt").Value));
                tdoc.Element("hsdoc").Element("item").AddBeforeSelf(itemNode);
                //tdoc.Element("hsdoc").AddFirst(itemNode);
            }
            //创建日志
            dateNow = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            string oversion=tdoc.Root.Element("ModifyLog").Element("log").Attribute("version").Value;
            XElement log = new XElement("log",
                new XAttribute("position",optypes.Text),
                new XAttribute("date", dateNow),
                new XAttribute("version", addVersion(oversion)),
                new XAttribute("serialNumber", modor.Text),
                new XAttribute("user", moder.Text),
                new XAttribute("principal", ""),
                new XAttribute("cause", "新增档案操作类型"+ optypes.Text),
                new XAttribute("content", ""),
                new XAttribute("tester", "")
                );
            tdoc.Root.Element("ModifyLog").AddFirst(log);
            tdoc.Save(targetfile);
            System.Windows.MessageBox.Show("同步成功！");
        }

        //选择源文件
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //选择文件夹
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "指定文件|scanoptype.cdata";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //File.Copy(dialog.FileName, dialog.FileName + ".xml");
                //tmpfile = dialog.FileName + ".xml";
                tmpfile = dialog.FileName;
            }
        }

        //选择目标文件
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //选择文件夹
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "指定文件|archoptype.cdata";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                targetfile = dialog.FileName;
                System.Windows.MessageBox.Show("OK!");
            }
        }

        private string addVersion(string old)
        {
            int tmp = old.LastIndexOf('.');
            return old.Substring(0, tmp+1) + (int.Parse(old.Substring(tmp + 1)) + 1);
        }

        //选择源目录
        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            if (m_Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!File.Exists(m_Dialog.SelectedPath + "\\scanoptype.cdata"))
                {
                    System.Windows.MessageBox.Show("当前目录无scanoptype.cdata...");
                    return;
                }
                srcdirT.IsReadOnly = false;
                srcdirT.Text = m_Dialog.SelectedPath;
                srcdirT.IsReadOnly = true;
                tmpfile = m_Dialog.SelectedPath + "\\scanoptype.cdata";
            }  
        }
        //选择目标目录
        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            if (m_Dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!File.Exists(m_Dialog.SelectedPath + "\\archoptype.cdata"))
                {
                    System.Windows.MessageBox.Show("当前目录无archoptype.cdata...");
                    return;
                }
                tardirT.IsReadOnly = false;
                tardirT.Text = m_Dialog.SelectedPath;
                tardirT.IsReadOnly = true;
                targetfile = m_Dialog.SelectedPath + "\\archoptype.cdata";
            }
        }
        //读取配置
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            srcdirT.IsReadOnly = false;
            srcdirT.Text = ConfigurationManager.AppSettings["srcDir"];
            srcdirT.IsReadOnly = true;
            tardirT.IsReadOnly = false;
            tardirT.Text = ConfigurationManager.AppSettings["tarDir"];
            tardirT.IsReadOnly = true;
            tmpfile = ConfigurationManager.AppSettings["srcDir"] + "\\scanoptype.cdata";
            targetfile = ConfigurationManager.AppSettings["tarDir"] + "\\archoptype.cdata";
            
        }
        //保存配置
        private void Window_Closed(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["srcDir"].Value = srcdirT.Text;
            config.AppSettings.Settings["tarDir"].Value = tardirT.Text;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
