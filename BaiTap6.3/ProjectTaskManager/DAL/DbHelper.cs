using System;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ProjectTaskManager.DAL
{
    /// <summary>Lớp tiện ích kết nối & thực thi lệnh SQL (luôn dùng tham số - chống SQL Injection).</summary>
    public static class DbHelper
    {
        public static string ConnectionString
            => ConfigurationManager.ConnectionStrings["ProjectTaskDB"].ConnectionString;

        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);

        /// <summary>Tạo SqlParameter, tự đổi null thành DBNull.</summary>
        public static SqlParameter P(string name, object value)
            => new SqlParameter(name, value ?? DBNull.Value);

        public static DataTable ExecuteDataTable(string sql, params SqlParameter[] ps)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] ps)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] ps)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}
