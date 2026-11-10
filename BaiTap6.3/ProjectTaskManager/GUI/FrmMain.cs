using System.Windows.Forms;

namespace ProjectTaskManager.GUI
{
    public class FrmMain : Form
    {
        public FrmMain()
        {
            Text = "Quản lý Công việc & Tiến độ Dự án";
            Width = 1150; Height = 720;
            StartPosition = FormStartPosition.CenterScreen;

            var tabs = new TabControl { Dock = DockStyle.Fill };
            var ucCongViec = new UcCongViec();
            var ucBaoCao = new UcBaoCao();

            tabs.TabPages.Add(Page("Công việc (List / Kanban)", ucCongViec));
            tabs.TabPages.Add(Page("Dự án", new UcDuAn()));
            tabs.TabPages.Add(Page("Thành viên", new UcThanhVien()));
            tabs.TabPages.Add(Page("Cảnh báo trễ hạn & Báo cáo", ucBaoCao));

            // Tải lại dữ liệu mỗi khi chuyển tab để thấy thay đổi từ tab khác
            tabs.SelectedIndexChanged += (s, e) =>
            {
                if (tabs.SelectedTab.Controls.Count > 0 && tabs.SelectedTab.Controls[0] is IRefreshable r)
                    r.RefreshData();
            };
            Controls.Add(tabs);
        }

        private static TabPage Page(string title, UserControl uc)
        {
            var p = new TabPage(title);
            uc.Dock = DockStyle.Fill;
            p.Controls.Add(uc);
            return p;
        }
    }

    public interface IRefreshable { void RefreshData(); }
}
