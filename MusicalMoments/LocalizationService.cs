using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;

namespace MusicalMoments
{
    internal static class LocalizationService
    {
        public static void ApplyResourcesToControls(Control.ControlCollection controls, string baseName, Assembly assembly)
        {
            ResourceManager rm = new ResourceManager(baseName, assembly);
            foreach (Control control in controls)
            {
                if (control.HasChildren)
                {
                    ApplyResourcesToControls(control.Controls, baseName, assembly);
                }

                string key = $"{control.Name}ResxText";
                string resourceValue = rm.GetString(key);
                if (!string.IsNullOrEmpty(resourceValue))
                {
                    control.Text = resourceValue;
                }
            }
        }

        public static void BuildLocalizationBaseFiles(Control.ControlCollection controls, string filePath)
        {
            StringBuilder resxContent = new StringBuilder();
            resxContent.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            resxContent.AppendLine("<root>");
            resxContent.AppendLine("  <!-- 简化的头部信息 -->");
            resxContent.AppendLine("  <resheader name=\"resmimetype\">");
            resxContent.AppendLine("    <value>text/microsoft-resx</value>");
            resxContent.AppendLine("  </resheader>");
            resxContent.AppendLine("  <resheader name=\"version\">");
            resxContent.AppendLine("    <value>2.0</value>");
            resxContent.AppendLine("  </resheader>");
            resxContent.AppendLine("  <resheader name=\"reader\">");
            resxContent.AppendLine("    <value>System.Resources.ResXResourceReader, System.Windows.Forms, ...</value>");
            resxContent.AppendLine("  </resheader>");
            resxContent.AppendLine("  <resheader name=\"writer\">");
            resxContent.AppendLine("    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, ...</value>");
            resxContent.AppendLine("  </resheader>");
            resxContent.Append(BuildControlToXmlDataValue(controls));
            resxContent.AppendLine("</root>");
            File.WriteAllText(filePath, resxContent.ToString());
        }

        private static string BuildControlToXmlDataValue(Control.ControlCollection controls)
        {
            StringBuilder resxEntries = new StringBuilder();
            foreach (Control control in controls)
            {
                if (control.HasChildren)
                {
                    resxEntries.Append(BuildControlToXmlDataValue(control.Controls));
                }

                if (!string.IsNullOrEmpty(control.Name) && !string.IsNullOrEmpty(control.Text))
                {
                    string escapedControlText = control.Text.Replace("<", "&lt;").Replace(">", "&gt;");
                    resxEntries.AppendLine($"  <data name=\"{control.Name}ResxText\" xml:space=\"preserve\">");
                    resxEntries.AppendLine($"    <value>{escapedControlText}</value>");
                    resxEntries.AppendLine("  </data>");
                }
            }

            return resxEntries.ToString();
        }
    }
}
