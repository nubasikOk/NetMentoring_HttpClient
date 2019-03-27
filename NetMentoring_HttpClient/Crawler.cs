using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;
using System.Text;
using System.IO;
using NetMentoring_HttpClient.Interfaces;

namespace NetMentoring_HttpClient
{
    public class Crawler : ICrawler
    {
        private readonly string baseUrl;
        private const string htmlDocumentMediaType = "text/html";
        private readonly ISet<Uri> visitedUrls = new HashSet<Uri>();
        private readonly ILogger logger;
        private readonly IContentSaver contentSaver;

        public int MaxDeepLevel { get; set; }


        public Crawler(string baseUrl, IContentSaver contentSaver,int maxDeepLevel, ILogger logger)
        {
            if (maxDeepLevel < 0)
            {
                throw new ArgumentException($"{nameof(maxDeepLevel)} can't be less than 0");
            }
            this.baseUrl = baseUrl;
            this.contentSaver = contentSaver;
            this.logger = logger;
            MaxDeepLevel = maxDeepLevel;

        }
        public void LoadFromUrl(string url)
        {
            visitedUrls.Clear();
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                LoadPage(httpClient, httpClient.BaseAddress, 0);
            }
        }

        private void LoadPage(HttpClient httpClient, Uri uri, int level)
        {
            if (level > MaxDeepLevel || visitedUrls.Contains(uri) || !IsValidSchema(uri.Scheme))
            {
                return;
            }
            visitedUrls.Add(uri);
            HttpResponseMessage head = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, uri)).Result;

            if (!head.IsSuccessStatusCode)
            {
                return;
            }

            if (head.Content.Headers.ContentType?.MediaType == htmlDocumentMediaType)
            {
                ProcessHtmlDocument(httpClient, uri, level);
            }
            else
            {
                ProcessFile(httpClient, uri);
            }
        }

        private void ProcessFile(HttpClient httpClient, Uri uri)
        {
            logger.Log($"File founded: {uri}");
           
            var response = httpClient.GetAsync(uri).Result;
            logger.Log($"File loaded: {uri}");
            contentSaver.SaveFile(uri, response.Content.ReadAsStreamAsync().Result);
        }

        private void ProcessHtmlDocument(HttpClient httpClient, Uri uri, int level)
        {
            logger.Log($"Url founded: {uri}");
            
            var response = httpClient.GetAsync(uri).Result;
            var document = new HtmlDocument();
            document.Load(response.Content.ReadAsStreamAsync().Result, Encoding.UTF8);
            logger.Log($"Html loaded: {uri}");
            contentSaver.SaveHtmlDocument(uri, GetDocumentFileName(document), GetDocumentStream(document));

            var attributesWithLinks = document.DocumentNode.Descendants().SelectMany(d => d.Attributes.Where(IsAttributeWithLink));
            foreach (var attributesWithLink in attributesWithLinks)
            {
                LoadPage(httpClient, new Uri(httpClient.BaseAddress, attributesWithLink.Value), level + 1);
            }
        }

        private bool IsValidSchema(string schema)
        {
            return schema == "http" || schema == "https";
        }

        private string GetDocumentFileName(HtmlDocument document)
        {
            return document.DocumentNode.Descendants("title").FirstOrDefault()?.InnerText + ".html";
        }

        private Stream GetDocumentStream(HtmlDocument document)
        {
            MemoryStream memoryStream = new MemoryStream();
            document.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        private bool IsAttributeWithLink(HtmlAttribute attribute)
        {
            return attribute.Name == "src" || attribute.Name == "href";
        }
    }
}
