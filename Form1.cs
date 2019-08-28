using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Computer_Theory___Minimize_an_Automaton
{
    public partial class Form1 : Form
    {
        JffReader jff_handler;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //jff_handler = new JffReader("C:\\Users\\DELL\\Documents\\automata.jff");
            jff_handler = null;
            txtJFFFilePath.Text = "C:\\Users\\DELL\\Documents\\Jffs\\DFA_1.jff";
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if(txtJFFFilePath.Text == string.Empty)
                {
                    lblState.Text = "Warning, empty file path while loading jff file, provide a valid file path";
                    return;
                }
                jff_handler = new JffReader(txtJFFFilePath.Text);
                lblState.Text = "An Automaton has been loaded!";
            }
            catch(Exception exception)
            {
                lblState.Text = "An error ocurred while loading the automaton.\nError message: "+exception.Message;
            }
    
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtExportFilePath.Text == string.Empty)
                {
                    lblState.Text = "Warning, empty file path while saving the automaton, provide a valid file path";
                    return;
                }
                
                if(jff_handler == null)
                {
                    lblState.Text = "Warning, No Automaton is Loaded. \nTry loading first an Automaton and try again";
                    return;
                }

                jff_handler.Save_jff_file(txtExportFilePath.Text);
                lblState.Text = "The loaded Automaton has been saved!";
            }
            catch (Exception exception)
            {
                lblState.Text = "An error ocurred while saving the automaton.\nError message: " + exception.Message;
            }
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            if (jff_handler == null)
            {
                lblState.Text = "Warning, No Automaton is Loaded. \nLoad an Automaton first and then try to minimize";
                return;
            }

            lblState.Text = jff_handler.Minimize_Automaton();
        }
    }
}
