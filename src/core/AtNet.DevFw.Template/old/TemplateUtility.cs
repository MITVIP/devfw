﻿//
// Copyright 2011 @ OPS Inc,All right reseved.
// Name:TemplateUtility.cs
// Author:newmin
// Create:2011/06/05
//

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AtNet.DevFw.Template;

namespace AtNet.DevFw.Template
{
    public sealed class TemplateUtility
    {
        internal static string Read(string templateID)
        {
            //从缓存中获取模板内容
            return TemplateCache.GetTemplateContent(templateID);
        }

        /// <summary>
        /// 使用模板引擎自带的压缩程序压缩代码
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string CompressHtml(string html)
        {
            //html = Regex.Replace(html, ">(\\s)+<", "><");
            ////html = Regex.Replace(html, "<!--[^\\[][\\s\\S]*?-->|(^?!=http:|https:)//(.+?)\r\n|\r|\n|\t|(\\s\\s)", String.Empty);
            //html = Regex.Replace(html, "<!--[^\\[][\\s\\S]*?-->|\r|\n|\t|(\\s\\s)", String.Empty);
            //return html;

            html = Regex.Replace(html, ">(\\s)+<", "><");

            //替换 //单行注释
            html = Regex.Replace(html, "[\\s|\\t]+\\/\\/[^\\n]*(?=\\n)", String.Empty);

            //替换多行注释
            //const string multCommentPattern = "";
            html = Regex.Replace(html, "/\\*[^\\*]+\\*/", String.Empty);

            //替换<!-- 注释 -->
            html = Regex.Replace(html, "<!--[^\\[][\\s\\S]*?-->", String.Empty);

            //html = Regex.Replace(html, "<!--[^\\[][\\s\\S]*?-->|(^?!=http:|https:)//(.+?)\r\n|\r|\n|\t|(\\s\\s)", String.Empty);
            html = Regex.Replace(html, "\r|\n|\t|(\\s\\s)", String.Empty);

            return html;
        }

        internal static string GetTemplateID(string filePath, TemplateNames nametype)
        {
            Match match = Regex.Match(filePath, "templates(/|\\\\)+#*(.+?)$", RegexOptions.IgnoreCase);
            if (String.IsNullOrEmpty(match.Value)) throw new Exception("模版页文件名:" + filePath + "不合法");

            string fileName = match.Groups[2].Value;
            if (!fileName.ToLower().EndsWith(".phtml") && nametype == TemplateNames.FileName)
            {
                string id = String.Format("{0}{1}",
                    match.Groups[1].Value,
                    match.Groups[2].Value)
                    .Replace('\\', '/');

                return id.Substring(0, id.LastIndexOf('.'));
            }
            else
            {
                return MD5.EncodeTo16(Regex.Replace(fileName, "/|\\\\", String.Empty).ToLower());
            }
        }

        /// <summary>
        /// 获取部分模板的编号
        /// </summary>
        /// <param name="_path"></param>
        /// <param name="tmpPath"></param>
        /// <returns></returns>
        internal static string GetPartialTemplateID(string _path, string tmpPath, out string partialFilePath)
        {
            string _filepath = tmpPath;
            string _filename = _path;

            //
            // inc/header.html
            //

            if (!_path.StartsWith("/"))
            {
                DirectoryInfo p_wrap = null,
                    p_par = null,
                    p_curr = new FileInfo(_filepath).Directory;


                //exsample path: ../../inc/top.phtml
                if (Regex.IsMatch(_path, "^\\.\\./"))
                {
                    Regex pathRegex = new Regex("\\.\\./");
                    int dirlayer = pathRegex.Matches(_path).Count;
                    _filename = pathRegex.Replace(_filename, String.Empty);

                    int i = 0;
                    do
                    {
                        if (p_par != null)
                        {
                            p_wrap = p_par;
                            p_par = p_par.Parent;
                        }
                        else
                        {
                            p_par = p_curr.Parent;
                        }

                        if (p_wrap != null)
                        {
                            p_curr = p_wrap;
                        }
                    } while (++i <= dirlayer);

                    _filename = String.Format("{0}/{1}", p_curr.Name, _filename);
                }
                else
                {
                    //
                    //exsample path: inc/top.phtml
                    //

                    do
                    {
                        if (p_par != null)
                        {
                            p_wrap = p_par;
                            p_par = p_par.Parent;
                        }
                        else
                        {
                            p_par = p_curr.Parent;
                        }

                        if (p_wrap != null)
                        {
                            p_curr = p_wrap;
                        }

                        _filename = String.Format("{0}/{1}", p_curr.Name, _filename);
                    } while (String.Compare(p_par.Name, "templates", true) != 0);
                }
            }

            //
            //TODO:测试
            //
            partialFilePath = _filename;

            return MD5.EncodeTo16(Regex.Replace(_filename, "/|\\\\", String.Empty).ToLower());
        }

