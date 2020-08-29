using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class Port : Form
    {
        public Port()
        {
            InitializeComponent();
            button1.DialogResult = DialogResult.OK;
            CheckForIllegalCrossThreadCalls = false;
            //MessageBox.Show(int.MaxValue.ToString());
        }

        //string marka = "samsung"; string api = "28";  //I have wanted to realize logics. so I have wanted to see that it works correct or not.
        private void button1_Click(object sender, EventArgs e)
        {
            Form1.port_no = (int)numericUpDown1.Value;
        }
    }
}
