using Reptile.Entity;
using Reptile.Interface;
using Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Reptile
{
    public class ReptileCore
    {
        //目标地址，初始地址
        private string targetUrl;
        //负责显示消息的Watcher
        private IWatcher watcher;
        //爬虫线程
        private Thread reptileThread;
        private Regex regex;
        //商店评论地址模版
        private const string shopCommentTemplate = @"http://www.dianping.com/shop/{0}/review_more?pageno={1}";
        //用户关注地址模版
        private const string userFollowsTemplate = @"http://www.dianping.com/member/{0}/follows?pg={1}";
        //用户粉丝地址模版
        private const string userFansTemplate = @"http://www.dianping.com/member/{0}/fans?pg={1}";
        //Http请求操作类
        private HttpHelper httpHelper = new HttpHelper();
        //数据库
        private testEntities dbContext = new testEntities();

        public ReptileCore() { }

        //构造函数，传入目标地址
        public ReptileCore(string url)
        {
            this.targetUrl = url;
        }


        //向爬虫类注册负责更新消息的watcher
        public void Register(IWatcher watcher)
        {
            this.watcher = watcher;
        }

        //启动爬虫线程
        public void BeginCatch()
        {
            reptileThread = new Thread(new ThreadStart(process));
            reptileThread.IsBackground = true;
            reptileThread.Start();
        }

        //关闭爬虫线程
        public void AbortCatch()
        {
            if (reptileThread == null) return;
            if (reptileThread.IsAlive)
            {
                reptileThread.Abort();
            }
        }

        private string[] shroad = { "http://www.jtcx.sh.cn/zhishu/fastroad.jsp", "http://www.jtcx.sh.cn/zhishu/ground.jsp" };
        private string[] shroadHome = {"http://www.jtcx.sh.cn/TravelServlet?type=parkingSpace.xml",
                                        "http://www.jtcx.sh.cn/TravelServlet?type=viaduct.xml",
                                        "http://www.jtcx.sh.cn/TravelServlet?type=MainRoadEvent.xml",
                                        "http://www.jtcx.sh.cn/TravelServlet?type=Congestion.xml"};
        private int[] fieldcount = { 2, 3, 2, 4 };
        private string[] ico = { "畅通", "较畅通", "拥挤", "堵塞" };

        //爬虫线程主方法
        private void process()
        {
            var regexTime = new Regex(@"(2015-\d{2}-\d{2}\s\d{2}:\d{2})", RegexOptions.IgnoreCase);
            var regexRoad = new Regex(@"zs_ico(\d)[\s\S]+?setmapimage[^\>]+\>([^\<]+)[\s\S]+?class[^\>]+\>([^\<]+)[\s\S]+?class[^\>]+\>([^\<]+)[\s\S]+?class[^\>]+\>([^\<]+)", RegexOptions.IgnoreCase);
            var regexHome = new Regex(@"name=""([^""]+)""\svalue=""([^""]+)""", RegexOptions.IgnoreCase);
            string html;
            DateTime currentTime = new DateTime();

            while (true)
            {
                for (var i = 0; i < 2; i++)
                {
                    html = getHtml(shroad[i]);
                    watcher.Log("获取当前时间");
                    var time = regexTime.Match(html);
                    var tmpTime = DateTime.Parse(time.Groups[1].Value);
                    if (currentTime.Equals(tmpTime))
                    {
                        watcher.Log("当前数据未更新");
                        break;
                    }
                    currentTime = tmpTime;
                    watcher.Log(currentTime.ToString());
                    watcher.Log("获取道路信息");

                    var result = regexRoad.Matches(html);
                    foreach (Match item in result)
                    {
                        var road = new SHRoadIndex();
                        road.State = ico[int.Parse(item.Groups[1].Value.Trim()) - 1];
                        road.Name = item.Groups[2].Value.Trim();
                        road.CurrentIndex = decimal.Parse(item.Groups[3].Value.Trim());
                        road.ReferenceIndex = decimal.Parse(item.Groups[4].Value.Trim());
                        road.DValue = decimal.Parse(item.Groups[5].Value.Trim());
                        road.Date = currentTime.ToShortDateString();
                        road.Time = currentTime.ToShortTimeString();
                        road.Type = i;
                        dbContext.SHRoadIndex.Add(road);
                        watcher.Log(road.State + "," + road.Name + "," + road.CurrentIndex + "," +
                            road.ReferenceIndex + "," + road.DValue + "," + road.Time.ToString());
                    }
                }

                //停车位
                html = getHtml(shroadHome[0]);
                var home = regexHome.Matches(html);
                for (var i = 0; i < home.Count; i += 2)
                {
                    var packing = new Packing()
                    {
                        Name = home[i].Groups[2].Value,
                        PackingSpace = int.Parse(home[i + 1].Groups[2].Value),
                        Time = DateTime.Now.ToShortTimeString(),
                        date = DateTime.Now.ToShortDateString()
                    };
                    watcher.Log(packing.Name + "," + packing.PackingSpace + "," + packing.Time);
                    dbContext.Packing.Add(packing);
                }

                //城市快速路
                html = getHtml(shroadHome[1]);
                home = regexHome.Matches(html);
                for (var i = 0; i < home.Count; i += 3)
                {
                    var news = new News();
                    var time = DateTime.Parse(home[i + 1].Groups[2].Value);
                    news.RoadName = home[i].Groups[2].Value;
                    news.Time = time.ToShortTimeString();
                    news.date = time.ToShortDateString();
                    news.Detail = home[i].Groups[2].Value + " " + (home[i + 2].Groups[2].Value.Equals("22") ? "拥挤" : "阻塞");
                    news.RoadType = 1;

                    if (Exists(news)) continue;
                    watcher.Log(news.RoadName + "," + news.Detail + "," + news.Time);
                    dbContext.News.Add(news);
                }

                //干线公路
                html = getHtml(shroadHome[2]);
                home = regexHome.Matches(html);
                for (var i = 0; i < home.Count; i += 2)
                {
                    var news = new News();
                    var time = DateTime.Parse(home[i].Groups[2].Value);
                    news.Time = time.ToShortTimeString();
                    news.date = time.ToShortDateString();
                    news.Detail = home[i + 1].Groups[2].Value;
                    news.RoadType = 2;

                    if (Exists(news)) continue;
                    watcher.Log(news.RoadName + "," + news.Detail + "," + news.Time);
                    dbContext.News.Add(news);
                }

                //地面道路
                html = getHtml(shroadHome[3]);
                home = regexHome.Matches(html);
                for (var i = 0; i < home.Count; i += 4)
                {
                    var news = new News();
                    var time = DateTime.Parse(home[i + 2].Groups[2].Value);
                    news.RoadName = home[i].Groups[2].Value;
                    news.Detail = home[i + 1].Groups[2].Value + " " + (home[i + 3].Groups[2].Value.Equals("22") ? "拥挤" : "阻塞");
                    news.Time = time.ToShortTimeString();
                    news.date = time.ToShortDateString();
                    news.RoadType = 3;

                    if (Exists(news)) continue;
                    watcher.Log(news.RoadName + "," + news.Detail + "," + news.Time);
                    dbContext.News.Add(news);
                }
                //写入数据库
                watcher.Log("写入数据库...");
                save();
                watcher.Log("写入成功。");
                watcher.Log("等待两分钟");
                sleep();
            }
        }

        private bool Exists(News news)
        {
            return dbContext.News.FirstOrDefault(p =>
                p.Detail.Equals(news.Detail)) != null;
        }

        private void save()
        {
            try
            {
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                watcher.Log(ex.Message);
            }
        }

        //使用Get方法获取html
        private string getHtml(string url)
        {
            return httpHelper.GetHtml(
                new HttpItem()
                {
                    URL = url,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36"
                }).Html;
        }

        //停顿两分钟
        private void sleep()
        {
            Thread.Sleep(120000);
        }
    }
}
