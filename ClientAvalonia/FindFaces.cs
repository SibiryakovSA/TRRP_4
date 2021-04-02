using System;
using System.IO;
using System.Net.Http;

namespace ClientAvalonia
{
    public static class FindFaces
    {
        public static bool Find(string imagePath, string serverHost)
        {
            try
            {
                var filename = Path.GetFileName(imagePath);
                var extension = Path.GetExtension(imagePath);
            
                var client = new HttpClient();
                var content = new MultipartFormDataContent();
                content.Headers.Add("input-type", "file");
                content.Add(new StreamContent(
                        File.OpenRead(imagePath)), 
                    "image", 
                    filename
                );
            
                var res = client.PostAsync(serverHost, content).GetAwaiter().GetResult();
                var resString = new StreamReader(res.Content.ReadAsStream()).ReadToEnd();
                resString = resString.Split('[')[1].Split(']')[0];
                resString = resString.Replace("\"", "");
                var listImgs = resString.Split(',');

                if (listImgs.Length > 0)
                {
                    Directory.CreateDirectory("result");
                    foreach (var imgStr in listImgs)
                    {
                        var bytes = Convert.FromBase64String(imgStr);
                        var name = Path.GetRandomFileName() + extension;
                        File.WriteAllBytes("result/" + name, bytes);
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }
    }
}