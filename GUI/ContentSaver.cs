using NetMentoring_HttpClient.Interfaces;
using System;
using System.IO;
using System.Linq;


namespace GUI
{
    public class ContentSaver : IContentSaver
    {
        private readonly DirectoryInfo rootDirectory;

        public ContentSaver(DirectoryInfo rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public void SaveHtmlDocument(Uri uri, string name, Stream documentStream)
        {
            try
            {
                string directoryPath = CombineLocations(rootDirectory, uri);
                Directory.CreateDirectory(directoryPath);
                name = RemoveInvalidSymbols(name);
                string fileFullPath = Path.Combine(directoryPath, name);

                SaveToFile(documentStream, fileFullPath);
            }
            finally
            {
                documentStream.Close();
            }
        }

        public void SaveFile(Uri uri, Stream fileStream)
        {
            try
            {
                string fileFullPath = CombineLocations(rootDirectory, uri);
                var directoryPath = Path.GetDirectoryName(fileFullPath);
                Directory.CreateDirectory(directoryPath);
                if (Directory.Exists(fileFullPath))
                {
                    fileFullPath = Path.Combine(fileFullPath, Guid.NewGuid().ToString());
                }
                SaveToFile(fileStream, fileFullPath);
                
            }
            finally
            {
                fileStream.Close();
            }
        }

        private void SaveToFile(Stream stream, string fileFullPath)
        {
            using (var createdFileStream = File.Create(fileFullPath))
            {
                stream.CopyTo(createdFileStream);
            }
        }

        private string CombineLocations(DirectoryInfo directory, Uri uri)
        {
            return Path.Combine(directory.FullName, uri.Host) + uri.LocalPath.Replace("/", @"\");
        }

        private string RemoveInvalidSymbols(string filename)
        {
            var invalidSymbols = Path.GetInvalidFileNameChars();
            return new string(filename.Where(c => !invalidSymbols.Contains(c)).ToArray());
        }
    }
}
