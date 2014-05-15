# MiniBlog

A blogging engine based on HTML5 and ASP.NET.

__Live demo__: http://miniblog.azurewebsites.net/  
Username: _demo_  
Password: _demo_  


## Simple, flexible and powerful

A minimal, yet full featured blog engine using ASP.NET Razor Web Pages. 
Perfect for the blogger who wants to selfhost a blog. 

### Features

* Best-in-class __performance__
 * Gets a perfect score of 100/100 on Google Page Speed
 * Uses __CDN__ for Bootstrap and jQuery in release mode (debug="false")
* __Windows Live Writer__ (WLW) support
 * Optimized for WLW
 * Assumes WLW is the main way to write posts
 * You don't have to use WLW (but you should)
* RSS and ATOM __feeds__
* Schedule posts to be published on a future date
* __SEO__ optimized
 * Uses HTML 5 __microdata__ to add semantic meaning
 * Support for __robots.txt__ and __sitemap.xml__
* Theming support
 * Based on Bootstrap themes. Makes it easy to customize your blog
 * Comes with a one-column and a two-column theme
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

### Connecting with Windows Live Writer (WLW)

To connect to MiniBlog with Windows Live Writer (as of WLW build 14.0.8117.416):

- Launch Windows Live Writer

- If you have not used Windows Live Writer to connect to a blog you will get a dialog window asking you to specify what blog service you use. If you have already connected Windows Live Writer to a blog, you can go to _Blogs -> Add blog account..._ and get to the same dialog window.

- In the __What blog service do you use?__ dialog window you will tick the _Other blog service_ radio option and click next.

- The __Add a blog account__ dialog window will ask you for the web address of your blog, the username and password. The web address is the root address of your site. For example, use http://miniblog.azurewebsites.net/ for the live demo site.

- The __Select blog type__ dialog window will let you know Windows Live Writer was not able to detect your blog type. It will ask you for the type of blog and the remote posting URL.  
Type of blog that you are using: _Metaweblog API_  
Remote posting URL for your blog: _http://&lt;root-address&gt;/metaweblog_  
Click next.

- The __Your blog has been set up__ dialog window will let you give your blog a nickname for the Windows Live Writer instance. Change that if you want and click finish to get to posting!

Windows Live Writer can be downloaded at:  
[http://www.microsoft.com/en-us/download/details.aspx?id=8621](http://www.microsoft.com/en-us/download/details.aspx?id=8621)  
