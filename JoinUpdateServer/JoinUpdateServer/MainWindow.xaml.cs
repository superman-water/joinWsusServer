using System;
using System.Windows;
using Microsoft.Win32;
using System.ServiceProcess;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace JoinUpdateServer
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

        //此程序的主要任务，完成添加客户端电脑，加入到单位内部的wsus服务器，实现内部服务网统一更新补丁。
        //wangjd@2017
        private void joinUpdateServer(object sender, RoutedEventArgs e)
        {
            //join主要任务
            try
            {
                    RegistryKey key = Registry.LocalMachine;
                    key.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate");
                    key.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU");
                //创建需要的注册表路径，防止OpenSub
                    RegistryKey software = key.OpenSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate", true);
                    //该项必须已存在
                    software.SetValue("WUServer", "http://192.168.107.12", RegistryValueKind.String);
                    software.SetValue("WUStatusServer", "http://192.168.107.12", RegistryValueKind.String);
                    RegistryKey software2 = key.OpenSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU", true);
                   //该项必须已存在
                    software2.SetValue("AutoInstallMinorUpdates", "00000001", RegistryValueKind.DWord);
                    software2.SetValue("NoAutoUpdate", "0000000", RegistryValueKind.DWord);
                    software2.SetValue("AUOptions", "00000003", RegistryValueKind.DWord);
                    software2.SetValue("ScheduledInstallDay", "00000000", RegistryValueKind.DWord);
                    //software2.SetValue("ScheduledInstallTime", "00000003", RegistryValueKind.DWord);
                    software2.SetValue("UseWUServer", "00000001", RegistryValueKind.DWord);
                    software2.SetValue("DetectionFrequency", "00000016", RegistryValueKind.DWord);
                    software2.SetValue("DetectionFrequencyEnabled", "00000001", RegistryValueKind.DWord);
                //如果该键值原本已经存在，则会修改替换原来的键值，如果不存在则是创建该键值。
                SetServiceStartMode(2);
                //开启服务自动开启
                RestartService("wuauserv");
                Createbat("delme.bat");
                RunBat("c:\\delme.bat");
                MessageBox.Show("恭喜，加入服务器设置成功！");
                //重启更新服务
            }
                catch (Exception error)
                {
                //MessageBox.Show("sorry");
                //MessageBox.Show(error.ToString());

                }
            }
        public void RestartService(String name)
        {
            String Sname= name;
            try
            {


                ServiceController sc = new ServiceController(Sname);
                if ((sc.Status.Equals(ServiceControllerStatus.Stopped)) || (sc.Status.Equals(ServiceControllerStatus.StopPending)))
                 //服务是否已经停止。
                {

                    sc.Start();
                    //服务停止则启动

                }
                else
                {

                    //sc.Stop();
                    //pass
                }


                sc.Refresh();
            }
            catch (Exception)
            {
                MessageBox.Show("sorry 服务启动失败，请重启电脑。");
            }





        }
     
        public void SetServiceStartMode(int Mode)
        {
            //重新启动windows update服务
            try
            {
                string ServiceName = "wuauserv";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\services\" + ServiceName, true);
                key.SetValue("Start", Mode);
                key.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("已经正在运行。");
            }

        }


        private void Createbat(string filename)
        {
            string v_filepath;
            string s;
            v_filepath = "c:\\" + filename;
            // 判断 bat文件是否存在，如果存在先把文件删除
            if (System.IO.File.Exists(v_filepath))
                System.IO.File.Delete(v_filepath);
            s = @"gpupdate /force
reg delete HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate /v AccountDomainSid /f
reg delete HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate /v PingID /f
reg delete HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate /v
SusClientId /f
net stop wuauserv
del %SystemRoot%\SoftwareDistribution\*.* /S /Q
net start wuauserv
wuauclt /resetauthorization /detectnow
wuauclt.exe /downloadnow
wuauclt.exe /reportnow";

        File.WriteAllText(v_filepath, s, Encoding.Default);   
        //将s字符串的内容写入v_filepath指定的bat文件中。
        }

        private void RunBat(string filename)
        {
            //运行bat文件
            Process pro = new Process();
            FileInfo file = new FileInfo(filename);
            pro.StartInfo.WorkingDirectory = file.Directory.FullName;
            pro.StartInfo.FileName = filename;
            pro.StartInfo.CreateNoWindow = false;
            pro.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pro.Start();
            pro.WaitForExit();
            System.IO.File.Delete("c:\\deleme.bat");
            //删除bat文件
        }

    }
}
