using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace RTXTest
{
    public static class Util
    {
        public static T GetObjectFromCache<T>(string filePath, Func<T> cacheGenFunc)
        {
            if (File.Exists(filePath))
            {
                var serializedContent = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(serializedContent);
            }
            else
            {
                T cacheObj = cacheGenFunc();

                var serializedObjJson = JsonConvert.SerializeObject(cacheObj);

                using (var fileStream = File.Create(filePath))
                {
                    var content = Encoding.UTF8.GetBytes(serializedObjJson);
                    fileStream.Write(content, 0, content.Length);
                }

                return cacheObj;
            }
        }

    }
}
