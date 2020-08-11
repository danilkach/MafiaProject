using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MafiaClassLibrary;
using System.IO;

namespace MafiaGame
{
    public partial class CodeEditor : Form
    {
        public CodeEditor(string code, RoleEditor parent)
        {
            InitializeComponent();
            form = parent;
            codeBox.Text = sourceCode = code;
        }
        private void собратьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string errors = new Role()
                {
                    RoleCode = codeBox.Text
                }.AssembleRole();
            errorBox.Text = string.Empty;
            if (errors == string.Empty)
            {
                errorBox.Text = "Скомпилировано успешно";
            }
            else
                errorBox.Text = errors;
        }

        private void CodeEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Сохранить изменения?", "Внимание"
                , MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                form.roleGridView.CurrentRow.Tag = codeBox.Text;
            else if (result == DialogResult.No)
                form.roleGridView.CurrentRow.Tag = sourceCode;
            else
                e.Cancel = true;
        }

        private void отменитьИзмененияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите отменить изменения?",
                "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                codeBox.Text = sourceCode;
        }

        private void очиститьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Очистить?", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                codeBox.Text = (new StreamReader(System.Reflection.Assembly.GetEntryAssembly().
                        GetManifestResourceStream("MafiaGame.Resources.UserDefinedTemplate.cs")).ReadToEnd());
        }
        private string sourceCode;
        private RoleEditor form;
    }
}
