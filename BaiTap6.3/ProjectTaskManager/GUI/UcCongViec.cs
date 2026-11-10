using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ProjectTaskManager.BUS;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.GUI
{
    /// <summary>Quản lý công việc: lọc theo dự án/trạng thái, List view + Kanban, CRUD, giao việc, chuyển trạng thái.</summary>
    public class UcCongViec : UserControl, IRefreshable
    {
        private readonly CongViecBUS cvBus = new CongViecBUS();
        private readonly DuAnBUS daBus = new DuAnBUS();

        private readonly ComboBox cboDuAn = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cboLoc = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly RadioButton rdoList = new RadioButton { Text = "List view", Checked = true };
        private readonly RadioButton rdoKanban = new RadioButton { Text = "Kanban" };

        private readonly DataGridView dgv = UI.Grid();
        private readonly TableLayoutPanel kanban = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, Visible = false };
        private readonly Dictionary<string, ListBox> cols = new Dictionary<string, ListBox>();

        private readonly TextBox txtTen = new TextBox();
        private readonly TextBox txtMoTa = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical };
        private readonly DateTimePicker dtpHan = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
        private readonly ComboBox cboUuTien = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox cboTT = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

        private CongViecDTO kanbanSelected;
        private bool loading;

        public UcCongViec()
        {
            // ---------- Thanh lọc (trên) ----------
            var top = new Panel { Dock = DockStyle.Top, Height = 46 };
            cboDuAn.SetBounds(70, 10, 240, 24);
            cboLoc.SetBounds(400, 10, 140, 24);
            cboLoc.Items.Add("(Tất cả)");
            cboLoc.Items.AddRange(TrangThaiTask.All);
            cboLoc.SelectedIndex = 0;
            rdoList.SetBounds(580, 10, 90, 24);
            rdoKanban.SetBounds(675, 10, 80, 24);
            top.Controls.AddRange(new Control[]
            {
                UI.Lbl("Dự án", 10, 10, 55), cboDuAn,
                UI.Lbl("Trạng thái", 325, 10, 70), cboLoc,
                rdoList, rdoKanban,
                UI.Btn("Tải lại", 770, 7, 80, (s, e) => LoadTasks())
            });

            // ---------- Khu vực hiển thị (giữa) ----------
            var host = new Panel { Dock = DockStyle.Fill };
            for (int i = 0; i < 4; i++) kanban.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kanban.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            kanban.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Color[] colors = { Color.Gray, Color.RoyalBlue, Color.DarkOrange, Color.SeaGreen };
            for (int i = 0; i < 4; i++)
            {
                string st = TrangThaiTask.All[i];
                kanban.Controls.Add(new Label
                {
                    Text = st, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = colors[i], ForeColor = Color.White, Font = new Font(Font, FontStyle.Bold)
                }, i, 0);
                var lb = new ListBox { Dock = DockStyle.Fill, HorizontalScrollbar = true, IntegralHeight = false };
                lb.SelectedIndexChanged += (s, e) => KanbanSelect((ListBox)s);
                cols[st] = lb;
                kanban.Controls.Add(lb, i, 1);
            }
            host.Controls.Add(dgv);
            host.Controls.Add(kanban);

            // ---------- Form chỉnh sửa (dưới) ----------
            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 175 };
            txtTen.SetBounds(90, 10, 330, 24);
            dtpHan.SetBounds(500, 10, 150, 24);
            txtMoTa.SetBounds(90, 46, 560, 60);
            cboUuTien.SetBounds(760, 10, 140, 24); cboUuTien.Items.AddRange(TrangThaiTask.DoUuTienAll); cboUuTien.SelectedIndex = 1;
            cboTT.SetBounds(760, 46, 140, 24); cboTT.Items.AddRange(TrangThaiTask.All); cboTT.SelectedIndex = 0;
            bottom.Controls.AddRange(new Control[]
            {
                UI.Lbl("Tên task", 10, 10, 75), txtTen,
                UI.Lbl("Hạn nộp", 430, 10, 65), dtpHan,
                UI.Lbl("Mô tả", 10, 46, 75), txtMoTa,
                UI.Lbl("Ưu tiên", 680, 10, 75), cboUuTien,
                UI.Lbl("Trạng thái", 680, 46, 75), cboTT,
                UI.Btn("Thêm task", 10, 125, 100, (s, e) => Run(Them)),
                UI.Btn("Sửa", 120, 125, 80, (s, e) => Run(Sua)),
                UI.Btn("Xóa", 210, 125, 80, (s, e) => Run(Xoa)),
                UI.Btn("Giao việc...", 300, 125, 110, (s, e) => GiaoViec()),
                UI.Btn("◀ Lùi trạng thái", 440, 125, 130, (s, e) => Run(() => Chuyen(-1))),
                UI.Btn("Tiến trạng thái ▶", 580, 125, 140, (s, e) => Run(() => Chuyen(+1)))
            });

            // Thứ tự add: Fill trước, rồi Bottom, rồi Top
            Controls.Add(host);
            Controls.Add(bottom);
            Controls.Add(top);

            dgv.SelectionChanged += (s, e) => { if (!loading) FillForm(Selected()); };
            cboDuAn.SelectedIndexChanged += (s, e) => { if (!loading) LoadTasks(); };
            cboLoc.SelectedIndexChanged += (s, e) => { if (!loading) LoadTasks(); };
            rdoKanban.CheckedChanged += (s, e) => { kanban.Visible = rdoKanban.Checked; dgv.Visible = !rdoKanban.Checked; };

            RefreshData();
        }

        // ---------------- Tải dữ liệu ----------------
        public void RefreshData()
        {
            try
            {
                loading = true;
                int oldId = (cboDuAn.SelectedItem as DuAnDTO)?.MaDuAn ?? 0;
                cboDuAn.Items.Clear();
                cboDuAn.Items.Add(new DuAnDTO { MaDuAn = 0, TenDuAn = "-- Tất cả dự án --" });
                foreach (var d in daBus.GetAll()) cboDuAn.Items.Add(d);
                cboDuAn.SelectedIndex = 0;
                foreach (DuAnDTO d in cboDuAn.Items) if (d.MaDuAn == oldId) cboDuAn.SelectedItem = d;
                loading = false;
                LoadTasks();
            }
            catch (Exception ex) { loading = false; UI.Error(ex); }
        }

        private void LoadTasks()
        {
            try
            {
                loading = true;
                int? maDA = (cboDuAn.SelectedItem as DuAnDTO)?.MaDuAn;
                if (maDA == 0) maDA = null;
                string tt = cboLoc.SelectedIndex <= 0 ? null : cboLoc.SelectedItem.ToString();
                var list = cvBus.GetByFilter(maDA, tt);

                dgv.DataSource = list;
                UI.Headers(dgv, "MaTask", "Mã", "TenTask", "Tên task", "TenDuAn", "Dự án", "HanNop", "Hạn nộp",
                                "DoUuTien", "Ưu tiên", "TrangThai", "Trạng thái", "NguoiThucHien", "Người thực hiện");
                dgv.Columns["HanNop"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                ColorRows();

                foreach (var lb in cols.Values) { lb.Items.Clear(); }
                foreach (var t in list)
                    if (cols.ContainsKey(t.TrangThai)) cols[t.TrangThai].Items.Add(t);
                foreach (var lb in cols.Values) lb.ClearSelected();
                kanbanSelected = null;
            }
            catch (Exception ex) { UI.Error(ex); }
            finally { loading = false; }
        }

        /// <summary>Tô đỏ task quá hạn (chưa Done).</summary>
        private void ColorRows()
        {
            foreach (DataGridViewRow r in dgv.Rows)
            {
                if (r.DataBoundItem is CongViecDTO t && t.HanNop.HasValue && t.HanNop < DateTime.Now && t.TrangThai != "Done")
                    r.DefaultCellStyle.BackColor = Color.MistyRose;
                else if (r.DataBoundItem is CongViecDTO d && d.TrangThai == "Done")
                    r.DefaultCellStyle.BackColor = Color.Honeydew;
            }
        }

        // ---------------- Chọn task ----------------
        private void KanbanSelect(ListBox src)
        {
            if (loading || src.SelectedItem == null) return;
            loading = true;
            foreach (var lb in cols.Values) if (lb != src) lb.ClearSelected();
            loading = false;
            kanbanSelected = src.SelectedItem as CongViecDTO;
            FillForm(kanbanSelected);
        }

        private CongViecDTO Selected()
            => rdoKanban.Checked ? kanbanSelected : dgv.CurrentRow?.DataBoundItem as CongViecDTO;

        private void FillForm(CongViecDTO t)
        {
            if (t == null) return;
            txtTen.Text = t.TenTask;
            txtMoTa.Text = t.MoTa;
            dtpHan.Value = t.HanNop ?? DateTime.Now;
            cboUuTien.SelectedItem = t.DoUuTien;
            cboTT.SelectedItem = t.TrangThai;
        }

        private CongViecDTO ReadForm(int maTask)
        {
            int maDA = (cboDuAn.SelectedItem as DuAnDTO)?.MaDuAn ?? 0;
            if (maTask > 0) maDA = Selected()?.MaDuAn ?? maDA;   // sửa: giữ nguyên dự án của task
            return new CongViecDTO
            {
                MaTask = maTask, MaDuAn = maDA,
                TenTask = txtTen.Text.Trim(), MoTa = txtMoTa.Text.Trim(),
                HanNop = dtpHan.Value, DoUuTien = cboUuTien.SelectedItem?.ToString(),
                TrangThai = cboTT.SelectedItem?.ToString()
            };
        }

        // ---------------- Nghiệp vụ ----------------
        private void Run(Action a)
        {
            try { a(); LoadTasks(); } catch (Exception ex) { UI.Error(ex); }
        }

        private void Them()
        {
            var t = ReadForm(0);
            if (t.MaDuAn == 0) throw new ArgumentException("Hãy chọn một dự án cụ thể ở ô 'Dự án' phía trên trước khi thêm task.");
            cvBus.Them(t);
            UI.Info("Đã tạo công việc mới trong dự án.");
        }

        private void Sua()
        {
            var sel = Selected() ?? throw new ArgumentException("Chưa chọn công việc cần sửa.");
            cvBus.Sua(ReadForm(sel.MaTask));
            UI.Info("Đã cập nhật công việc.");
        }

        private void Xoa()
        {
            var sel = Selected() ?? throw new ArgumentException("Chưa chọn công việc cần xóa.");
            if (UI.Confirm($"Xóa công việc \"{sel.TenTask}\"?")) cvBus.Xoa(sel.MaTask);
        }

        private void Chuyen(int buoc)
        {
            var sel = Selected() ?? throw new ArgumentException("Hãy chọn một công việc.");
            string moi = CongViecBUS.TrangThaiKe(sel.TrangThai, buoc);
            if (moi == null) throw new ArgumentException("Công việc đã ở trạng thái đầu/cuối, không thể chuyển tiếp.");
            cvBus.ChuyenTrangThai(sel.MaTask, moi);   // UPDATE CongViec SET TrangThai=@TT WHERE MaTask=@Ma
        }

        private void GiaoViec()
        {
            var sel = Selected();
            if (sel == null) { UI.Info("Hãy chọn một công việc để giao."); return; }
            using (var f = new FrmPhanCong(sel))
                if (f.ShowDialog(this) == DialogResult.OK) LoadTasks();
        }
    }
}
