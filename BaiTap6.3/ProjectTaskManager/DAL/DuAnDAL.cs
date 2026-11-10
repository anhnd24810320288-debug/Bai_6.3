using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.DAL
{
    public class DuAnDAL
    {
        public List<DuAnDTO> GetAll()
        {
            var list = new List<DuAnDTO>();
            const string sql = "SELECT MaDuAn, TenDuAn, NgayBatDau, NgayKetThuc, TrangThai FROM DuAn ORDER BY MaDuAn";
            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new DuAnDTO
                        {
                            MaDuAn = r.GetInt32(0),
                            TenDuAn = r.GetString(1),
                            NgayBatDau = r.IsDBNull(2) ? (DateTime?)null : r.GetDateTime(2),
                            NgayKetThuc = r.IsDBNull(3) ? (DateTime?)null : r.GetDateTime(3),
                            TrangThai = r.IsDBNull(4) ? null : r.GetString(4)
                        });
                    }
                }
            }
            return list;
        }

        public int Insert(DuAnDTO d)
        {
            const string sql = @"INSERT INTO DuAn (TenDuAn, NgayBatDau, NgayKetThuc, TrangThai)
                                 VALUES (@Ten, @BD, @KT, @TT)";
            return DbHelper.ExecuteNonQuery(sql,
                DbHelper.P("@Ten", d.TenDuAn), DbHelper.P("@BD", d.NgayBatDau),
                DbHelper.P("@KT", d.NgayKetThuc), DbHelper.P("@TT", d.TrangThai));
        }

        public int Update(DuAnDTO d)
        {
            const string sql = @"UPDATE DuAn SET TenDuAn=@Ten, NgayBatDau=@BD, NgayKetThuc=@KT, TrangThai=@TT
                                 WHERE MaDuAn=@Ma";
            return DbHelper.ExecuteNonQuery(sql,
                DbHelper.P("@Ten", d.TenDuAn), DbHelper.P("@BD", d.NgayBatDau),
                DbHelper.P("@KT", d.NgayKetThuc), DbHelper.P("@TT", d.TrangThai),
                DbHelper.P("@Ma", d.MaDuAn));
        }

        /// <summary>Xóa dự án kèm toàn bộ task & phân công (trong 1 transaction).</summary>
        public int Delete(int maDuAn)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        Exec(conn, trans, @"DELETE FROM PhanCong WHERE MaTask IN
                                            (SELECT MaTask FROM CongViec WHERE MaDuAn=@Ma)", maDuAn);
                        Exec(conn, trans, "DELETE FROM CongViec WHERE MaDuAn=@Ma", maDuAn);
                        int n = Exec(conn, trans, "DELETE FROM DuAn WHERE MaDuAn=@Ma", maDuAn);
                        trans.Commit();
                        return n;
                    }
                    catch { trans.Rollback(); throw; }
                }
            }
        }

        private static int Exec(SqlConnection c, SqlTransaction t, string sql, int ma)
        {
            using (var cmd = new SqlCommand(sql, c, t))
            {
                cmd.Parameters.AddWithValue("@Ma", ma);
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
