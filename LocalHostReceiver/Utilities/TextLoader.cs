using System.Text;

namespace LocalHostReceiver.Utilities;

public class TextLoader
{
    public List<string> LoadPermittedHeaders(string filePath)
    {
        List<string> permittedHeaders = new List<string>();

        using (var fileStream = File.OpenRead(filePath))
        {
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 1024))
            {
                string line;
                while ((line = streamReader.ReadLine()!) != null)
                {
                    permittedHeaders.Add(line);
                }
            }
        }

        return permittedHeaders;
    }
}
