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
    public partial class RoleEditor : Form
    {
        public RoleEditor()
        {
            InitializeComponent();
            roles = new List<Role>();
        }
        private void roleGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                CodeEditor editor;
                if (roleGridView.Rows[e.RowIndex].Tag != null)
                    editor = new CodeEditor(roleGridView.Rows[e.RowIndex].Tag.ToString(), this);
                else
                {
                    Stream stream = System.Reflection.Assembly.GetEntryAssembly().
                        GetManifestResourceStream("MafiaGame.Resources.UserDefinedTemplate.cs");
                    editor = new CodeEditor(new StreamReader(stream).ReadToEnd(), this);
                }
                editor.ShowDialog();
            }
        }
         private List<Role> roles;

        private void новыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            roleGridView.Rows.Clear();
        }

        private void открытьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Кастомные пакеты для игры в мафию (*.ppak)|*.ppak";
            dialog.ShowDialog();
            if (File.Exists(dialog.FileName))
            {
                try
                {
                    Pack loadedPack = new Pack();
                    loadedPack.LoadPack(dialog.FileName);
                    roleGridView.Rows.Clear();
                    foreach(Role r in loadedPack.Roles)
                    {
                        roleGridView.Rows.Add(r.Name, r.Singular);
                        roleGridView.Rows[roleGridView.Rows.Count - 1].Tag = r.RoleCode;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Неверный файл, загрузка отменена", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pack newPack = new Pack();
            for (int i = 0; i < roleGridView.Rows.Count; i++)
            {
                if (roleGridView.Rows[i].Cells[0].Value == null || roleGridView.Rows[i].Cells[0].Value.ToString() == "")
                {
                    MessageBox.Show("Заполните все необходимые поля", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Role newRole = new Role(roleGridView.Rows[i].Cells[0].Value.ToString(), (bool)roleGridView.Rows[i].Cells[1].Value, roleGridView.Rows[i].Tag.ToString());
                string assemblyResult = newRole.AssembleRole();
                if (assemblyResult == string.Empty)
                    newPack.Roles.Add(newRole);
                else
                {
                    MessageBox.Show("Ошибка при сборке в роли \"" + roleGridView.Rows[i].Cells[0].Value.ToString()
                        + "\", исправьте следующие ошибки перед сборкой пакета:\r\n"
                        + assemblyResult, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Кастомные пакеты для игры в мафию (*.ppak)|*.ppak";
            dialog.FileName = "mafiaPack.ppak";
            dialog.ShowDialog();
            if (!dialog.FileName.Contains(".ppak"))
                dialog.FileName += ".ppak";
            newPack.SavePack(dialog.FileName);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            roleGridView.Rows.Add("", false);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            roleGridView.Rows.Remove(roleGridView.CurrentRow);
        }
    }
}