        /// <summary>
        /// 显示模板信息到页面
        /// </summary>
        public static void PrintTemplatesInfo()
        {
            string templateContent = Embed.SysTemplatePage;
            StringBuilder sb = new StringBuilder();

            sb.Append(@"<style type=""text/css"">
                          table{width:100%;background:#eee;margin:0 auto;line-height:25px;color:#222;cursor:pointer}
                          table td{background:white;padding:0 8px;}
                          table th{background:#006699;color:white;}
                          table tr.hover td{background:#222;color:white;}
                          table tr.even td{}
                        </style>
                        <script type=""text/javascript"">
                             function dynamicTable(table) {
                                if (table && table.nodeName === 'TABLE') {
                                    var rows = table.getElementsByTagName('tr');
                                    for (var i = 0; i < rows.length; i++) {
                                    if (i % 2 == 1) if (!rows[i].className) rows[i].className = 'even';
                                    rows[i].onmouseover = function () {
                                        this.className = this.className.indexOf('even') != -1 ? 'hover even' : 'hover';
                                    };
                                    rows[i].onmouseout = function () {
                                        this.className = this.className.indexOf('even') == -1 ? '' : 'even';
                                    };
                                 }
                               }
                            }</script>

                        <table cellspacing=""1"" id=""templates"">
                            <tr>
                                <th style=""width:50px;""></th><th style=""width:150px;"">模板编号</th><th style=""width:80px;"">模板类型</th><th>文件名</th><th>模板注释</th><th>文件路径</th></tr>
				<!--
        		<tr><td colspan=""6"" align=""center"" style=""background:#c20000;color:white"">扩展名为“.phtml”表示为一个部分视图；部分视图只能使用ID命名</td></tr>
           		-->");

            Template tpl;
            string tplFileName, tplContent; //模板文件名,内容
            int i = 0;

            foreach (string key in TemplateCache.templateDictionary.Keys)
            {
                tpl = TemplateCache.templateDictionary[key];
                tplFileName =
                    new Regex("templates(/|\\\\)+#*(.+?)$", RegexOptions.IgnoreCase).Match(tpl.FilePath).Groups[2].Value
                        .Replace("\\", "/");
                tplContent = tpl.Content;

                sb.Append("<tr><td class=\"center\">").Append((++i).ToString()).Append("</td><td class=\"center\">")
                    .Append(key.ToLower()).Append("</td><td class=\"center\">")
                    .Append(

                        //RegexUtility.partialRegex.IsMatch(tplContent) && tplContent.IndexOf("<title>") != -1 
                        !tpl.FilePath.EndsWith(".phtml")
                            ? "<span style=\"color:#333\">模板页面</span>"
                            : "<span style=\"color:#006699\">部分视图</span>")
                    .Append("</td><td>/").Append(tplFileName).Append("</td><td>").Append(tpl.Comment)
                    .Append("</td><td>").Append(tpl.FilePath).Append("</td></tr>");
            }

            sb.Append(@"<tr><td colspan=""6"" align=""center"" style=""background:#f0f0f0;color:#333"">
						部分视图扩展名为“.phtml”,可允许格式如:
						&nbsp;&nbsp;A:${partial:""inc/header.phtml""}
						&nbsp;&nbsp;B:${partial:""/tmpdir/inc/header.phtml""}
						&nbsp;&nbsp;C:${partial:""../../inc/header.phtml""}
						</td></tr>");

            sb.Append(
                "</table><script type=\"text/javascript\">dynamicTable(document.getElementsByTagName('table')[0]);</script>");

            templateContent = TemplateRegexUtility.Replace(templateContent, match =>
            {
                switch (match.Groups[1].Value)
                {
                    case "title":
                        return "模板信息";
                    case "content":
                        return sb.ToString();
                    case "year":
                        return DateTime.Now.Year.ToString();
                }
                return String.Empty;
            });

            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
            response.Write(TemplateUtility.CompressHtml(templateContent));
            response.End();
        }
    }
}