using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.DAL
{
    public class CongViecDAL
    {
        // Truy vấn nền: JOIN tên dự án + gom tên các thành viên được giao
        private const string SelectBase = @"
SELECT cv.MaTask, cv.MaDuAn, cv.TenTask, cv.MoTa, cv.HanNop, cv.DoUuTien, cv.TrangThai, da.TenDuAn,
       ISNULL(STUFF((SELECT N', ' + tv.HoTen
                     FROM PhanCong pc JOIN ThanhVien tv ON tv.MaThanhVien = pc.MaThanhVien
                     WHERE pc.MaTask = cv.MaTask
                     FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, ''), N'') AS NguoiThucHien
FROM CongViec cv
JOIN DuAn da ON da.MaDuAn = cv.MaDuAn ";

        private static CongViecDTO Map(SqlDataReader r) => new CongViecDTO
        {
            MaTask = r.GetInt32(r.GetOrdinal("MaTask")),
            MaDuAn = r.GetInt32(r.GetOrdinal("MaDuAn")),
            TenTask = r.GetString(r.GetOrdinal("TenTask")),
            MoTa = r.IsDBNull(r.GetOrdinal("MoTa")) ? null : r.GetString(r.GetOrdinal("MoTa")),
            HanNop = r.IsDBNull(r.GetOrdinal("HanNop")) ? (DateTime?)null : r.GetDateTime(r.GetOrdinal("HanNop")),
            DoUuTien = r.IsDBNull(r.GetOrdinal("DoUuTien")) ? null : r.GetString(r.GetOrdinal("DoUuTien")),
            TrangThai = r.GetString(r.GetOrdinal("TrangThai")),
            TenDuAn = r.GetString(r.GetOrdinal("TenDuAn")),
            NguoiThucHien = r.GetString(r.GetOrdinal("NguoiThucHien"))
        };

        private static List<CongViecDTO> Query(string sql, params SqlParameter[] ps)
        {
            var list = new List<CongViecDTO>();
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(Map(r));
            }
            return list;
        }

        /// <summary>Lọc task theo dự án và/hoặc trạng thái (null = không lọc).</summary>
        public List<CongViecDTO> GetByFilter(int? maDuAn, string trangThai)
        {
            string sql = SelectBase + @"
WHERE (@MaDuAn IS NULL OR cv.MaDuAn = @MaDuAn)
  AND (@TrangThai IS NULL OR cv.TrangThai = @TrangThai)
ORDER BY cv.HanNop, cv.MaTask";
            return Query(sql,
                new SqlParameter("@MaDuAn", (object)maDuAn ?? DBNull.Value),
                new SqlParameter("@TrangThai", (object)trangThai ?? DBNull.Value));
        }

        /// <summary>Công việc quá hạn: HanNop &lt; hiện tại và TrangThai != 'Done' (dùng SqlDataReader).</summary>
        public List<CongViecDTO> GetOverdue(int? maDuAn)
        {
            string sql = SelectBase + @"
WHERE cv.HanNop < GETDATE() AND cv.TrangThai <> N'Done'
  AND (@MaDuAn IS NULL OR cv.MaDuAn = @MaDuAn)
ORDER BY cv.HanNop";
            return Query(sql, new SqlParameter("@MaDuAn", (object)maDuAn ?? DBNull.Value));
        }

        /// <summary>Thêm task, trả về MaTask vừa tạo.</summary>
        public int Insert(CongViecDTO t)
        {
            const string sql = @"INSERT INTO CongViec (MaDuAn, TenTask, MoTa, HanNop, DoUuTien, TrangThai)
                                 VALUES (@DA, @Ten, @MoTa, @Han, @UT, @TT);
                                 SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return Convert.ToInt32(DbHelper.ExecuteScalar(sql,
                DbHelper.P("@DA", t.MaDuAn), DbHelper.P("@Ten", t.TenTask), DbHelper.P("@MoTa", t.MoTa),
                DbHelper.P("@Han", t.HanNop), DbHelper.P("@UT", t.DoUuTien), DbHelper.P("@TT", t.TrangThai)));
        }

        public int Update(CongViecDTO t)
        {
            const string sql = @"UPDATE CongViec SET MaDuAn=@DA, TenTask=@Ten, MoTa=@MoTa, HanNop=@Han,
                                        DoUuTien=@UT, TrangThai=@TT
                                 WHERE MaTask=@Ma";
            return DbHelper.ExecuteNonQuery(sql,
                DbHelper.P("@DA", t.MaDuAn), DbHelper.P("@Ten", t.TenTask), DbHelper.P("@MoTa", t.MoTa),
                DbHelper.P("@Han", t.HanNop), DbHelper.P("@UT", t.DoUuTien), DbHelper.P("@TT", t.TrangThai),
                DbHelper.P("@Ma", t.MaTask));
        }

        /// <summary>Chuyển trạng thái bằng Parameterized SQL Update.</summary>
        public int UpdateTrangThai(int maTask, string trangThaiMoi)
            => DbHelper.ExecuteNonQuery(
                "UPDATE CongViec SET TrangThai = @TT WHERE MaTask = @Ma",
                DbHelper.P("@TT", trangThaiMoi), DbHelper.P("@Ma", maTask));

        public int Delete(int maTask)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (var c1 = new SqlCommand("DELETE FROM PhanCong WHERE MaTask=@Ma", conn, trans))
                        { c1.Parameters.AddWithValue("@Ma", maTask); c1.ExecuteNonQuery(); }
                        int n;
                        using (var c2 = new SqlCommand("DELETE FROM CongViec WHERE MaTask=@Ma", conn, trans))
                        { c2.Parameters.AddWithValue("@Ma", maTask); n = c2.ExecuteNonQuery(); }
                        trans.Commit();
                        return n;
                    }
                    catch { trans.Rollback(); throw; }
                }
            }
        }
    }
}
