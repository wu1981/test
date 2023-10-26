using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using System.Xml;
using Moldau.Common;
using Moldau.QT2;
using Moldau.Config;

/// <summary>
/// DeviceApply 的摘要描述
/// </summary>
/// 

public enum ApplyFormStatus { Create = 0, TeamLeader = 1, Manager1 = 2, Manager2 = 3, HasOrder = 4, FinishedBuy = 5, Reject = 6, StopApply = 7 };
public enum ApplyConfirmLevel { TeamLeader = 1, Manager1 = 2, Manager2 = 3 };
public enum ApplyConfirmType { Self = 0, Vicar = 1 };
public enum ApplyAgreeType { Agree = 0, DiaAgree = 1 };
public enum DeviceStatus { Apply = 0, ApplyFinished = 1, StartBuy = 2, BuyFinished = 3, StopBuy = 4 };
public enum FormBuyType { self = 0, zicai = 1, net = 2 };


public class DeviceApply
{
    public static SqlConnection conn = Moldau.Config.WebConfig.GetConn("QT2");

    public static string[] FormStatusString = new string[] { "創建", "直屬主管處理完畢", "一階主管處理完畢", "申請處理完畢", "已經下單", "完成購買", "退件", "表單作廢" };
    public static string[] DeviceStatusString = new string[] { "申請中", "申請完成", "購買中", "購買完成", "停止購買" };
	
    public DeviceApply()
	{

	}

    //Check Server Right

    public static void CheckRight()
    {
        if ((int)HttpContext.Current.Session["UserFlag"] < 1) HttpContext.Current.Response.Redirect("~/RightError.aspx?ErrorID=1");
    }

    public static string LeaderInput(String UserName)
    {
        if (QT2UserInformation.IsLeader(UserName))
            return "<input type=\"hidden\" id=\"LeaderInput\" value=\"1\" />";
        else
            return "<input type=\"hidden\" id=\"LeaderInput\" value=\"0\" />";
    }

    public static string ConfirmLevelInput(ApplyConfirmLevel ConfirmLevel)
    {
        return "<input type=\"hidden\" id=\"ConfirmLevel\"  name=\"ConfirmLevel\"  value=\"" + Convert.ToString((int)ConfirmLevel) +"\" />";
    }


    public static string InitFormNO()
    {


        string FormNO = "";
        string FormNO_4F = DateTime.Today.ToString("yyMM");
        string FormNO_2E = "";

        SqlCommand SeleComm = new SqlCommand("Select top 1 FormNO from DRForm order by ID DESC", conn);

        if (conn.State == ConnectionState.Open)
            conn.Close();
        conn.Open();
        SqlDataReader TmpSqlDataReader = SeleComm.ExecuteReader();
        if (TmpSqlDataReader.HasRows)
        {
            TmpSqlDataReader.Read();
            string OrignalMaxFormNO = TmpSqlDataReader.GetString(0);
            if (FormNO_4F == OrignalMaxFormNO.Substring(0, 4))
            {
                FormNO_2E = Convert.ToString(Convert.ToInt32(OrignalMaxFormNO.Substring(4, 2)) + 1);
                if (FormNO_2E.Length == 1)
                {
                    FormNO_2E = "0" + FormNO_2E;
                }
                FormNO = FormNO_4F + FormNO_2E;
            }
            else
            {
                FormNO = FormNO_4F + "01";
            }

        }
        else
        {
            FormNO = FormNO_4F + "01";
        }
        conn.Close();
        return FormNO;
    }


    public static void SendNoteMail(string FromUserName, string ToUserNameString, string CCUserNameString, string Subject, string MailBody)
    {

        string MailTo, MailFrom, MailCC;

        //MailTo
        MailTo = "";
        string[] ToUserNameArray = ToUserNameString.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < ToUserNameArray.Length; i++)
        {
            if (QT2UserInformation.GetMyEmail(ToUserNameArray[i]) == null)
                continue;
            if (MailTo == "")
                MailTo = QT2UserInformation.GetMyEmail(ToUserNameArray[i]);
            else
                MailTo += "," + QT2UserInformation.GetMyEmail(ToUserNameArray[i]);

        }


