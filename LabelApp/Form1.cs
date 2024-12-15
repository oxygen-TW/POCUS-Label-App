using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json;

namespace LabelApp
{
    public partial class Form1 : Form
    {
        private int current_id;
        private string rootPath = ".\\output\\";
        private string TempFile = ".\\temp";
        private Dictionary<int, int> videoList;
        private int totalCount = 123;

        public Form1()
        {
            InitializeComponent();
            videoList = GenerateDict(totalCount);
            Console.WriteLine(videoList.Count);

            for (int i = 0; i <= totalCount; i++)
            {
                goto_box1.Items.Add(i);
            }

            current_id = 0;

            if (File.Exists(TempFile))
            {
                DialogResult res;
                res = MessageBox.Show("有暫存的進度，是否載入先前的結果?", "繼續", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (res == DialogResult.Yes)
                {
                    var results = File.ReadAllText(TempFile);
                    videoList = JsonConvert.DeserializeObject<Dictionary<int, int>>(results);
                    Console.WriteLine("Load json.");
                    updateStatusLabel();
                }
                else
                {
                    File.Delete(TempFile);
                    Console.WriteLine("Temp file has been deleted.");
                }
            }
            else
            {
                //尚未執行過
                string info_msg = "您好，非常感謝您協助本研究的進行！本研究旨在利用電腦視覺技術，對肺部POCUS影像進行COVID-19的輔助診斷。然而，我們目前使用的公開資料集品質不穩定，因此誠請您協助進行資料集品質的標註。\r\n\r\n" +
                    "本次資料集共包含124個影片，請您協助判讀這些超音波影片是否為高品質（合格）資料。合格的標準為：假設該超音波影片屬於您的患者，且其主訴與呼吸系統相關，此影片應具有足夠的資訊和正確的解剖定位，以提供診斷線索。\r\n\r\n" +
                    "再次感謝您的協助，若標記途中有任何疑問，隨時歡迎與我聯絡\n" +
                    "r12945029@ntu.edu.tw 劉子豪";

                MessageBox.Show(info_msg, "說明", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }
        private static Dictionary<int, int> GenerateDict(int totalCount)
        {
            Dictionary<int, int> fileDictionary = new Dictionary<int, int>();

            for (int i = 0; i < totalCount+1; i++)
            {
                fileDictionary[i] = -1;
            }

            return fileDictionary;
        }
        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {
            Console.WriteLine((rootPath + current_id.ToString() + ".mp4").Trim());
            axWindowsMediaPlayer1.URL = rootPath + current_id.ToString() + ".mp4";
        }

        public static void WriteDictionaryToJsonFile<T, U>(Dictionary<T, U> dictionary, string filePath)
        {
            // 將字典轉換為 JSON 字符串，使用縮排格式
            string jsonString = JsonConvert.SerializeObject(dictionary, Newtonsoft.Json.Formatting.Indented);

            // 將 JSON 字符串寫入檔案
            File.WriteAllText(filePath, jsonString);
        }

        private void updateStatusLabel()
        {
            string BaseText = "Label=";
            if (videoList[current_id] == -1)
            {
                AnsLabel.Text = BaseText + "N/L";
            }
            else
            {
                Console.WriteLine(videoList[current_id]);
                AnsLabel.Text = BaseText + (videoList[current_id] == 1 ? "High Quality": "Low Quality");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            videoList[current_id] = 1;
            updateStatusLabel();
            checkFinished();
            save_to_tempfile();
        }

        private void checkEnd()
        {
            BackBtn.Enabled = current_id - 1 < 0 ? false : true;
            NextBtn.Enabled = current_id + 1 > totalCount ? false : true;
        }

        private void back_Click(object sender, EventArgs e)
        {
            current_id--;
            IdLabel.Text = "ID=" + current_id.ToString();
            axWindowsMediaPlayer1.URL = rootPath + current_id.ToString() + ".mp4";
            axWindowsMediaPlayer1.Ctlcontrols.play();
            updateStatusLabel();
            checkFinished();
            checkEnd();

        }

        private void next_Click(object sender, EventArgs e)
        {
            current_id ++;
            IdLabel.Text = "ID=" + current_id.ToString();
            axWindowsMediaPlayer1.URL = rootPath + current_id.ToString() + ".mp4";
            axWindowsMediaPlayer1.Ctlcontrols.play();
            updateStatusLabel();
            checkFinished();
            checkEnd();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            videoList[current_id] = 0;
            updateStatusLabel();
            checkFinished();
            save_to_tempfile();
        }

        private void goto_box1_SelectedValueChanged(object sender, EventArgs e)
        {
            current_id = (int)goto_box1.SelectedItem;
            IdLabel.Text = "ID=" + current_id.ToString();
            axWindowsMediaPlayer1.URL = rootPath + current_id.ToString() + ".mp4";
            axWindowsMediaPlayer1.Ctlcontrols.play();
            updateStatusLabel();
            checkFinished();
            checkEnd();
        }

        private void checkFinished()
        {
            if (videoList.ContainsValue(-1))
            {
                saveBtn.Enabled = false;
            }
            else
            {
                saveBtn.Enabled = true;
            }
        }
        private void saveBtn_Click(object sender, EventArgs e)
            {SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "存檔",
                    FileName = "QC_label",          // 設定預設檔名
                    Filter = "JSON files (*.json)|*.json", // 限制副檔名為 .json
                    DefaultExt = "json",             // 預設副檔名
                    AddExtension = true              // 自動添加副檔名
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    Console.WriteLine("選擇的存檔路徑: " + filePath);
                    WriteDictionaryToJsonFile(videoList, filePath);
                    MessageBox.Show("存檔成功", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Console.WriteLine("存檔取消");
                MessageBox.Show("存檔取消", "取消", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            }

        private void save_to_tempfile()
        {
            WriteDictionaryToJsonFile(videoList, TempFile);
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

    }
}
