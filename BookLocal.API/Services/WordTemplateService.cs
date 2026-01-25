using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace BookLocal.API.Services
{
    public interface IWordTemplateService
    {
        byte[] GenerateDocument(string templatePath, Dictionary<string, string> replacements);
    }

    public class WordTemplateService : IWordTemplateService
    {
        public byte[] GenerateDocument(string templatePath, Dictionary<string, string> replacements)
        {
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template not found at {templatePath}");
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fileStream.CopyTo(memoryStream);
                }

                memoryStream.Position = 0;

                using (var doc = WordprocessingDocument.Open(memoryStream, true))
                {
                    var body = doc.MainDocumentPart.Document.Body;

                    foreach (var paragraph in body.Descendants<Paragraph>())
                    {
                        ReplaceInParagraph(paragraph, replacements);
                    }

                    doc.Save();
                }

                return memoryStream.ToArray();
            }
        }

        private void ReplaceInParagraph(Paragraph paragraph, Dictionary<string, string> replacements)
        {
            var textNodes = paragraph.Descendants<Text>().ToList();
            if (textNodes.Count == 0) return;

            bool changed = true;
            while (changed)
            {
                changed = false;

                string fullText = string.Join("", textNodes.Select(t => t.Text));

                foreach (var replacement in replacements)
                {
                    int index = fullText.IndexOf(replacement.Key);
                    if (index != -1)
                    {
                        var (startNode, startIdx) = GetNodeInfo(textNodes, index);
                        var (endNode, endIdx) = GetNodeInfo(textNodes, index + replacement.Key.Length - 1);

                        if (startNode == null || endNode == null) continue;

                        if (startNode == endNode)
                        {
                            string text = startNode.Text;
                            startNode.Text = text.Remove(startIdx, replacement.Key.Length).Insert(startIdx, replacement.Value);
                        }
                        else
                        {
                            string startText = startNode.Text;
                            startNode.Text = startText.Substring(0, startIdx) + replacement.Value;

                            int validStartNodeIndex = textNodes.IndexOf(startNode);
                            int validEndNodeIndex = textNodes.IndexOf(endNode);

                            for (int i = validStartNodeIndex + 1; i < validEndNodeIndex; i++)
                            {
                                textNodes[i].Text = string.Empty;
                            }

                            string endText = endNode.Text;
                            int lengthInEndNode = endIdx + 1; 
                            endNode.Text = endText.Substring(lengthInEndNode);
                        }

                        changed = true;
                        break;
                    }
                }
            }
        }

        private (Text node, int index) GetNodeInfo(List<Text> nodes, int globalIndex)
        {
            int currentPos = 0;
            foreach (var node in nodes)
            {
                if (globalIndex < currentPos + node.Text.Length)
                {
                    return (node, globalIndex - currentPos);
                }
                currentPos += node.Text.Length;
            }
            return (null, -1);
        }
    }
}
