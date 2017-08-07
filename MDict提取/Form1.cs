using PInvoke;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace MDict提取
{
    public partial class Form1 : Form
    {
        Thread thread;
        public Form1()
        {
            InitializeComponent();
        }

        private void OnExtract(object hwndObj)
        {
            IntPtr hwndMain = (IntPtr)hwndObj;
            int counter = 0;
            while (true)
            {
                counter++;
                User32.SendMessage(hwndMain, User32.WindowMessage.WM_COMMAND,
                new IntPtr(1046), IntPtr.Zero);//【下一条】菜单项的id
                User32.SendMessage(hwndMain, User32.WindowMessage.WM_COMMAND,
                new IntPtr(1234), IntPtr.Zero);//【拷贝当前内容到粘贴板】菜单项的id
                string text=null;
                this.BeginInvoke(new Action(()=> {
                    text = Clipboard.GetText();//不能在子线程中访问Clipboard
                    text = Regex.Replace(text, @"\s", "");
                    File.AppendAllText("d:/data.txt", text + "\r\n");
                    labelCount.Text = counter.ToString();
                    labelMsg.Text = text;
                }) );
                
                Thread.Sleep(200);
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            IntPtr hwndMain = User32.FindWindow("MDictMainWnd", "MDict");
            if (hwndMain.ToInt32()==0)
            {
                MessageBox.Show("找不到MDict窗口");
                return;
            }
            File.Delete("d:/data.txt");
            thread = new Thread(new ParameterizedThreadStart(OnExtract));
            thread.Start(hwndMain);
            btnExtract.Enabled = false;
            btnStop.Enabled = true;            
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            thread.Abort();
            btnExtract.Enabled = true;
            btnStop.Enabled = false;
        }
    }
}
