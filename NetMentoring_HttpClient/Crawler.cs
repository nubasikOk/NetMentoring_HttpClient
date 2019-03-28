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
        private const string htmlDocumentMediaType = "text/html";
        private readonly ISet<Uri> visitedUrls = new HashSet<Uri>();
        private readonly ILogger logger;
        private readonly IContentSaver contentSaver;
        private readonly IDomainConstraint domainConstraint;

        public int MaxDeepLevel { get; set; }


        public Crawler(IContentSaver contentSaver,int maxDeepLevel, IDomainConstraint domainConstraint,  ILogger logger)
        {
            if (maxDeepLevel < 0)
            {
                throw new ArgumentException($"{nameof(maxDeepLevel)} can't be less than 0");
            }
            this.contentSaver = contentSaver;
            this.logger = logger;
            this.domainConstraint = domainConstraint;
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
            if (!IsAcceptableUri(uri, domainConstraint))
            {
                return;
            }
            var response = httpClient.GetAsync(uri).Result;
            logger.Log($"File loaded: {uri}");
            contentSaver.SaveFile(uri, response.Content.ReadAsStreamAsync().Result);
        }

        private void ProcessHtmlDocument(HttpClient httpClient, Uri uri, int level)
        {
            logger.Log($"Url founded: {uri}");
            if (!IsAcceptableUri(uri, domainConstraint))
            {
                return;
            }

            var response =  httpClient.GetAsync(uri).Result;
            var document = new HtmlDocument();
            var memoryStream = response.Content.ReadAsStreamAsync().Result;
            document.Load(memoryStream, Encoding.UTF8);
            logger.Log($"Html loaded: {uri}");
            memoryStream.Seek(0, SeekOrigin.Begin);
            contentSaver.SaveHtmlDocument(uri, GetDocumentFileName(document), memoryStream);

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


        private bool IsAttributeWithLink(HtmlAttribute attribute)
        {
            return attribute.Name == "src" || attribute.Name == "href";
        }

        private bool IsAcceptableUri(Uri uri, IDomainConstraint constraint)
        {
            return constraint.IsAcceptable(uri);
        }
    }
}
