using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ProjectTaskManager.BUS;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.GUI
{
    /// <summary>Hộp thoại giao việc: tick chọn một hoặc nhiều thành viên cho 1 task.</summary>
    public class FrmPhanCong : Form
    {
        private readonly CheckedListBox clb = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true };
        private readonly PhanCongBUS pcBus = new PhanCongBUS();
        private readonly int maTask;

        public FrmPhanCong(CongViecDTO task)
        {
            maTask = task.MaTask;
            Text = "Giao việc: " + task.TenTask;
            Width = 380; Height = 420;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;

            var pnl = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            var btnOk = UI.Btn("Lưu phân công", 130, 8, 120, BtnOk_Click);
            var btnCancel = UI.Btn("Hủy", 260, 8, 80, (s, e) => Close());
            pnl.Controls.AddRange(new Control[] { btnOk, btnCancel });
            Controls.Add(clb);
            Controls.Add(pnl);

            try
            {
                var daGiao = pcBus.GetThanhVienDuocGiao(maTask);
                foreach (var tv in new ThanhVienBUS().GetAll())
                    clb.Items.Add(tv, daGiao.Contains(tv.MaThanhVien));
            }
            catch (Exception ex) { UI.Error(ex); }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            try
            {
                var ds = new List<int>();
                foreach (ThanhVienDTO tv in clb.CheckedItems) ds.Add(tv.MaThanhVien);
                pcBus.GiaoViec(maTask, ds);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { UI.Error(ex); }
        }
    }
}
