

   ----------------------------------------------------------------------
           README file for Web Markup Minifier: Core 0.9.0 Beta 3

   ----------------------------------------------------------------------

          Copyright 2014 Andrey Taritsyn - http://www.taritsyn.ru
		  
		  
   ===========
   DESCRIPTION
   ===========   
   The Web Markup Minifier (abbreviated WebMarkupMin) is a .NET library 
   that contains a set of markup minifiers. The objective of this project 
   is to improve the performance of web applications by reducing the size 
   of HTML, XHTML and XML code.

   WebMarkupMin absorbed the best of existing solutions from non-microsoft 
   platforms: Juriy Zaytsev's Experimental HTML Minifier 
   (http://kangax.github.com/html-minifier/) (written in JavaScript) 
   and Sergiy Kovalchuk's HtmlCompressor 
   (http://code.google.com/p/htmlcompressor/) (written in Java).

   Minification of markup produces by removing extra whitespaces, 
   comments and redundant code (only for HTML and XHTML). In addition, 
   HTML and XHTML minifiers supports the minification of CSS code from 
   style tags and attributes, and minification of JavaScript code from 
   script tags, event attributes and hyperlinks with javascript: protocol. 
   WebMarkupMin.Core contains built-in JavaScript minifier based on the 
   Douglas Crockford's JSMin (http://github.com/douglascrockford/JSMin) 
   and built-in CSS minifier based on the Mads Kristensen's Efficient 
   stylesheet minifier 
   (http://madskristensen.net/post/Efficient-stylesheet-minification-in-C.aspx). 
   The above mentioned minifiers produce only the most simple minifications 
   of CSS and JavaScript code, but you can always install additional modules 
   that support the more powerful algorithms of minification: 
   WebMarkupMin.MsAjax (contains minifier-adapters for the Microsoft Ajax 
   Minifier - http://ajaxmin.codeplex.com) and WebMarkupMin.Yui 
   (contains minifier-adapters for YUI Compressor for .Net - 
   http://yuicompressor.codeplex.com).
   
   =============
   RELEASE NOTES
   =============
   In HTML/XHTML settings was replaced 2 properties: `MinifyEmbeddedJsTemplates`
   by `ProcessableScriptTypeList` (default is empty) and
   `MinifyDataBindAttributes` by `MinifyDataBindings` (default `true`).

   =============
   DOCUMENTATION
   =============
   See documentation on CodePlex - 
   http://webmarkupmin.codeplex.com/documentation