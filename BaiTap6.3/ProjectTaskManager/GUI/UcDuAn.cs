using System;
using System.Windows.Forms;
using ProjectTaskManager.BUS;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.GUI
{
    public class UcDuAn : UserControl, IRefreshable
    {
        private readonly DuAnBUS bus = new DuAnBUS();
        private readonly DataGridView dgv = UI.Grid();
        private readonly TextBox txtTen = new TextBox();
        private readonly DateTimePicker dtpBD = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker dtpKT = new DateTimePicker { Format = DateTimePickerFormat.Short };
        private readonly ComboBox cboTT = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private int maDuAn = 0;

        public UcDuAn()
        {
            var pnl = new Panel { Dock = DockStyle.Bottom, Height = 140 };
            txtTen.SetBounds(100, 12, 400, 24);
            dtpBD.SetBounds(100, 48, 130, 24);
            dtpKT.SetBounds(370, 48, 130, 24);
            cboTT.SetBounds(100, 84, 200, 24);
            cboTT.Items.AddRange(TrangThaiDuAn.All);
            cboTT.SelectedIndex = 0;

            pnl.Controls.AddRange(new Control[]
            {
                UI.Lbl("Tên dự án", 10, 12), txtTen,
                UI.Lbl("Ngày bắt đầu", 10, 48), dtpBD,
                UI.Lbl("Ngày kết thúc", 270, 48), dtpKT,
                UI.Lbl("Trạng thái", 10, 84), cboTT,
                UI.Btn("Thêm", 540, 10, 90, (s, e) => Run(() => { bus.Them(Read()); UI.Info("Đã thêm dự án."); })),
                UI.Btn("Sửa", 540, 48, 90, (s, e) => Run(() => { bus.Sua(Read()); UI.Info("Đã cập nhật dự án."); })),
                UI.Btn("Xóa", 540, 86, 90, (s, e) => Run(() =>
                {
                    if (maDuAn > 0 && UI.Confirm("Xóa dự án này cùng toàn bộ công việc bên trong?")) { bus.Xoa(maDuAn); Clear(); }
                })),
                UI.Btn("Làm mới", 640, 10, 90, (s, e) => { Clear(); RefreshData(); })
            });

            Controls.Add(dgv);   // Fill thêm trước
            Controls.Add(pnl);   // Bottom thêm sau
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.CurrentRow?.DataBoundItem is DuAnDTO d)
                {
                    maDuAn = d.MaDuAn; txtTen.Text = d.TenDuAn;
                    dtpBD.Value = d.NgayBatDau ?? DateTime.Today;
                    dtpKT.Value = d.NgayKetThuc ?? DateTime.Today;
                    cboTT.SelectedItem = d.TrangThai;
                }
            };
            RefreshData();
        }

        private DuAnDTO Read() => new DuAnDTO
        {
            MaDuAn = maDuAn, TenDuAn = txtTen.Text.Trim(),
            NgayBatDau = dtpBD.Value.Date, NgayKetThuc = dtpKT.Value.Date,
            TrangThai = cboTT.SelectedItem?.ToString()
        };

        private void Clear() { maDuAn = 0; txtTen.Clear(); cboTT.SelectedIndex = 0; }

        private void Run(Action a)
        {
            try { a(); RefreshData(); } catch (Exception ex) { UI.Error(ex); }
        }

        public void RefreshData()
        {
            try
            {
                dgv.DataSource = bus.GetAll();
                UI.Headers(dgv, "MaDuAn", "Mã", "TenDuAn", "Tên dự án", "NgayBatDau", "Bắt đầu",
                                "NgayKetThuc", "Kết thúc", "TrangThai", "Trạng thái");
            }
            catch (Exception ex) { UI.Error(ex); }
        }
    }
}
