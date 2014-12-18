using Reptile.Entity;
using Reptile.Interface;
using Reptile.Model;
using System;
using System.Collections.Generic;
using System.Text;
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
        private reptileEntities dbContext = new reptileEntities();

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

        //爬虫线程主方法
        private void process()
        {
            //更新页面显示消息
            watcher.Log("开始抓取Shop数据，当前抓取地址为:" + targetUrl);

            //获取网页内容
            var html = getHtml(targetUrl);
            //获取页数
            var pageCount = getPageCount(html);

            watcher.Log("当前分类总页数为:" + pageCount);

            //获取所有商店的信息
            var shopList = getShopDetail(pageCount);

            watcher.Log("Shop抓取完毕，共有 " + shopList.Count + " 条记录，开始写入数据库");

            //存入数据库
            save(); 

            watcher.Log("写入数据库完毕，开始抓取点评数据");

            //对抓取到的所有商店信息进行遍历
            foreach (var shop in shopList)
            {
                //根据商店ID获取所有评论
                var result = getComment(shop.ID);
                watcher.Log("已抓取 " + shop.Name + " 的评论记录和所有评论用户的粉丝，正在写入数据库...");

                //存入数据库
                save();

                watcher.Log("写入数据库完毕，抓取下一条记录");
            }
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

        //获取页码总数
        private int getPageCount(string html)
        {
            //页面正则
            regex = new Regex(@"class=""[Pp]{1}age[Ll]{1}ink""[\s\S]*?title=""(\d+)""");
            //获取所有匹配历史
            var matches = regex.Matches(html);
            //如果无匹配，则说明只有一页
            if (matches.Count == 0) return 1;
            //返回所匹配的最后一个，即最大值
            return int.Parse(matches[matches.Count - 1].Groups[1].Value);
        }

        //获取商店的详细信息
        private List<ShopEntity> getShopDetail(int pageCount)
        {
            var result = new List<ShopEntity>();
            //商店信息正则
            regex = new Regex(@"href=""/shop/(\d+)""\s+\>[\n\s]+\<h4\>([^\<]+)\</h4\>[\s\S]*?质量\<b\>([^\<]+)[\s\S]*?环境\<b\>([^\<]+)[\s\S]*?服务\<b\>([^\<]+)");
            //根据页数遍历商店
            for (var i = 1; i <= pageCount; i++)
            {
                //获取当前页商店列表html
                var html = getHtml(targetUrl + "p" + i);
                var matches = regex.Matches(html);
                foreach (Match item in matches)
                {
                    watcher.Log(item.Groups[1].Value + " " + item.Groups[2].Value + " "
                        + item.Groups[3].Value + " " + item.Groups[4].Value + " " + item.Groups[5].Value);
                    //保存抓取信息
                    var shop = new ShopEntity()
                    {
                        ID = item.Groups[1].Value, //商店ID
                        Name = item.Groups[2].Value, //商店名称
                        Url = "/shop/" + item.Groups[1].Value, //商店URL
                        QualityScore = decimal.Parse(item.Groups[3].Value), //质量得分
                        EnvironmentScore = decimal.Parse(item.Groups[4].Value), //环境得分
                        ServiceScore = decimal.Parse(item.Groups[5].Value) //服务得分
                    };

                    result.Add(shop);
                    dbContext.Shop.Add(new Shop()
                    {
                        ShopID = shop.ID,
                        Name = shop.Name,
                        Url = shop.Url,
                        Quality = shop.QualityScore,
                        Environment = shop.EnvironmentScore,
                        Service = shop.ServiceScore
                    });
                }

                sleep();//停顿一段随机的时间，尽量避免被封IP
            }
            return result;
        }

        //获取对应商店的所有评论
        private List<CommentEntity> getComment(string id)
        {
            var result = new List<CommentEntity>();
            var html = getHtml(string.Format(shopCommentTemplate, id, 1));

            var pageCount = getPageCount(html);
            watcher.Log(pageCount.ToString());

            regex = new Regex(@"href=""(/member/\d+)""\s+user-id=""(\d+)""\s+class=""j_card""[\s\S]*?产品(\d{1})[\s\S]*?环境(\d{1})[\s\S]*?服务(\d{1})");

            for (var i = 1; i <= pageCount; i++)
            {
                if (i > 1)
                    html = getHtml(string.Format(shopCommentTemplate, id, i));
                var matches = regex.Matches(html);
                
                foreach (Match item in matches)
                {
                    var comment = new CommentEntity()
                    {
                        UserUrl = item.Groups[1].Value,
                        UserID = item.Groups[2].Value,
                        ShopId = id,
                        ProductScore = int.Parse(item.Groups[3].Value),
                        EnvironmentScore = int.Parse(item.Groups[4].Value),
                        ServiceScore = int.Parse(item.Groups[5].Value)
                    };
                    result.Add(comment);
                    dbContext.Comment.Add(new Comment()
                    {
                        UserID = comment.UserID,
                        UserUrl = comment.UserUrl,
                        ShopID = comment.ShopId,
                        ProductScore = comment.ProductScore,
                        EnvironmentScore = comment.EnvironmentScore,
                        ServiceScore = comment.ServiceScore
                    });

                    var user = getUserDetail(comment.UserID);
                    if (user == null) continue;
                    watcher.Log(user.userID + " " + user.userName + " " + user.follows.Count + " " + user.fans.Count);
                }

                sleep();//停顿
            }


            return result;
        }

        //临时的已访问用户记录列表
        private List<string> visitedUsers = new List<string>();
        //获取用户详细信息
        private UserEntity getUserDetail(string id)
        {
            if (visitedUsers.Contains(id)) return null;
            else visitedUsers.Add(id);

            var user = new UserEntity();
            user.userID = id;

            var html = getHtml(string.Format(userFollowsTemplate, id, 1));
            //获取用户名
            user.userName = getUserName(html);
            //获取关注
            user.follows = getFansFollows(userFollowsTemplate, id);
            //获取粉丝
            user.fans = getFansFollows(userFansTemplate, id);
            //判断数据库中是否已有当前用户，若有则直接返回该用户
            if (dbContext.User.Find(id) != null) return user;

            //加入到数据库
            dbContext.User.Add(new User() { UserID = user.userID, Name = user.userName });
            user.follows.ForEach(new Action<string>((s) => { dbContext.Follows.Add(new Follows() { UserID = id, FollowID = s }); }));
            user.fans.ForEach(new Action<string>((s) => { dbContext.Fans.Add(new Fans() { UserID = id, FanID = s }); }));

            return user;
        }

        private List<string> getFansFollows(string urlTemplate, string userid)
        {
            var result = new List<string>();

            var html = getHtml(string.Format(urlTemplate, userid, 1));

            //获取页码数
            var pageCount = getUserPageCount(html);

            for (var i = 1; i <= pageCount; i++)
            {
                if (i > 1) //第一页不需要再次获取
                    html = getHtml(string.Format(urlTemplate, userid, i));
                regex = new Regex(@"\<img\s+title=""[^""]+""\s+user-id=""(\d+)""");
                var matches = regex.Matches(html);
                foreach (Match item in matches)
                {
                    result.Add(item.Groups[1].Value);
                }

                sleep();//停顿
            }

            return result;
        }

        //获取用户界面的页码数
        private int getUserPageCount(string html)
        {
            regex = new Regex(@"data-pg=""(\d+)""\>");
            var matches = regex.Matches(html);
            if (matches.Count == 0) return 1;
            return int.Parse(matches[matches.Count - 1].Groups[1].Value);
        }

        private string getUserName(string html)
        {
            regex = new Regex(@"class=""name""\>([^\<]+)");
            return regex.Match(html).Groups[1].Value;
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

        //用于停顿一段随机的时间（2-5秒）
        private Random rand = new Random();
        private void sleep()
        {
            Thread.Sleep(rand.Next(2000, 5000));
        }
    }
}
