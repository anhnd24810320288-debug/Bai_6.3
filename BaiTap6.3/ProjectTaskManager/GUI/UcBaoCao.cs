using System;
using System.Drawing;
using System.Windows.Forms;
using ProjectTaskManager.BUS;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.GUI
{
    /// <summary>Cảnh báo công việc trễ hạn (SqlDataReader) + thống kê % hoàn thành theo thành viên.</summary>
    public class UcBaoCao : UserControl, IRefreshable
    {
        private readonly ThongKeBUS bus = new ThongKeBUS();
        private readonly DuAnBUS daBus = new DuAnBUS();
        private readonly ComboBox cboDuAn = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DataGridView dgvTre = UI.Grid();
        private readonly DataGridView dgvTK = UI.Grid();
        private readonly Label lblTre = new Label { Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.Firebrick };
        private readonly Label lblTK = new Label { Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.SeaGreen };
        private bool loading;

        public UcBaoCao()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 46 };
            cboDuAn.SetBounds(70, 10, 260, 24);
            top.Controls.AddRange(new Control[]
            {
                UI.Lbl("Dự án", 10, 10, 55), cboDuAn,
                UI.Btn("Làm mới", 345, 7, 90, (s, e) => LoadReport())
            });

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            var pnlTre = new Panel { Dock = DockStyle.Fill };
            var pnlTK = new Panel { Dock = DockStyle.Fill };
            pnlTre.Controls.Add(dgvTre); pnlTre.Controls.Add(lblTre);
            pnlTK.Controls.Add(dgvTK);   pnlTK.Controls.Add(lblTK);
            tlp.Controls.Add(pnlTre, 0, 0);
            tlp.Controls.Add(pnlTK, 0, 1);

            Controls.Add(tlp);
            Controls.Add(top);
            cboDuAn.SelectedIndexChanged += (s, e) => { if (!loading) LoadReport(); };
            RefreshData();
        }

        public void RefreshData()
        {
            try
            {
                loading = true;
                int old = (cboDuAn.SelectedItem as DuAnDTO)?.MaDuAn ?? 0;
                cboDuAn.Items.Clear();
                foreach (var d in daBus.GetAll()) cboDuAn.Items.Add(d);
                if (cboDuAn.Items.Count > 0) cboDuAn.SelectedIndex = 0;
                foreach (DuAnDTO d in cboDuAn.Items) if (d.MaDuAn == old) cboDuAn.SelectedItem = d;
                loading = false;
                LoadReport();
            }
            catch (Exception ex) { loading = false; UI.Error(ex); }
        }

        private void LoadReport()
        {
            var da = cboDuAn.SelectedItem as DuAnDTO;
            if (da == null) return;
            try
            {
                // 1) Công việc quá hạn (SqlDataReader trong DAL)
                var tre = bus.GetCongViecTreHan(da.MaDuAn);
                dgvTre.DataSource = tre;
                UI.Headers(dgvTre, "MaTask", "Mã", "TenTask", "Tên task", "HanNop", "Hạn nộp",
                                   "DoUuTien", "Ưu tiên", "TrangThai", "Trạng thái", "NguoiThucHien", "Người thực hiện");
                dgvTre.Columns["HanNop"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                foreach (DataGridViewRow r in dgvTre.Rows) r.DefaultCellStyle.BackColor = Color.MistyRose;
                lblTre.Text = $"⚠ Công việc trễ hạn (HanNop < hiện tại và chưa Done): {tre.Count}";

                // 2) Tỷ lệ hoàn thành từng thành viên (GROUP BY) + toàn dự án (ExecuteScalar)
                dgvTK.DataSource = bus.GetTyLeTheoThanhVien(da.MaDuAn);
                UI.Headers(dgvTK, "HoTen", "Thành viên", "TongTask", "Tổng task",
                                  "TaskHoanThanh", "Đã hoàn thành", "TyLeHoanThanh", "Tỷ lệ hoàn thành (%)");
                lblTK.Text = $"📊 Tiến độ dự án \"{da.TenDuAn}\": {bus.GetTyLeDuAn(da.MaDuAn):0.##}% hoàn thành";
            }
            catch (Exception ex) { UI.Error(ex); }
        }
    }
}
