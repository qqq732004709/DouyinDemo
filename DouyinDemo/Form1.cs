using Microsoft.Playwright;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Forms;

namespace DouyinDemo
{
    public partial class Form1 : Form
    {
        IBrowser browser;
        IBrowserContext context;
        IPage page;
        IPlaywright playwright;
        WebView2 webView;

        public string CookieInfo { get; set; }

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            //动态加载webView2 并开启CDP控制端口
            webView = new WebView2
            {
                Visible = true,
                Dock = DockStyle.Fill,
            };
            container.Controls.Add(webView);
            await webView.EnsureCoreWebView2Async(await CoreWebView2Environment.CreateAsync(null, null, new CoreWebView2EnvironmentOptions()
            {
                AdditionalBrowserArguments = "--remote-debugging-port=9223",
            })).ConfigureAwait(true);

            //通过playwright跳转douyin.com
            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.ConnectOverCDPAsync("http://localhost:9223");
            context = browser.Contexts[0];
            page = context.Pages[0];
            await page.GotoAsync("https://creator.douyin.com");
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //清除缓存 重新加载当前页面
            await context.ClearCookiesAsync();
            await page.GotoAsync("https://www.douyin.com");
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            //保存cookie到内存中
            var authInfo = await context.CookiesAsync();
            var cookieJson = JsonSerializer.Serialize(authInfo);
            CookieInfo = cookieJson;
            MessageBox.Show("保存成功");
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            //加载cookie
            var cookieList = JsonSerializer.Deserialize<List<Microsoft.Playwright.Cookie>>(CookieInfo);
            if (cookieList != null && cookieList.Count > 0)
            {
                await context.AddCookiesAsync(cookieList);
            }
            await page.GotoAsync("https://www.douyin.com");
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions() { Name = "发布作品" }).ClickAsync();

            await page.Locator(".upload-btn-input--1NeEX").SetInputFilesAsync(new[] { @"C:\Users\xy\Documents\WeChat Files\wxid_p2q9v7slvazj21\FileStorage\File\2023-02\6分钟视频-第二版3种音乐\6分钟视频-第二版3种音乐.mp4" });

            //循环检测视频是否上传成功
            var loopCount = 0;
            while (true)
            {
                try
                {
                    loopCount++;
                    if (loopCount == 50)
                    {
                        MessageBox.Show("上传失败");
                        return;
                    }
                    await page.GetByText("重新上传").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
                }
                catch (Exception ex)
                {
                    //未上传成功等待1秒继续
                    await page.WaitForTimeoutAsync(1000);
                    continue;
                }
            }

            //标题
            await page.Locator(".zone-container").FillAsync("这是个测试标题​");

            //视频分类
            await page.Locator("div.semi-cascader-selection").ClickAsync();
            await page.GetByRole(AriaRole.Listitem).Filter(new LocatorFilterOptions { HasText = "电视剧" }).ClickAsync();
            await page.GetByText("电视剧解说").ClickAsync();

            //视频标签
            await page.GetByPlaceholder("输入后按 Enter 键可添加自定义标签").FillAsync("测试标签");
            await page.GetByPlaceholder("输入后按 Enter 键可添加自定义标签").PressAsync("Enter");
            await page.PauseAsync();
            //申请关联热点
            await page.GetByText("点击输入热点词").ClickAsync();
            await page.GetByText("美国公然喊话中国对伊朗施压").ClickAsync();

            //允许他人保存视频
            await page.Locator("label").Filter(new LocatorFilterOptions { HasText = "不允许" }).ClickAsync();
            //设置谁可以看
            await page.Locator("label").Filter(new LocatorFilterOptions { HasText = "好友可见" }).ClickAsync();

            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "发布", Exact = true }).ClickAsync();
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "暂不同步" }).ClickAsync();
        }
    }
}
