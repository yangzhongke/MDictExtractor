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
        private Thread thread;
        private string lastText;
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
                if(!User32.IsWindow(hwndMain))
                {
                    this.BeginInvoke(new Action(()=> {
                        MessageBox.Show("MDict已经退出");
                        btnExtract.Enabled = true;
                        btnStop.Enabled = false;
                    }));
                    break;
                }
                counter++;
                User32.SendMessage(hwndMain, User32.WindowMessage.WM_COMMAND,
                new IntPtr(1046), IntPtr.Zero);//【下一条】菜单项的id
                User32.SendMessage(hwndMain, User32.WindowMessage.WM_COMMAND,
                new IntPtr(1234), IntPtr.Zero);//【拷贝当前内容到粘贴板】菜单项的id
                string text=null;
                this.BeginInvoke(new Action(()=> {         
                    
                    text = Regex.Replace(Clipboard.GetText(), @"\s", "");//不能在子线程中访问Clipboard
                    if (lastText == text)//如果和上次一样，则不处理
                    {
                        return;
                    }
                    lastText = text;
                    File.AppendAllText("d:/data.txt", text + "\r\n");
                    labelCount.Text = counter.ToString();
                    labelMsg.Text = text;
                }) );
                
                Thread.Sleep(200);
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            if(File.Exists("d:/data.txt") && MessageBox.Show("请先备份d:/data.txt，否则将覆盖，是否继续？",
                "提示",MessageBoxButtons.YesNo)!= DialogResult.Yes)
            {
                return;
            }
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
