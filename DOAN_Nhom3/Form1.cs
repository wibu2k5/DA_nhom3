using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;          // dùng SaveFileDialog/File/Path
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DOAN_Nhom3
{
    public partial class Form1 : Form
    {
        // ====== Undo/Redo state ======
        private readonly Stack<string> undoStack = new Stack<string>();
        private readonly Stack<string> redoStack = new Stack<string>();
        private bool isInternalChange = false;   // chặn vòng lặp TextChanged khi set Text bằng code
        private string previousText = string.Empty;
        private string currentFilePath = null;   // đường dẫn file hiện tại (nếu đã lưu)

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Trạng thái ban đầu
            previousText = rtbEditor.Text;
            UpdateUndoRedoButtons();

            // Gắn phím tắt Ctrl+Z / Ctrl+Y / Ctrl+S
            rtbEditor.KeyDown += rtbEditor_KeyDown;

            // Tiêu đề
            this.Text = "Trình soạn thảo cơ bản - Undo/Redo (Stack)";
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void rtbEditor_TextChanged(object sender, EventArgs e)
        {
            // Mỗi thay đổi do người dùng gõ -> lưu trạng thái trước vào undoStack
            if (isInternalChange) return;

            undoStack.Push(previousText);
            redoStack.Clear(); // thao tác mới thì Redo không còn hợp lệ
            previousText = rtbEditor.Text;

            UpdateUndoRedoButtons();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveToFile();
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            Redo();
        }

        // ====== Phím tắt ======
        private void rtbEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                Undo();
                e.SuppressKeyPress = true; // chặn Undo mặc định của RichTextBox
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                Redo();
                e.SuppressKeyPress = true; // chặn Redo mặc định
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                SaveToFile();
                e.SuppressKeyPress = true;
            }
        }

        // ====== Logic Undo/Redo/Save ======
        private void Undo()
        {
            if (undoStack.Count == 0) return;

            isInternalChange = true;

            // Lưu trạng thái hiện tại vào Redo trước khi hoàn tác
            redoStack.Push(rtbEditor.Text);

            // Khôi phục văn bản trước đó
            rtbEditor.Text = undoStack.Pop();

            isInternalChange = false;

            // Cập nhật previousText sau khi set Text bằng code
            previousText = rtbEditor.Text;

            // Đưa caret về cuối cho dễ quan sát
            rtbEditor.SelectionStart = rtbEditor.TextLength;
            rtbEditor.SelectionLength = 0;

            UpdateUndoRedoButtons();
        }

        private void Redo()
        {
            if (redoStack.Count == 0) return;

            isInternalChange = true;

            // Trước khi Redo, đẩy trạng thái hiện tại vào Undo
            undoStack.Push(rtbEditor.Text);

            // Phục hồi trạng thái đã được Undo
            rtbEditor.Text = redoStack.Pop();

            isInternalChange = false;

            previousText = rtbEditor.Text;
            rtbEditor.SelectionStart = rtbEditor.TextLength;
            rtbEditor.SelectionLength = 0;

            UpdateUndoRedoButtons();
        }

        private void SaveToFile()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentFilePath))
                {
                    File.WriteAllText(currentFilePath, rtbEditor.Text, Encoding.UTF8);
                    MessageBox.Show("Đã lưu: " + currentFilePath, "Lưu file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    using (var sfd = new SaveFileDialog
                    {
                        Title = "Lưu văn bản",
                        Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                        DefaultExt = "txt",
                        AddExtension = true,
                        FileName = "document.txt"
                    })
                    {
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllText(sfd.FileName, rtbEditor.Text, Encoding.UTF8);
                            currentFilePath = sfd.FileName;
                            this.Text = "Trình soạn thảo - " + Path.GetFileName(currentFilePath);
                            MessageBox.Show("Đã lưu: " + currentFilePath, "Lưu file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu file:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateUndoRedoButtons()
        {
            btnUndo.Enabled = undoStack.Count > 0;
            btnRedo.Enabled = redoStack.Count > 0;
        }
    }
}