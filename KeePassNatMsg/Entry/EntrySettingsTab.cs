using KeePass.Forms;
using KeePassLib.Collections;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace KeePassNatMsg.Entry
{
    internal sealed class EntrySettingsTab
    {
        private readonly StringDictionaryEx _customData;
        private readonly EntryConfig _config;
        private readonly System.Drawing.Font _font;

        internal EntrySettingsTab(PwEntryForm form)
        {
            var tabControl = FindControl<TabControl>(form, "m_tabMain");
            var customDataField = typeof(PwEntryForm).GetField("m_sdCustomData",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _customData = customDataField == null ? null : customDataField.GetValue(form) as StringDictionaryEx;
            if (tabControl == null || _customData == null)
                return;

            _font = form.Font;
            _config = ReadConfig();
            tabControl.TabPages.Add(CreateTabPage());
        }

        private TabPage CreateTabPage()
        {
            var page = new TabPage("KeePassNatMsg")
            {
                AutoScroll = true,
                Font = _font,
                Padding = new Padding(8),
                UseVisualStyleBackColor = true
            };
            var group = new GroupBox
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 8, 10, 10),
                Text = "Minimum URL match accuracy"
            };

            var layout = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Dock = DockStyle.Top,
                Padding = new Padding(4, 6, 4, 4),
                RowCount = 4
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var explanation = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 0, 3, 8),
                Text = "Choose how accurately this entry URL must match the URL requested by KeePassXC-Browser."
            };

            var exact = CreateRadioButton(
                "Exact: Match the complete URL, including port, path and query string.",
                UrlMatchMode.Exact);
            var hostname = CreateRadioButton(
                "Hostname: Match the hostname and port; ignore path and query string.",
                UrlMatchMode.Hostname);
            var domain = CreateRadioButton(
                "Domain: Match the same domain and ignore port, path and query string.",
                UrlMatchMode.Domain);

            layout.Controls.Add(explanation, 0, 0);
            layout.Controls.Add(domain, 0, 1);
            layout.Controls.Add(hostname, 0, 2);
            layout.Controls.Add(exact, 0, 3);
            group.Controls.Add(layout);
            page.Controls.Add(group);
            return page;
        }

        private RadioButton CreateRadioButton(string text, UrlMatchMode mode)
        {
            var radio = new RadioButton
            {
                AutoSize = true,
                Checked = _config.UrlMatchMode == mode,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 3, 3, 5),
                Text = text,
            };
            radio.CheckedChanged += delegate
            {
                if (!radio.Checked)
                    return;

                _config.UrlMatchMode = mode;
                WriteConfig();
            };
            return radio;
        }

        private EntryConfig ReadConfig()
        {
            if (!_customData.Exists(KeePassNatMsgExt.SettingKey))
                return new EntryConfig();

            try
            {
                using (var reader = new JsonTextReader(new StringReader(_customData.Get(KeePassNatMsgExt.SettingKey))))
                {
                    return KeePassNatMsgExt.ExtInstance.NewJsonSerializer().Deserialize<EntryConfig>(reader)
                        ?? new EntryConfig();
                }
            }
            catch (JsonException)
            {
                return new EntryConfig();
            }
        }

        private void WriteConfig()
        {
            var writer = new StringWriter();
            KeePassNatMsgExt.ExtInstance.NewJsonSerializer().Serialize(writer, _config);
            _customData.Set(KeePassNatMsgExt.SettingKey, writer.ToString());
        }

        private static T FindControl<T>(Control parent, string name) where T : Control
        {
            var controls = parent.Controls.Find(name, true);
            return controls.Length == 0 ? null : controls[0] as T;
        }
    }
}
