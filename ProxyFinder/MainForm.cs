﻿using System;
using System.Windows.Forms;

using ProxyFinder.sites;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProxyFinder
{
    public partial class MainForm : Form
    {
        public const Boolean isDebug = false;

        private Site[] mySites = new Site[]{    //在这里加入新写的继承site的类
                new _216_site(),  
                new qqbb_site(),
                new yunproxy_site(),
                //new Cnproxy_site(),
                //new Samair_site(),
                //new Proxyworld_site(),
                new myplantblog_site(),
                new kingproxies_site(),
                new eemmuu_site(),
                new qianggeEmail(),
            };

        private List<Proxy> all_me_proxys_list = new List<Proxy>();
        private List<Site> checked_sites = new List<Site>();

        private List<Proxy> all_proxys = new List<Proxy>();
        
        /// <summary>
        /// 保存已存在的代理（不包含端口，所以同一个ip，不同端口也会被认为重复）
        /// </summary>
        private List<String> all_string_proxys = new List<string>();

        private Boolean isProxyJudgerUrlFileExist = true;
        private int judgedCount = 0;

        public MainForm()
        {
            InitializeComponent();

            System.Net.ServicePointManager.DefaultConnectionLimit = 512;

            checkedListBox2.SetItemCheckState(1, CheckState.Checked);
            checkedListBox2.SetItemCheckState(2, CheckState.Checked);

            foreach (Site site in mySites)
            {
                ListViewItem item = new ListViewItem();
                item.SubItems.Add(site.getSiteName());
                item.SubItems.Add("");
                item.Tag = site;
                item.Checked = true;
                if (site.getSiteName() == "Samair")
                {
                    item.Checked = false;
                }
                listView1.Items.Add(item);
                if (isDebug)
                {
                    item.Checked = false;
                }
            }
            if (isDebug)
            {
                textBox3.Text = "C:\\me.asp";
                textBox4.Text = "C:\\person.html";
            }
            checkBox2.Checked = true;
            timer1.Start();
        }

        private delegate void onJudgeDone_dg();
        private void onJudgeDone()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new onJudgeDone_dg(onJudgeDone));
            } 
            else
            {
                judgedCount ++;
                progressBar1.Value = judgedCount;
                label1.Text = "验证中（" + judgedCount.ToString() + "/" + all_proxys.Count.ToString() + "）";
                if (judgedCount >= all_proxys.Count)
                {
                    label1.Text = "验证完成";
                    String file_path = textBox4.Text;
                    FileStream fs_http = new FileStream(file_path, FileMode.Create);
                    StreamWriter sw_http = new StreamWriter(fs_http, Encoding.UTF8);
                    foreach (Proxy p in all_proxys)
                    {
                        if (checkedListBox2.GetItemChecked(0) && p.level == eProxyLevel.TRANSPARENT)
                        {
                            sw_http.Write(p.toPersonString() + "\n");
                        }
                        else if (checkedListBox2.GetItemChecked(1) && p.level == eProxyLevel.ANONYMOUS)
                        {
                            sw_http.Write(p.toPersonString() + "\n");
                        }
                        else if (checkedListBox2.GetItemChecked(2) && p.level == eProxyLevel.HIGH_ANONYMITY)
                        {
                            sw_http.Write(p.toPersonString() + "\n");
                        }
                        
                    }
                    sw_http.Close();
                    button1.Enabled = true;
                    listView1.Enabled = true;
                    timer1.Start();
                }
            }
        }

        private delegate void onFind_dg(Boolean isSuccess, Site site);

        private void onFind(Boolean isSuccess, Site site)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new onFind_dg(onFind), new object[] { isSuccess, site });
                } 
                else
                {
                    
                    foreach (ListViewItem item in listView1.Items)
                    {
                        Site s = (Site)item.Tag;
                        if (site == s)
                        {
                            if ((site.getProxys() == null || site.getProxys().Count == 0) 
                                && site.retryCount < 3)
                            {
                                site.retryCount++;
                                item.SubItems[2].Text = String.Format("第{0}次重试", site.retryCount);
                                Thread thread = new Thread(new ThreadStart(site.run));
                                thread.IsBackground = true;
                                thread.Start();
                                return;
                            }
                            
                            if (isSuccess && site.getProxys() != null && site.getProxys().Count != 0)
                            {
                                item.SubItems[2].Text = site.getProxys().Count.ToString();

                                foreach (Proxy p in site.getProxys())
                                {
                                    if (p.port > 65535 /*|| p.type == eProxyType.HTTP*/)
                                    {
                                        continue;
                                    }
                                    if (!all_string_proxys.Contains(p.toIpString()))
                                    {
                                        if (site is _216_site || site is qqbb_site || site is yunproxy_site)
                                        {
                                            all_me_proxys_list.Add(p);
                                        }
                                        if (checkBox1.Checked && (p.type == eProxyType.SOCKS4 || p.type == eProxyType.SOCKS5))
                                        {
                                            all_proxys.Add(p);
                                            all_string_proxys.Add(p.toIpString());
                                        }
                                        else if (checkBox2.Checked && (p.type == eProxyType.HTTP))
                                        {
                                            all_proxys.Add(p);
                                            all_string_proxys.Add(p.toIpString());
                                        }
                                    }
                                    /*switch (p.type)
                                    {
                                        case eProxyType.HTTP:
                                            if (!all_http_proxys.Contains(p.toString()))
                                            {
                                                if (site is _216_site || site is qqbb_site || site is yunproxy_site)
                                                {
                                                    all_me_proxys_list.Add(p);
                                                }
                                                all_http_proxys_list.Add(p);
                                                all_http_proxys.Add(p.toString());
                                            }
                                            break;
                                        case eProxyType.SOCKS4:
                                        case eProxyType.SOCKS5:
                                            if (!all_socks_proxys.Contains(p.toString()))
                                            {
                                                if (site is _216_site || site is qqbb_site || site is yunproxy_site)
                                                {
                                                    all_me_proxys_list.Add(p);
                                                }
                                                all_socks_proxys.Add(p.toString());
                                            }
                                            break;
                                    }*/

                                }
                                
                            }
                            else
                            {
                                item.SubItems[2].Text = "error";
                            }
                            
                        }
                    }
                    if (checked_sites.Contains(site))
                    {
                        checked_sites.Remove(site);
                        if (checked_sites.Count == 0)
                        {
                            if (all_proxys == null || all_proxys.Count == 0)
                            {
                                label1.Text = "未发现任何代理";
                                button1.Enabled = true;
                                listView1.Enabled = true;
                                return;
                            }
                            saveME();
                            if (isProxyJudgerUrlFileExist)
                            {
                                label1.Text = "开始验证...";
                                progressBar1.Maximum = all_proxys.Count;
                                ThreadPool.SetMaxThreads(int.Parse(textBox1.Text), 512);
                                judgedCount = 0;
                                FileStream fs = new FileStream("forPE.txt", FileMode.Create);
                                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                                
                                foreach (Proxy p in all_proxys)
                                {
                                    sw.WriteLine(p.toString());
                                    ProxyJudger pj = new ProxyJudger(p);
                                    pj.onJudgeDone += onJudgeDone;
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(pj.doProxyJudge), int.Parse(textBox2.Text) * 1000);
                                }
                                sw.Close();
                                fs.Close();
//                                 progressBar1.Maximum = all_http_proxys_list.Count;
//                                 ThreadPool.SetMaxThreads(int.Parse(textBox1.Text), 512);
//                                 //ThreadPool.SetMaxThreads(1, 512);
//                                 judgedCount = 0;
//                                 foreach (Proxy p in all_http_proxys_list)
//                                 {
//                                     ProxyJudger pj = new ProxyJudger(p);
//                                     pj.onJudgeDone += onJudgeDone;
//                                     ThreadPool.QueueUserWorkItem(new WaitCallback(pj.doProxyJudge), int.Parse(textBox2.Text) * 1000);
//                                 }
                            }
                            else
                            {
                                label1.Text = "完成（未验证）";
                                button1.Enabled = true;
                                listView1.Enabled = true;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void saveME()
        {
            String file_path = textBox3.Text;
            FileStream fs = new FileStream(file_path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
            foreach (Proxy p in all_me_proxys_list)
            {
                if (p.toMeString() != "")
                {
                    sw.Write(p.toMeString() + "\n");
                }
            }
            sw.Close();
            fs.Close();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (!File.Exists("proxyjudger.txt"))
            {
                MessageBox.Show("验证网址文件不存在，请检查应用程序目录下的proxyjudger.txt文件!");
                isProxyJudgerUrlFileExist = false;
            }
            else
            {
                FileStream fs = new FileStream("proxyjudger.txt", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.WriteLine(textBox5.Text);
                sw.Close();
                fs.Close();
            }
            label1.Text = "开始读取...";
            button1.Enabled = false;
            listView1.Enabled = false;
            checked_sites.Clear();
            all_me_proxys_list.Clear();
            all_proxys.Clear();
            all_string_proxys.Clear();
            foreach (ListViewItem item in listView1.Items)
            {
                
                if (item.Checked)
                {
                    item.SubItems[2].Text = "读取中";
                    Site site = (Site)item.Tag;
                    checked_sites.Add(site);
                    site.onFindResultEvent += onFind;
                    site.retryCount = 0;
                    Thread th = new Thread(new ThreadStart(site.run));
                    th.IsBackground = true;
                    th.Start();
                }
            }
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (button1.Enabled)
            {
                button1.PerformClick();
            }
            timer1.Stop();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (File.Exists("proxyjudger.txt"))
            {
                StreamReader sr = new StreamReader("proxyjudger.txt");
                textBox5.Text = sr.ReadLine();
                sr.Close();
            }
        }
    }
}
