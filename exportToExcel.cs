using MySql.Data.MySqlClient;
using Microsoft.Office.Interop.Excel;

namespace Data
{
    class exportToExcel
    {

        static Application app;
        static Workbook wb;
        static Worksheet ws;
        static void excelExport(string tableName)
        {
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;password=2358;database=sessions;charset=utf8");
            con.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            System.Data.DataTable dTable = new System.Data.DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();
            string sqlSelectAll = "SELECT * from " + tableName;
            adapter.SelectCommand = new MySqlCommand(sqlSelectAll, con);

            adapter.Fill(dTable);
            makeTable(dTable, tableName);
            con.Close();
        }
        static void makeTable(System.Data.DataTable table, string tableName)
        {

            ws = (Worksheet)wb.Worksheets.Add();

            for (int i = 0; i < table.Columns.Count; i++)
            {

                ws.Cells[1, i + 1] = table.Columns[i].ColumnName;
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {

                for (int j = 0; j < table.Columns.Count; j++)
                {
                    ws.Cells[i + 2, j + 1] = table.Rows[i][j].ToString();
                }
            }
            ws.Name = tableName;


        }
        public static void export()
        {
            app = new Application();
            app.Visible = false;
            wb = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);

            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;password=2358;database=sessions;charset=utf8");
            con.Open();
            string sql = "show tables";
            MySqlCommand temp = new MySqlCommand(sql, con);
            MySqlDataReader rdr = temp.ExecuteReader();
            while (rdr.Read())
            {
                excelExport(rdr.GetString(0));
            }

            wb.SaveAs(@"D:\tables.xlsx");
            wb.Close();
        }
    }
}
