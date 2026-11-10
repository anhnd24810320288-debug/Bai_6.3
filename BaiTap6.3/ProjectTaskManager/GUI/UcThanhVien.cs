using System;
using System.Windows.Forms;
using ProjectTaskManager.BUS;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.GUI
{
    public class UcThanhVien : UserControl, IRefreshable
    {
        private readonly ThanhVienBUS bus = new ThanhVienBUS();
        private readonly DataGridView dgv = UI.Grid();
        private readonly TextBox txtTen = new TextBox(), txtEmail = new TextBox(), txtCM = new TextBox();
        private int ma = 0;

        public UcThanhVien()
        {
            var pnl = new Panel { Dock = DockStyle.Bottom, Height = 140 };
            txtTen.SetBounds(100, 12, 300, 24);
            txtEmail.SetBounds(100, 48, 300, 24);
            txtCM.SetBounds(100, 84, 300, 24);
            pnl.Controls.AddRange(new Control[]
            {
                UI.Lbl("Họ tên", 10, 12), txtTen,
                UI.Lbl("Email", 10, 48), txtEmail,
                UI.Lbl("Chuyên môn", 10, 84), txtCM,
                UI.Btn("Thêm", 440, 10, 90, (s, e) => Run(() => { bus.Them(Read()); UI.Info("Đã thêm thành viên."); })),
                UI.Btn("Sửa", 440, 48, 90, (s, e) => Run(() => { bus.Sua(Read()); UI.Info("Đã cập nhật."); })),
                UI.Btn("Xóa", 440, 86, 90, (s, e) => Run(() =>
                {
                    if (ma > 0 && UI.Confirm("Xóa thành viên này (và các phân công liên quan)?")) { bus.Xoa(ma); Clear(); }
                })),
                UI.Btn("Làm mới", 540, 10, 90, (s, e) => { Clear(); RefreshData(); })
            });
            Controls.Add(dgv);
            Controls.Add(pnl);
            dgv.SelectionChanged += (s, e) =>
            {
                if (dgv.CurrentRow?.DataBoundItem is ThanhVienDTO t)
                { ma = t.MaThanhVien; txtTen.Text = t.HoTen; txtEmail.Text = t.Email; txtCM.Text = t.ChuyenMon; }
            };
            RefreshData();
        }

        private ThanhVienDTO Read() => new ThanhVienDTO
        { MaThanhVien = ma, HoTen = txtTen.Text.Trim(), Email = txtEmail.Text.Trim(), ChuyenMon = txtCM.Text.Trim() };

        private void Clear() { ma = 0; txtTen.Clear(); txtEmail.Clear(); txtCM.Clear(); }

        private void Run(Action a)
        {
            try { a(); RefreshData(); } catch (Exception ex) { UI.Error(ex); }
        }

        public void RefreshData()
        {
            try
            {
                dgv.DataSource = bus.GetAll();
                UI.Headers(dgv, "MaThanhVien", "Mã", "HoTen", "Họ tên", "Email", "Email", "ChuyenMon", "Chuyên môn");
            }
            catch (Exception ex) { UI.Error(ex); }
        }
    }
}
