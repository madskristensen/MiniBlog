# MiniBlog

A blogging engine based on HTML5 and ASP.NET. For an ASP.NET Core version, see [Miniblog.Core](https://github.com/madskristensen/Miniblog.Core).

[![Build status](https://ci.appveyor.com/api/projects/status/n78wm50a4a3odecb)](https://ci.appveyor.com/project/madskristensen/miniblog)

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

__Live demo__: http://miniblog.azurewebsites.net/  
Username: _demo_  
Password: _demo_  

### Custom theme
In search for custom designed themes for MiniBlog? [Click here](https://francis.bio/miniblog-themes/).

## Simple, flexible and powerful

A minimal, yet full featured blog engine using ASP.NET Razor Web Pages. 
Perfect for the blogger who wants to selfhost a blog. 

### Features

* Best-in-class __performance__
 * Gets a perfect score of 100/100 on Google Page Speed
 * Uses __CDN__ for Bootstrap and jQuery in release mode (debug="false")
 * Easy setting for serving static files from another domain. 
     * Supports the ASP.NET [Reverse Proxy](https://github.com/madskristensen/ReverseProxyCDN)
* __Open Live Writer__ (OLW) support
 * Optimized for OLW
 * Assumes OLW is the main way to write posts
 * You don't have to use OLW (but you should)
* RSS and ATOM __feeds__
* Schedule posts to be published on a future date
* Get feedback on an unpublished post by sending a preview link 
* __SEO__ optimized
 * Uses HTML 5 __microdata__ to add semantic meaning
 * Support for __robots.txt__ and __sitemap.xml__
* Theming support
 * Based on Bootstrap themes. Makes it easy to customize your blog
 * Comes with a one-column, two-column, and off-canvas theme
* __No database__ required
 * Uses the same XML format as BlogEngine.NET
 * Move your existing blog to MiniBlog using [MiniBlog Formatter](https://github.com/madskristensen/MiniBlogFormatter)
* __Inline editing__ of blog posts
* Comments support
 * __Gravatar__ support 
 * Can easily be replaced by 3rd-party commenting system
* __Drag 'n drop__ images to upload
 * Automatically __optimizes uploaded images__
* Uses latest technologies
 * __OpenGraph__ enabled
 * Based on jQuery and Bootstrap
* Best-in-class __accessibility__
* __Mobile__ friendly
* Works on any host including __Windows Azure__ Websites

### Why another blog engine?
7 years have passed since I started the [BlogEngine.NET](http://dotnetblogengine.net) project. 
It was using cutting edge technology for its time and quickly became the 
most popular blogging platform using ASP.NET.

The MiniBlog was born as a test to see what a modern blog engine could
look like today with the latest ASP.NET and HTML 5 technologies. Just like
with BlogEngine.NET, the goal was to see how small and simple such a 
blog engine could be. 

This is the result.

### Connecting with Open Live Writer (OLW)

To connect to MiniBlog with Open Live Writer:

- Launch Open Live Writer

- If you have not used Open Live Writer to connect to a blog you will get a dialog window asking you to specify what blog service you use. If you have already connected Open Live Writer to a blog, you can go to _Blogs -> Add blog account..._ and get to the same dialog window.

- In the __What blog service do you use?__ dialog window you will tick the _Other services_ radio option and click next.

- The __Add a blog account__ dialog window will ask you for the web address of your blog, the username and password. The web address is the root address of your site. For example, use http://miniblog.azurewebsites.net/ for the live demo site.

- The __Download Blog Theme__ dialog window will let you know Open Live Writer can download your blog theme if you allow it to publish a temporary post. Selecting yes will allow you to view how your posts will look directly from the Open Live Writer editor. 

- The __Select blog type__ dialog window will let you know Open Live Writer was not able to detect your blog type. It will ask you for the type of blog and the remote posting URL.  
Type of blog that you are using: _Metaweblog API_  
Remote posting URL for your blog: _http://&lt;root-address&gt;/metaweblog_  
Click next.

- The __Your blog has been set up__ dialog window will let you give your blog a nickname for the Open Live Writer instance. Change that if you want and click finish to get to posting!

Open Live Writer can be downloaded at:  
[http://openlivewriter.org/](http://openlivewriter.org/)  

### Configuring MiniBlog as Virtual Application

MiniBlog is very compact and can be configured as a Virtual Application so you'd be able to use it alongside your existing websites. 
For example if you've got a running ASP.NET website at `http://yourexamplesite.com/` and you want to setup a blog under `/blog/` path, you could setup `http://yourexamplesite.com/blog/` with a few simple tweaks in web.config settings:

- Set `blog:path` element of `appSettings` to the virtual path that you've configured for MiniBlog. Example with path `blog`

```xml
<add key="blog:path" value="blog"/>
```

- Update the `path` attribute of all the `<handlers>` in web.config. Example with path `blog`

```xml
<handlers>
    <remove name="CommentHandler"/>
    <add name="CommentHandler" verb="*" type="CommentHandler" path="/blog/comment.ashx"/>
    <remove name="PostHandler"/>
    <add name="PostHandler" verb="POST" type="PostHandler" path="/blog/post.ashx"/>
    <remove name="MetaWebLogHandler"/>
    <add name="MetaWebLogHandler" verb="POST,GET" type="MetaWeblogHandler" path="/blog/metaweblog"/>
    <remove name="FeedHandler"/>
    <add name="FeedHandler" verb="GET" type="FeedHandler" path="/blog/feed/*"/>
    <remove name="CssHandler"/>
    <add name="CssHandler" verb="GET" type="MinifyHandler" path="/blog*.css"/>
    <remove name="JsHandler"/>
    <add name="JsHandler" verb="GET" type="MinifyHandler" path="/blog*.js"/>
</handlers>

<httpErrors>
    <remove statusCode="404"/>
    <error statusCode="404" responseMode="ExecuteURL" path="/blog/404.cshtml"/>
</httpErrors>
```

After changing the config all that is left is configuring a Virtual Application with the same path(ex. `blog`) inside your IIS website.
