using Reptile.Entity;
using Reptile.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Reptile
{
    public class ReptileCore
    {
        private string targetUrl;
        private HttpHelper httpHelper = new HttpHelper();
        private IWatcher watcher;
        private Thread reptileThread;
        private Regex regex;
        private List<Shop> shopList = new List<Shop>();
        private const string shopCommentTemplate = @"http://www.dianping.com/shop/{0}/review_more?pageno={1}";
        private const string userFollowsTemplate = @"http://www.dianping.com/member/{0}/follows?pg={1}";
        private const string userFansTemplate = @"http://www.dianping.com/member/{0}/fans?pg={1}";

        public ReptileCore(string url)
        {
            this.targetUrl = url;
        }

        public void Register(IWatcher watcher)
        {
            this.watcher = watcher;
        }

        public void BeginCatch()
        {
            reptileThread = new Thread(new ThreadStart(process));
            reptileThread.IsBackground = true;
            reptileThread.Start();
        }

        public void AbortCatch()
        {
            if (reptileThread == null) return;
            if (reptileThread.IsAlive)
            {
                reptileThread.Abort();
            }
        }

        private void process()
        {
            watcher.Log("开始抓取Shop数据，当前抓取地址为:" + targetUrl);

            //网页内容
            var html = getHtml(targetUrl);
            //获取页数
            var pageCount = getPageCount(html);

            watcher.Log("当前分类总页数为:" + pageCount);
            if (pageCount == 0) return;

            //获取所有商店的信息
            getShopDetail(pageCount);

            watcher.Log("Shop抓取完毕，共有 " + shopList.Count + " 条记录，开始写入数据库");

            //TO DO

            watcher.Log("写入数据库完毕，开始抓取点评数据");

            foreach (var shop in shopList)
            {
                var result = getComment(shop.ID);
                watcher.Log("已抓取 " + shop.Name + " 的评论记录，正在写入数据库...");
                //To Do


                watcher.Log("写入数据库完毕，抓取下一条记录");
            }
        }

        //获取页码总数
        private int getPageCount(string html)
        {
            regex = new Regex(@"class=""[Pp]{1}age[Ll]{1}ink""\s+title=""(\d+)""");
            var matches = regex.Matches(html);
            if (matches.Count == 0) return 0;
            return int.Parse(matches[matches.Count - 1].Groups[1].Value);
        }

        //获取商店的详细信息
        private void getShopDetail(int pageCount)
        {
            regex = new Regex(@"href=""/shop/(\d+)""\s+\>[\n\s]+\<h4\>([^\<]+)\</h4\>[\s\S]*?质量\<b\>([^\<]+)[\s\S]*?环境\<b\>([^\<]+)[\s\S]*?服务\<b\>([^\<]+)");
            for (var i = 0; i < pageCount; i++)
            {
                var html = getHtml(targetUrl + "p" + i);
                var matches = regex.Matches(html);
                foreach (Match item in matches)
                {
                    watcher.Log(item.Groups[1].Value + " " + item.Groups[2].Value + " "
                        + item.Groups[3].Value + " " + item.Groups[4].Value + " " + item.Groups[5].Value);
                    shopList.Add(new Shop()
                    {
                        ID = item.Groups[1].Value,
                        Name = item.Groups[2].Value,
                        Url = "/shop/" + item.Groups[2].Value,
                        QualityScore = decimal.Parse(item.Groups[3].Value),
                        EnvironmentScore = decimal.Parse(item.Groups[4].Value),
                        ServiceScore = decimal.Parse(item.Groups[5].Value)
                    });
                }
            }
        }

        //获取对应商店的所有评论
        private List<Comment> getComment(string id)
        {
            var result = new List<Comment>();
            var html = getHtml(string.Format(shopCommentTemplate, id, 1));

            var pageCount = getPageCount(html);
            watcher.Log(html);

            regex = new Regex(@"href=""(/member/\d+)""\s+user-id=""(\d+)""\s+class=""j_card""[\s\S]*?产品(\d{1})[\s\S]*?环境(\d{1})[\s\S]*?服务(\d{1})");

            for (var i = 1; i <= pageCount; i++)
            {
                if (i > 1)
                    html = getHtml(string.Format(shopCommentTemplate, id, i));
                var matches = regex.Matches(html);
                foreach (Match item in matches)
                {
                    var comment = new Comment()
                    {
                        UserUrl = item.Groups[1].Value,
                        UserID = item.Groups[2].Value,
                        ShopId = id,
                        ProductScore = int.Parse(item.Groups[3].Value),
                        EnvironmentScore = int.Parse(item.Groups[4].Value),
                        ServiceScore = int.Parse(item.Groups[5].Value)
                    };
                    result.Add(comment);

                    var user = getUserDetail(comment.UserID);
                    if (user == null) continue;
                    watcher.Log(user.userID + " " + user.userName + " " + user.follows.Count + " " + user.fans.Count);
                    //to do
                }
            }


            return result;
        }

        //临时的已访问用户记录列表
        private List<string> visitedUsers = new List<string>();
        //获取用户详细信息
        private User getUserDetail(string id)
        {
            if (visitedUsers.Contains(id)) return null;
            else visitedUsers.Add(id);

            var user = new User();
            user.userID = id;

            var html = getHtml(string.Format(userFollowsTemplate, id, 1));
            //获取用户名
            user.userName = getUserName(html);
            //获取关注
            user.follows = getFansFollows(userFollowsTemplate, id);
            //获取粉丝
            user.fans = getFansFollows(userFansTemplate, id);

            return user;
        }

        private List<string> getFansFollows(string urlTemplate, string userid)
        {
            var result = new List<string>();

            var html = getHtml(string.Format(urlTemplate, userid, 1));
            //获取页码数
            var pageCount = getUserPageCount(html);

            for (var i = 1; i < pageCount; i++)
            {
                if (i > 1)
                    html = getHtml(string.Format(userFollowsTemplate, userid, i));
                regex = new Regex(@"\<img\s+title=""[^""]+""\s+user-id=""(\d+)""");
                var matches = regex.Matches(html);
                foreach (Match item in matches)
                {
                    result.Add(item.Groups[1].Value);
                }
            }

            return result;
        }

        private int getUserPageCount(string html)
        {
            regex = new Regex(@"data-pg=""(\d+)""\>");
            var matches = regex.Matches(html);
            if (matches.Count == 0) return 0;
            return int.Parse(matches[matches.Count - 1].Groups[1].Value);
        }

        private string getUserName(string html)
        {
            regex = new Regex(@"class=""name""\>([^\<]+)");
            return regex.Match(html).Groups[1].Value;
        }

        private string getHtml(string url)
        {
            return httpHelper.GetHtml(
                new HttpItem()
                {
                    URL = url,
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.65 Safari/537.36"
                }).Html;
        }
    }
}
