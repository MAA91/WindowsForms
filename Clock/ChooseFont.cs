﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Text;

namespace Clock
{
    public partial class ChooseFont : Form
    {
        public Font ChoosenFont {  get; 
            set; }

        public ChooseFont()
        {
            InitializeComponent();
            LoadFonts();
        }

        void LoadFonts()
        {
            string[] fonts = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*ttf").ToArray();
            for(int i = 0; i < fonts.Length; i++)
                fonts[i] = fonts[i].Split('\\').Last();
            comboBox.Items.AddRange(fonts);
            comboBox.SelectedIndex = 0;
        }

        private void comboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            string fontFile = $"{Directory.GetCurrentDirectory()}\\{comboBox.SelectedItem.ToString()}";
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(fontFile);
            Font font = new Font(pfc.Families[0], 36);
            labelExemple.Font = font;  
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            ChoosenFont = new Font(labelExemple.Font.FontFamily, labelExemple.Font.Size);
        }
    }
}