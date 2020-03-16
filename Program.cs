using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTP_WEB_ADONET_Core_Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpWebRequest reqw =
                (HttpWebRequest)HttpWebRequest.Create("https://ru.wikipedia.org/wiki/%D0%92%D1%81%D0%BF%D1%8B%D1%88%D0%BA%D0%B0_COVID-19");
            HttpWebResponse resp = (HttpWebResponse)reqw.GetResponse(); //создаем объект отклика
            StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.Default);

            var parser = new HtmlParser();
            var document = parser.ParseDocument(resp.GetResponseStream());
            //Console.WriteLine(document.DocumentElement.OuterHtml);
            IElement tableElement =
                document.QuerySelector("h3:has(> span#Распространение_по_странам_и_территориям) + table > tbody");
            //Console.WriteLine(tableElement.QuerySelector("tbody").InnerHtml);
            int count = 0;
            int totalInfected = 0;
            int totalDead = 0;
            int totalRecovered = 0;
            var rows = tableElement.QuerySelectorAll("tr");
            foreach (var item in rows)
            {
                if (count != 0 && count != rows.Count() - 1)
                {
                    try
                    {
                        if (!item.Children[0].InnerHtml.Contains("Макао"))
                        {
                            totalInfected +=
                            (item.Children[1].InnerHtml != "") ? Int32.Parse(item.Children[1].InnerHtml) : 0;
                            totalDead +=
                                (item.Children[3].InnerHtml != "") ? Int32.Parse(item.Children[3].InnerHtml) : 0;
                            totalRecovered +=
                                (item.Children[4].InnerHtml != "") ? Int32.Parse(item.Children[4].InnerHtml) : 0;
                        }
                    }
                    catch (Exception)
                    {

                        // throw;
                    }
                    // Console.WriteLine(item.Children[1].InnerHtml);
                    // Console.WriteLine();
                }
                
                count++;
            }
            Console.WriteLine(totalInfected);
            Console.WriteLine(totalDead);
            Console.WriteLine(totalRecovered);

            var deadPt = ((double)totalDead / (double)totalInfected) * 100d;
            var recPt = ((double)totalRecovered / (double)totalInfected) * 100d;
            Console.WriteLine(deadPt);
            Console.WriteLine(recPt);

            string connectionString =
              ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString;
            using (SqlConnection sqlConnection =
                new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                /*SqlCommand insertCommand =
                    sqlConnection.CreateCommand();
                Console.WriteLine($"INSERT INTO [TotalStat] ([dead_percent], [recovered_percent]) VALUES ({String.Format("{0:0.##}", deadPt)}, {String.Format("{0:0.##}", recPt)})");
                insertCommand.CommandText =
                    $"INSERT INTO [TotalStat] ([dead_percent], [recovered_percent]) VALUES ({deadPt.ToString().Replace(",", ".")}, {recPt.ToString().Replace(",", ".")})";


                Console.WriteLine($"{insertCommand.ExecuteNonQuery()} rows were inserted");*/
                
                
                SqlCommand selectCommand =
                    sqlConnection.CreateCommand();
                SqlParameter deadParam = new SqlParameter("@dead_p", SqlDbType.Float);
                deadParam.Value = 3.0f;
                selectCommand.CommandText =
                  "SELECT * FROM [dbo].[TotalStat] WHERE [dead_percent] > @dead_p;SELECT 'TotalCount' AS [Total], COUNT(*) FROM [dbo].[TotalStat]";
                selectCommand.Parameters.Add(deadParam);
                SqlDataReader reader =
                  selectCommand.ExecuteReader();
                do
                {
                    int lineNumber = 0;
                    while (reader.Read())
                    {
                        if (lineNumber == 0)
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                System.Console.Write(reader.GetName(i) + "    ");
                            }
                            System.Console.WriteLine();
                        }
                        if (reader.FieldCount == 2)
                        {
                            System.Console.WriteLine(reader[0] + "    " + reader[1]);
                        } else if (reader.FieldCount == 3)
                        {
                            System.Console.WriteLine(reader[0] + "    " + reader[1] + "    " + reader[2]);
                        }
                        lineNumber++;
                    }
                } while (reader.NextResult());
                reader.Close();
            }

            sr.Close();
        }
    }
}