        MailCC = "";
        string[] CCUserNameArray = CCUserNameString.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < CCUserNameArray.Length; i++)
        {
            if (QT2UserInformation.GetMyEmail(CCUserNameArray[i]) == null)
                continue;
            if (MailCC == "")
                MailCC = QT2UserInformation.GetMyEmail(CCUserNameArray[i]);
            else
                MailCC += "," + QT2UserInformation.GetMyEmail(CCUserNameArray[i]);

        }

        //MailFrom;
        if (QT2UserInformation.GetMyEmail(FromUserName) != null)
        {
            MailFrom = QT2UserInformation.GetMyEmail(FromUserName);
        }
        else
        {
            MailFrom = new QT2User(WebConfig.SendMailUser).Mail;
        }
        SmtpMail.MailSend(MailFrom, MailTo, MailCC, Subject, MailBody);

    }

    //Get SendMailUser
    public static string GetSendMailUser()
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(System.Web.HttpContext.Current.Server.MapPath("~/DeviceRequest/XML/SystemConfig.XML"));
        return XmlDoc.SelectSingleNode("/Config/SendMailUser/UserName").InnerText;
        
    }

    //Get Buy User;
    public static string GetBuyUser(int BuyType)
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(System.Web.HttpContext.Current.Server.MapPath("~/DeviceRequest/XML/SystemConfig.XML"));
        return XmlDoc.SelectSingleNode("/Config/BuyTypeSendUser/Type" + BuyType.ToString() + "/UserName").InnerText;

    }




    

    public static void WriteDeviceFormHeader(string TableName)
    {
        HttpContext.Current.Response.Write("<table class=\"ListDeviceTable\" id=\"" + TableName + "\"  cellpadding=\"2\" cellspacing=\"0\">");

    }

    //Check Admin

    public static bool ChkAdmin()
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(System.Web.HttpContext.Current.Server.MapPath("~/DeviceRequest/XML/SystemConfig.XML"));
        XmlNodeList AdminNodeList = XmlDoc.SelectNodes("/Config/Admin/UserName");
        for (int i = 0; i < AdminNodeList.Count; i++)
        {
            if (AdminNodeList[i].InnerText == HttpContext.Current.Session["UserName"].ToString())
            {
                return true;
            }
        }
        return false;
    }


    //Chk BuyUser
    public static bool ChkBuyUser(int BuyType)
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(System.Web.HttpContext.Current.Server.MapPath("~/DeviceRequest/XML/SystemConfig.XML"));
        XmlNodeList BuyUserNodes = XmlDoc.SelectNodes("/Config/BuyTypeSendUser/Type" + BuyType.ToString() + "/UserName");

        for (int i = 0; i < BuyUserNodes.Count; i++)
        {
            if (BuyUserNodes[i].InnerText == HttpContext.Current.Session["UserName"].ToString())
            {
                return true;
            }
            

        }
        return false;

    }


    //Write Device Form

    public static void WriteDeviceForm(System.Collections.Generic.List<int> FormIDList)
    {
        CloseConn();
        WriteDeviceFormHeader("ListFormTable");
        for (int k = 0; k < FormIDList.Count; k++)
        {



            DataTable DRForm = new System.Data.DataTable();
            DataTable DRDevice = new System.Data.DataTable();
            DataTable DRBuy = new System.Data.DataTable();


            SqlDataAdapter TmpSqlDataAdapter = new SqlDataAdapter();
            SqlCommand SelecComm = new SqlCommand();
            //獲取表單
            conn.Open();
            SelecComm = new SqlCommand("select * from DRForm where ID = " + FormIDList[k].ToString(), conn);
            TmpSqlDataAdapter.SelectCommand = SelecComm;
            TmpSqlDataAdapter.Fill(DRForm);
            conn.Close();

            //獲取Device
            if (DRForm.Rows.Count == 1)
            {
                SelecComm = new SqlCommand("select  @TotalNumber = sum(Number), @TotalPrice = sum(Price * Number )  from DRDevice where FormID = " + FormIDList[k].ToString(), conn);
                SelecComm.Parameters.Add(new SqlParameter("@TotalNumber", SqlDbType.Int));
                SelecComm.Parameters.Add(new SqlParameter("@TotalPrice", SqlDbType.Int));

                SelecComm.Parameters["@TotalNumber"].Direction = ParameterDirection.Output;
                SelecComm.Parameters["@TotalPrice"].Direction = ParameterDirection.Output;
                conn.Open();
                SelecComm.ExecuteNonQuery();
                conn.Close();

                //Write 表單頭
                HttpContext.Current.Response.Write("<tbody><tr style=\"color:White; background:#3399cc; text-align:left;\">" +
                                                   "<td colspan=\"12\"><input id=\"FormID" + FormIDList[k].ToString() + "\"   type=\"checkbox\" name=\"FormID\" />" +
                                                   "<a href=\"FormDetails.aspx?FormID=" + FormIDList[k].ToString() + "\"><strong><span style=\"color: #fffacd; margin-left:40px;\">單號: </span></strong>" + DRForm.Rows[0]["FormNO"].ToString() + "</a>" +
                                                   "<strong><span style=\"color: #fffacd;  margin-left:40px;\">設備總數量:  </span></strong>" + SelecComm.Parameters["@TotalNumber"].Value +
                                                   "<strong><span style=\"color: #fffacd ;  margin-left:40px;\">預估縂金額(元):  </span></strong>" + SelecComm.Parameters["@TotalPrice"].Value +
                                                   "<strong><span style=\"color: #fffacd ; margin-left:40px;\">表單狀態:  </span></strong>" + FormStatusString[Convert.ToInt32(DRForm.Rows[0]["status"])]);

                if (ChkAdmin())
                {
                    HttpContext.Current.Response.Write("<a href=\"SuperManageForm.aspx?FormID=" + FormIDList[k].ToString() + "\"><strong><span style=\"color: #fffacd; margin-left:40px;\">FULL-Y</span></strong></a>");
                }

                HttpContext.Current.Response.Write("</td></tr></tbody>");

                conn.Open();
                SelecComm = new SqlCommand("select * from DRDevice where FormID = " + DRForm.Rows[0]["ID"].ToString(), conn);
                TmpSqlDataAdapter.SelectCommand = SelecComm;
                TmpSqlDataAdapter.Fill(DRDevice);
                conn.Close();
                if (DRDevice.Rows.Count > 0)
                {
                    HttpContext.Current.Response.Write("<tbody><tr style=\"background:#FFFFFF\"><td style=\"width:5%\">" +
                                                        "編號</td>" +
                                                        "<td  style=\"width:10%\">" +
                                                        "設備廠商</td>" +
                                                        "<td  style=\"width:10%\">" +
                                                        "設備名稱</td>" +
                                                        "<td style=\"width:5%\">" +
                                                        "數量</td>" +
                                                        "<td style=\"width:10%\">" +
                                                        "預估單價(元)</td>" +
                                                        "<td style=\"width:5%\">" +
                                                        "設備類型</td>" +
                                                        "<td style=\"width:10%\">" +
                                                        "請購原因</td>" +
                                                        "<td style=\"width:10%\">" +
                                                        "請購備註</td>" +
                                                        "<td>" +
                                                        "購買情況</td>" +
                                                        "<td style=\"width:5%\">" +
                                                        "設備狀態</td>" +
                                                        "<td style=\"width:5%\">" +
                                                        "備註</td>");
                    if (ChkAdmin() || ChkBuyUser(Convert.ToInt32(DRForm.Rows[0]["BuyType"])))
                    {
                        HttpContext.Current.Response.Write("<td style=\"width:4%\">管理</td>");
                    }
                    else
                    {
                        HttpContext.Current.Response.Write("<td style=\"width:4%\">詳情</td>");
                    }

                    HttpContext.Current.Response.Write("</tr></tbody>");
                   

                    //Write Device
                    for (int j = 0; j < DRDevice.Rows.Count; j++)
                    {

                        //Write 編號

                        HttpContext.Current.Response.Write("<tbody><tr style=\"background:#FEFEEE\">");
                        HttpContext.Current.Response.Write("<td>");
                        HttpContext.Current.Response.Write(j + 1);
                        HttpContext.Current.Response.Write("</td>");

                        //設備廠商

                        HttpContext.Current.Response.Write("<td>");
                        HttpContext.Current.Response.Write(DRDevice.Rows[j]["Vender"]);
                        HttpContext.Current.Response.Write("</td>");

                        //設備名稱
                        HttpContext.Current.Response.Write("<td>");
                        HttpContext.Current.Response.Write(DRDevice.Rows[j]["Device"]);
                        HttpContext.Current.Response.Write("</td>");

                        //數量
                        HttpContext.Current.Response.Write("<td>");
                        HttpContext.Current.Response.Write(DRDevice.Rows[j]["Number"]);
                        HttpContext.Current.Response.Write("</td>");

                        //預估單價
                        HttpContext.Current.Response.Write("<td>");
                        HttpContext.Current.Response.Write(DRDevice.Rows[j]["Price"]);
                        HttpContext.Current.Response.Write("</td>");

                        //設備類型
                        HttpContext.Current.Response.Write("<td>");
                        HttpContext.Current.Response.Write(DRDevice.Rows[j]["DeviceType"]);
                        HttpContext.Current.Response.Write("</td>");

                        //請購原因
                        HttpContext.Current.Response.Write("<td>");
                        HttpContext.Current.Response.Write(DRDevice.Rows[j]["Reason"]);
                        HttpContext.Current.Response.Write("</td>");


                        //請購備註
                        HttpContext.Current.Response.Write("<td>");
                        HttpContext.Current.Response.Write(DRDevice.Rows[j]["Remark"]);
                        HttpContext.Current.Response.Write("</td>");

                        //購買情況
                        HttpContext.Current.Response.Write("<td>");
                        WriteDeviceBuyDetails(Convert.ToInt32(DRDevice.Rows[j]["ID"]));
                        HttpContext.Current.Response.Write("</td>");

                        //請購狀態
                        HttpContext.Current.Response.Write("<td>");
                        HttpContext.Current.Response.Write(DeviceStatusString[Convert.ToInt32(DRDevice.Rows[j]["Status"])]);
                        HttpContext.Current.Response.Write("</td>");

                        //購買備註
                        HttpContext.Current.Response.Write("<td>");
                        HttpContext.Current.Response.Write(DRDevice.Rows[j]["BuyRemark"]);
                        HttpContext.Current.Response.Write("</td>");



                        HttpContext.Current.Response.Write("<td>");
                        if (ChkAdmin() || ChkBuyUser(Convert.ToInt32(DRForm.Rows[0]["BuyType"])))
                            HttpContext.Current.Response.Write("<a href=\"javascript:void(0)\" onclick=\"OpenNewWindow(1," + DRDevice.Rows[j]["ID"].ToString() + ")\"  >管理</a>");
                        else
                            HttpContext.Current.Response.Write("<a href=\"javascript:void(0)\" onclick=\"OpenNewWindow(0," + DRDevice.Rows[j]["ID"].ToString() + ")\"  >详情</a>");

                        HttpContext.Current.Response.Write("</td>");

                        HttpContext.Current.Response.Write("</tr></tbody>");


                    }




                }

            }
        }

        HttpContext.Current.Response.Write("</table>");

    }


    //Write DeviceDetails
    public static void WriteDeviceDetails(System.Collections.Generic.List<int> DeviceIDList)
    {
        WriteDeviceFormHeader("ListDeviceTable");
        HttpContext.Current.Response.Write("<tbody><tr style=\"background:#cccccc; font-weight:bold;color:#FFF\"><td style=\"width:5%\">" +
                                                    "數據庫編號</td>" +
                                                    "<td  style=\"width:10%\">" +
                                                    "設備廠商</td>" +
                                                    "<td  style=\"width:10%\">" +
                                                    "設備名稱</td>" +
                                                    "<td style=\"width:5%\">" +
                                                    "數量</td>" +
                                                    "<td style=\"width:10%\">" +
                                                    "預估單價(元)</td>" +
                                                    "<td style=\"width:5%\">" +
                                                    "設備類型</td>" +
                                                    "<td style=\"width:10%\">" +
                                                    "請購原因</td>" +
                                                    "<td style=\"width:10%\">" +
                                                    "請購備註</td>" +
                                                    "<td>" +
                                                    "購買情況</td>" +
                                                    "<td style=\"width:5%\">" +
                                                    "設備狀態</td>" +
                                                    "<td style=\"width:5%\">" +
                                                    "備註</td>");
        
            HttpContext.Current.Response.Write("<td style=\"width:4%\">管理</td>");
            HttpContext.Current.Response.Write("</tr></tbody>");




        for (int i = 0; i < DeviceIDList.Count; i++)
        {
            SqlCommand SeleComm = new SqlCommand("Select * from DRDevice where ID = " + DeviceIDList[i].ToString(), conn);
            conn.Open();
            SqlDataReader TmpSqlDataReader = SeleComm.ExecuteReader();
            if (TmpSqlDataReader.HasRows)
            {
                TmpSqlDataReader.Read();
                if (i % 2 == 0)
                    HttpContext.Current.Response.Write("<tbody><tr style=\"background:#FEFEEE\">");
                else
                    HttpContext.Current.Response.Write("<tr style=\"background:#FFFFFF\">");
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write("<input id=\"DeviceID" + DeviceIDList[i].ToString() + "\"   type=\"checkbox\" name=\"DeviceID\" />");
                HttpContext.Current.Response.Write(TmpSqlDataReader["ID"]);
                HttpContext.Current.Response.Write("</td>");

                //設備廠商

                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(TmpSqlDataReader["Vender"]);
                HttpContext.Current.Response.Write("</td>");

                //設備名稱
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(TmpSqlDataReader["Device"]);
                HttpContext.Current.Response.Write("</td>");

                //數量
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(TmpSqlDataReader["Number"]);
                HttpContext.Current.Response.Write("</td>");

                //預估單價
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(TmpSqlDataReader["Price"]);
                HttpContext.Current.Response.Write("</td>");

                //設備類型
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(TmpSqlDataReader["DeviceType"]);
                HttpContext.Current.Response.Write("</td>");

                //請購原因
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(TmpSqlDataReader["Reason"]);
                HttpContext.Current.Response.Write("</td>");


                //請購備註
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(TmpSqlDataReader["Remark"]);
                HttpContext.Current.Response.Write("</td>");

                //購買情況
                HttpContext.Current.Response.Write("<td>");
                WriteDeviceBuyDetails(Convert.ToInt32(TmpSqlDataReader["ID"]));
                HttpContext.Current.Response.Write("</td>");

                //請購狀態
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(DeviceStatusString[Convert.ToInt32(TmpSqlDataReader["Status"])]);
                HttpContext.Current.Response.Write("</td>");

                //購買備註
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(TmpSqlDataReader["BuyRemark"]);
                HttpContext.Current.Response.Write("</td>");



                HttpContext.Current.Response.Write("<td>");
                if (ChkAdmin() || ChkBuyUser(GetFormBuyType(Convert.ToInt32(TmpSqlDataReader["FormID"]))))
                    HttpContext.Current.Response.Write("<a href=\"javascript:void(0)\" onclick=\"OpenNewWindow(1," + TmpSqlDataReader["ID"].ToString() + ")\"  >管理</a>");
                else
                    HttpContext.Current.Response.Write("<a href=\"javascript:void(0)\" onclick=\"OpenNewWindow(0," + TmpSqlDataReader["ID"].ToString() + ")\"  >详情</a>");

                HttpContext.Current.Response.Write("</td>");


                HttpContext.Current.Response.Write("</tr></tbody>");


                conn.Close();

            }

           
            //Write 編號

            
        }



        HttpContext.Current.Response.Write("</table>");



    }

    //Write 購買情況
    public static void WriteDeviceBuyDetails(int DeviceID)
    {
        if (DeviceID != null)
        {

            DataTable DRBuy = new DataTable();
            SqlConnection conn1 = Moldau.Config.WebConfig.GetConn("QT2");
            conn1.Open();
            SqlCommand SelecComm = new SqlCommand("select * from DRBuy where DeviceID = " + DeviceID.ToString(), conn1);
            SqlDataAdapter TmpSqlDataAdapter = new SqlDataAdapter();
            TmpSqlDataAdapter.SelectCommand = SelecComm;
            TmpSqlDataAdapter.Fill(DRBuy);
            conn1.Close();

            for (int k = 0; k < DRBuy.Rows.Count; k++)
            {
                HttpContext.Current.Response.Write(Convert.ToString(k + 1) + "--時間:" + ((DateTime)DRBuy.Rows[k]["BuyDate"]).ToShortDateString() + " 數量: " + DRBuy.Rows[k]["Number"].ToString() + " 單價(元):" + DRBuy.Rows[k]["Price"].ToString() + " 備註: " + DRBuy.Rows[k]["BuyRemark"].ToString() + "<br/>");
            }
        }
        
    }


    public static string ConvertIDToFormNO(int FormID)
    {
        if (FormID != null)
        {
            SqlCommand SeleComm = new SqlCommand("select @FormNO = FormNO   from DRForm where ID =" + FormID.ToString(), conn);
            SeleComm.Parameters.Add(new SqlParameter("@FormNO", SqlDbType.NVarChar, 6));
            SeleComm.Parameters["@FormNO"].Direction = ParameterDirection.Output;
         
            conn.Open();
            
            SeleComm.ExecuteNonQuery();
            conn.Close();
            return SeleComm.Parameters["@FormNO"].Value.ToString();
        }
        else
            return null;
    }

    public static int GetDeviceStatus(int DeviceID)
    {
        SqlCommand SeleComm = new SqlCommand("Select  @Status = status  from DRDevice where ID = " + DeviceID.ToString(), conn);
        SeleComm.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int));
        SeleComm.Parameters["@Status"].Direction = ParameterDirection.Output;
        conn.Open();
        SeleComm.ExecuteNonQuery();
        conn.Close();
        return Convert.ToInt32(SeleComm.Parameters["@Status"].Value);

    }

    public static int GetFormBuyType(int FormID)
    {
        SqlConnection conn1 = Moldau.Config.WebConfig.GetConn("QT2");
        SqlCommand SeleComm = new SqlCommand("Select  @BuyType = BuyType  from DRForm where ID = " + FormID.ToString(), conn1);
        SeleComm.Parameters.Add(new SqlParameter("@BuyType", SqlDbType.Int));
        SeleComm.Parameters["@BuyType"].Direction = ParameterDirection.Output;
        conn1.Open();
        SeleComm.ExecuteNonQuery();
        conn1.Close();
        return Convert.ToInt32(SeleComm.Parameters["@BuyType"].Value);
    }

    public static int GetFormStatus(int FormID)
    {
        SqlCommand SeleComm = new SqlCommand("Select  @Status = status  from DRForm where ID = " + FormID.ToString(), conn);
        SeleComm.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int));
        SeleComm.Parameters["@Status"].Direction = ParameterDirection.Output;
        conn.Open();
        SeleComm.ExecuteNonQuery();
        conn.Close();
        return Convert.ToInt32(SeleComm.Parameters["@Status"].Value);
    }

    public static void WriteFormDealWithStatus(int FormID)
    {
        DataTable DRConfirm = new DataTable();
        SqlCommand SeleComm = new SqlCommand("Select * from DRConfirm where FormID = " + FormID + "Order by ConfirmLevel", conn);
        conn.Open();
        SqlDataAdapter TmpSqlDataAdapter = new SqlDataAdapter();
        TmpSqlDataAdapter.SelectCommand = SeleComm;
        TmpSqlDataAdapter.Fill(DRConfirm);
        conn.Close();

        if (DRConfirm.Rows.Count > 0)
        {
            HttpContext.Current.Response.Write("<table id=\"DealWithTable\" style=\" width:100%; margin-top:20px; text-align:left;\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\">" +
                                                "<tr style=\"background:#c0c0c0\"><td style=\" width:10%;\">處理人</td><td style=\" width:20%;\">時間</td><td>意見</td></tr>");

            for (int i = 0; i < DRConfirm.Rows.Count; i++)
            {
                //User
                HttpContext.Current.Response.Write("<tr>");
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(DRConfirm.Rows[i]["ConfirmUser"]);
                if (Convert.ToInt32(DRConfirm.Rows[i]["ConfirmType"]) == (int)ApplyConfirmType.Vicar)
                {
                    HttpContext.Current.Response.Write("(代理)");
                }
                HttpContext.Current.Response.Write("</td>");
                //時間
                HttpContext.Current.Response.Write("<td>");
                HttpContext.Current.Response.Write(Convert.ToDateTime(DRConfirm.Rows[i]["ConfirmDate"]).ToString("yyyy/MM/dd--HH:mm"));
                HttpContext.Current.Response.Write("</td>");
                //意見
                HttpContext.Current.Response.Write("<td>");
                if (Convert.ToInt32(DRConfirm.Rows[i]["Agree"]) == (int)ApplyAgreeType.Agree)
                {
                    HttpContext.Current.Response.Write("同意---");
                }
                else
                {
                    HttpContext.Current.Response.Write("退件---");
                }
                HttpContext.Current.Response.Write(DRConfirm.Rows[i]["Remark"]);
                HttpContext.Current.Response.Write("</td>");

                HttpContext.Current.Response.Write("</tr>");
            }

            HttpContext.Current.Response.Write("</table>");

        }
    }


    public static void WritFormOrderStatus(int FormID)
    {
        DataTable DROrder = new DataTable();
        SqlCommand SeleComm = new SqlCommand("Select * from DROrder where FormID = " + FormID, conn);
        conn.Open();
        SqlDataAdapter TmpSqlDataAdapter = new SqlDataAdapter();
        TmpSqlDataAdapter.SelectCommand = SeleComm;
        TmpSqlDataAdapter.Fill(DROrder);
        conn.Close();
        if (DROrder.Rows.Count > 0)
        {

            HttpContext.Current.Response.Write("<table id=\"OrderTable\" style=\" width:100%; margin-top:20px; text-align:left;\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\">" +
                            "<tr style=\"background:#c0c0c0\"><td style=\" width:10%;\">" +
                            "下單人</td><td style=\" width:20%;\">時間</td><td>" +
                            "備註</td></tr>");



            //User
            HttpContext.Current.Response.Write("<tr>");
            HttpContext.Current.Response.Write("<td>");
            HttpContext.Current.Response.Write(DROrder.Rows[0]["UserName"]);
            HttpContext.Current.Response.Write("</td>");
            //時間
            HttpContext.Current.Response.Write("<td>");
            HttpContext.Current.Response.Write(Convert.ToDateTime(DROrder.Rows[0]["OrderDate"]).ToString("yyyy/MM/dd--HH:mm"));
            HttpContext.Current.Response.Write("</td>");
            //意見
            HttpContext.Current.Response.Write("<td>");
            HttpContext.Current.Response.Write(DROrder.Rows[0]["Remark"]);
            HttpContext.Current.Response.Write("</td>");

            HttpContext.Current.Response.Write("</tr></table> ");
        }
    }

    public static void WriteFormCancel(int FormID)
    {
        if(GetFormStatus(FormID) == (int)ApplyFormStatus.StopApply)
        {
            SqlCommand TmpSqlComm = new SqlCommand("select * from DRCancel where FormID = " + FormID.ToString(), conn);
            SqlDataReader TmpSqlDataReader;
            conn.Open();
            TmpSqlDataReader = TmpSqlComm.ExecuteReader();
            if (TmpSqlDataReader.HasRows)
            {
                TmpSqlDataReader.Read();
                HttpContext.Current.Response.Write("<table id=\"CancelTable\" style=\" width:100%; margin-top:20px; text-align:left;\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\">" +
                            "<tr style=\"background:#c0c0c0\"><td style=\" width:10%;\">" +
                            "取消人</td><td>理由</td></tr>");
                HttpContext.Current.Response.Write("<tr><td>" + TmpSqlDataReader["CancelUser"].ToString() + "</td><td>" + TmpSqlDataReader["Remark"].ToString() + "</td></tr></table>");
            }

            conn.Close();
            
        }
    }

    public static void WriteFormStatus(int FormID)
    {
        HttpContext.Current.Response.Write("<table id=\"FormStatusTable\"  style=\" width:100%; margin-top:20px; text-align:left;\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\">" +
                                        "<tr style=\"background:#c0c0c0\"><td style=\" width:10%; height: 10px;\">" +
                                        "表單狀態:</td><td style=\"height: 10px; text-align: left;\" colspan=\"2\">" + DeviceApply.FormStatusString[GetFormStatus(FormID)] + "</td></tr>" +
                                        "</table> ");
    }


    public static void WriteFormStatusSelect()
    {
        HttpContext.Current.Response.Write("<select id=\"FormStatus\" name=\"FormStatus\">");
        
        ApplyFormStatus[] TotalApplyFormStatus = (ApplyFormStatus[])Enum.GetValues(typeof(ApplyFormStatus));
        HttpContext.Current.Response.Write("<option value=\"\">請選擇</option>");
        foreach(ApplyFormStatus EachStatus in TotalApplyFormStatus)
        {
            HttpContext.Current.Response.Write("<option value=\"" + Convert.ToString((int)EachStatus) + "\">" + FormStatusString[(int)EachStatus] + "</option>");
        }

        HttpContext.Current.Response.Write("</select>");
  
    }


    public static void WriteDeviceStatusSelect()
    {
        HttpContext.Current.Response.Write("<select id=\"DeviceStatus\" name=\"DeviceStatus\">");

        DeviceStatus[] TotalDeviceStatus = (DeviceStatus[])Enum.GetValues(typeof(DeviceStatus));
        HttpContext.Current.Response.Write("<option value=\"\">請選擇</option>");
        foreach ( DeviceStatus  EachStatus  in TotalDeviceStatus)
        {
            HttpContext.Current.Response.Write("<option value=\"" + Convert.ToString((int)EachStatus) + "\">" + DeviceStatusString[(int)EachStatus] + "</option>");
        }

        HttpContext.Current.Response.Write("</select>");

    }

    public static int GetFormIDFromDeviceID(int DeviceID)
    {
        SqlCommand SeleComm = new SqlCommand("select @FormID = FormID from DRDevice where ID = " + DeviceID.ToString() ,conn);
        SeleComm.Parameters.Add(new SqlParameter("@FormID", SqlDbType.Int));
        SeleComm.Parameters["@FormID"].Direction = ParameterDirection.Output;
        conn.Open();
        SeleComm.ExecuteNonQuery();
        conn.Close();
        return Convert.ToInt32(SeleComm.Parameters["@FormID"].Value);

    }



    public static string GetItemFromDRForm(int FormID , string ColumnName)
    {
        if (FormID != null)
        {
            SqlCommand SeleComm = new SqlCommand("select " +  ColumnName + "  from DRForm where ID =" + FormID.ToString(), conn);
            SqlDataReader TmpSqlDataReader;
            string ReturnString = "";
            conn.Open();
            TmpSqlDataReader = SeleComm.ExecuteReader();
            if(TmpSqlDataReader.Read())
            {
                ReturnString = TmpSqlDataReader[ColumnName].ToString();
            }
            conn.Close();

            if (ReturnString == "")
                ReturnString = null;

            return ReturnString;
        }
        else
            return null;
    }


    public static string GetItemFromDRDevice(int DeviceID, string ColumnName)
    {

        if (DeviceID != null)
        {
            SqlCommand SeleComm = new SqlCommand("select " + ColumnName + " from DRDevice where ID = " + DeviceID.ToString(), conn);
            SqlDataReader TmpSqlDataReader;
            string ReturnString = "";
            conn.Open();
            TmpSqlDataReader = SeleComm.ExecuteReader();
            if (TmpSqlDataReader.Read())
            {
                ReturnString = TmpSqlDataReader[ColumnName].ToString();
            }
            conn.Close();

            if (ReturnString == "")
                ReturnString = null;

            return ReturnString;
        }
        else
            return null;
    }

    public static int GetMaxPrice()
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(System.Web.HttpContext.Current.Server.MapPath("~/DeviceRequest/XML/SystemConfig.XML"));
        return Convert.ToInt32(XmlDoc.SelectSingleNode("/Config/MaxPrice").InnerText);
    }

    public static void CloseConn()
    {
        if (conn.State == ConnectionState.Open)
        {
            conn.Close();
        }
    }

    public static string GetManager1(string TeamName)
    {
        SqlCommand SeleComm = new SqlCommand("select @TeamManager2 = TeamManager2 from Team where TeamName = '" + TeamName + "'" , conn);
        SeleComm.Parameters.Add(new SqlParameter("@TeamManager2", SqlDbType.NVarChar,50));
        SeleComm.Parameters["@TeamManager2"].Direction = ParameterDirection.Output;
        conn.Open();
        SeleComm.ExecuteNonQuery();
        conn.Close();
        return SeleComm.Parameters["@TeamManager2"].Value.ToString();
    }

}
