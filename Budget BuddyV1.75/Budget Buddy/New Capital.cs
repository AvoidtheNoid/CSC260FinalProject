using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Budget_Buddy
{
    public partial class New_Capital : Form
    {
        bool editing;
        Capital originalCapital;

        public New_Capital()
        {
            InitializeComponent();
        }

        public New_Capital(Capital originalCapital)
        {
            InitializeComponent();
            this.editing = true;
            this.originalCapital = originalCapital;
            textBox1.Text = originalCapital.name;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Done_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                //Add new budget with name in textbox
                if (!editing)
                {
                    Data.getActiveCList().Add(new Capital(textBox1.Text));
                }
                else
                {
                    originalCapital.name = textBox1.Text;
                }

                this.Close();
            }
        }
    }
}