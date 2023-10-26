using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// MP 的摘要描述
/// </summary>
public class MP
{
    public static SqlConnection conn = Moldau.Config.WebConfig.GetConn("QT2");
	public MP()
	{

	}

    
    public static void CloseConn()
    {
        if (conn.State == ConnectionState.Open)
        {
            conn.Close();
        }

    }

    public static string GetMasterPage(string FileName)
    {
        string ReturnString = "";
        ReturnString = File.ReadAllText(FileName, System.Text.Encoding.Default);
        return ReturnString;
    }

    public static string GetPageJs(string PageFileName)
    {
        string ReturnString = "";
        string TmpString = File.ReadAllText(HttpContext.Current.Server.MapPath("~/MP/JS/Function.js"), System.Text.Encoding.Default);
        TmpString = TmpString.Substring(TmpString.IndexOf("//" + PageFileName) + PageFileName.Length + 2);
        ReturnString = TmpString.Substring(0, TmpString.IndexOf("//" + PageFileName));
        return ReturnString;
    }

    public static string GetStructureUrl(int ID)
    {
        return HttpContext.Current.Server.MapPath("~/MP/XML/Structure" + ID.ToString() + ".xml");
    }

    public static int GetActiveStructureID()
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(HttpContext.Current.Server.MapPath("~/MP/Xml/Config.xml"));
        return Convert.ToInt32(XmlDoc.SelectSingleNode("//ActiveStructure").Attributes.GetNamedItem("ID").Value);
    }

    public static string GetMP_NO(DateTime CurrentDate)
    {
        return CurrentDate.ToString("yyyyMM");
    }

    public static string GetScoreNoString(string ScoreNo)
    {
        return ScoreNo.Substring(0, 4) + "年" + ScoreNo.Substring(4, 2) + "月";
    }

    public static int CheckUserRight(string UserName)
    {
        MP_Strcuture Structure = new MP_Strcuture(MP.GetStructureUrl(MP.GetActiveStructureID()));
        return Structure.GetUserLevelInt(UserName);
    }

    
}




//MP Score Item

public class MP_Score
{
    private string UserName;
    private int TotalScore;
    private string ScoreString;
    private string RemarkString;
    private string ScoreNo;
    private DateTime StartDate;
    private DateTime EndDate;
    private int StructureID;
    private int Status;
    private string TeamName;
    private bool Flag = false;
    private int TemplateID;
    private int ID;


    //屬性




    //Status 0 剛創建 , Duty Level 1 , Duty Level 2, 
    
    public MP_Score(string UserName, int TotalScore, string ScoreString , string RemarkString , string ScoreNo , DateTime StartDate , DateTime EndDate , int StructureID , int Status, string TeamName, int TemplateID)
    {
        this.UserName = UserName;
        this.TotalScore = TotalScore;
        this.ScoreString = ScoreString;
        this.RemarkString = RemarkString;
        this.ScoreNo = ScoreNo;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        this.StructureID = StructureID;
        this.Status = Status;
        this.TeamName = TeamName;
        this.TemplateID = TemplateID;

    }



    public MP_Score(int ID)
    {
        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("select * from MS_Score where ID = " + ID, conn);
        SqlDataReader TmpSqlDataReader;


        this.Flag = false;

        conn.Open();
        TmpSqlDataReader = comm.ExecuteReader();
        while (TmpSqlDataReader.Read())
        {
            this.UserName = TmpSqlDataReader["UserName"].ToString();
            this.TotalScore = Utils.StrToInt(TmpSqlDataReader["TotalScore"], 0);

            this.ScoreString = TmpSqlDataReader["ScoreString"].ToString();

            this.RemarkString = TmpSqlDataReader["RemarkString"].ToString();

            this.ScoreNo = TmpSqlDataReader["ScoreNo"].ToString();
            this.StartDate = Convert.ToDateTime(TmpSqlDataReader["StartDate"]);
            this.EndDate = Convert.ToDateTime(TmpSqlDataReader["EndDate"]);
            this.StructureID = Utils.StrToInt(TmpSqlDataReader["StructureID"], 1);
            this.Status = Utils.StrToInt(TmpSqlDataReader["Status"], -1);
            this.TeamName = TmpSqlDataReader["TeamName"].ToString();
            this.TemplateID = Utils.StrToInt(TmpSqlDataReader["TemplateID"], 1);
            this.ID = Utils.StrToInt(TmpSqlDataReader["ID"], 1);
            this.Flag = true;
            



        }


        conn.Close();



    }

    public MP_Score(string ScoreNo , string UserName)
    {
        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("select * from MS_Score where ScoreNo = '" + ScoreNo + "' and UserName = '" + UserName + "'", conn);
        SqlDataReader TmpSqlDataReader;

        conn.Open();
        TmpSqlDataReader = comm.ExecuteReader();
        if (TmpSqlDataReader.HasRows)
        {
            TmpSqlDataReader.Read();
            this.UserName = TmpSqlDataReader["UserName"].ToString();
            this.TotalScore = Utils.StrToInt(TmpSqlDataReader["TotalScore"], 0);
            this.ScoreString = TmpSqlDataReader["ScoreString"].ToString();
            this.RemarkString = TmpSqlDataReader["RemarkString"].ToString();
            this.ScoreNo = TmpSqlDataReader["ScoreNo"].ToString();
            this.StartDate = Convert.ToDateTime(TmpSqlDataReader["StartDate"]);
            this.EndDate = Convert.ToDateTime(TmpSqlDataReader["EndDate"]);
            this.StructureID = Utils.StrToInt(TmpSqlDataReader["StructureID"], 1);
            this.Status = Utils.StrToInt(TmpSqlDataReader["Status"], -1);
            this.TeamName = TmpSqlDataReader["TeamName"].ToString();
            this.TemplateID = Utils.StrToInt(TmpSqlDataReader["TemplateID"], 1);
            this.ID = Utils.StrToInt(TmpSqlDataReader["ID"], 1);
            this.Flag = true;

        }
        else
        {
            this.Flag = false;
        }
        conn.Close();
    }

    //Return -1 Delegate Exist Record     , ID  Delegate Success , 0 Delegate Fail
    public int InsertScore()
    {
        //存在相同的記錄
        if (new MP_Score(this.ScoreNo, this.UserName).Flag)
        {
            return -1;
        }

        
        SqlConnection conn = MP.conn;
        SqlCommand comm = new SqlCommand("insert into MS_Score(ScoreNo,UserName,TotalScore,ScoreString,RemarkString,StructureID,StartDate,EndDate,Status,TeamName,TemplateID) values(@ScoreNo,@UserName,@TotalScore,@ScoreString,@RemarkString,@StructureID,@StartDate,@EndDate,@Status,@TeamName,@TemplateID) ; select @InsertedID = @@Identity", conn);
        comm.Parameters.Add(new SqlParameter("@ScoreNo", SqlDbType.VarChar, 6));
        comm.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 50));
        comm.Parameters.Add(new SqlParameter("@TotalScore", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@ScoreString", SqlDbType.NVarChar, 1000));
        comm.Parameters.Add(new SqlParameter("@RemarkString", SqlDbType.NVarChar, 2000));
        comm.Parameters.Add(new SqlParameter("@StructureID", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.SmallDateTime));
        comm.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.SmallDateTime));
        comm.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@InsertedID", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@TeamName", SqlDbType.NVarChar, 50));
        comm.Parameters.Add(new SqlParameter("@TemplateID", SqlDbType.Int));

        comm.Parameters["@ScoreNo"].Value = this.ScoreNo;
        comm.Parameters["@UserName"].Value = this.UserName;
        comm.Parameters["@TotalScore"].Value = this.TotalScore;
        comm.Parameters["@ScoreString"].Value = this.ScoreString;
        comm.Parameters["@RemarkString"].Value = this.RemarkString;
        comm.Parameters["@StructureID"].Value = this.StructureID;
        comm.Parameters["@StartDate"].Value = this.StartDate;
        comm.Parameters["@EndDate"].Value = this.EndDate;
        comm.Parameters["@Status"].Value = this.Status;
        comm.Parameters["@TeamName"].Value = this.TeamName;
        comm.Parameters["@TemplateID"].Value = this.TemplateID;



        comm.Parameters["@InsertedID"].Direction = ParameterDirection.Output;
        MP.CloseConn();
        conn.Open();
        comm.ExecuteNonQuery();
        conn.Close();

        return Utils.StrToInt(comm.Parameters["@InsertedID"].Value, 0);


    }

    public static bool UpdateScore(int TotalScore, string ScoreString, string RemarkString, int Status, int ID)
    {
        //不存在的記錄
        if (!(new MP_Score(ID).Flag))
        {
            return false;
        }


        SqlConnection conn = MP.conn;
        SqlCommand comm = new SqlCommand("update MS_Score set TotalScore = @TotalScore , ScoreString = @ScoreString , RemarkString = @RemarkString , Status = @Status where ID = @ID ", conn);

        comm.Parameters.Add(new SqlParameter("@TotalScore", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@ScoreString", SqlDbType.NVarChar, 1000));
        comm.Parameters.Add(new SqlParameter("@RemarkString", SqlDbType.NVarChar, 2000));
        comm.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int));



        comm.Parameters["@TotalScore"].Value = TotalScore;
        comm.Parameters["@ScoreString"].Value = ScoreString;
        comm.Parameters["@RemarkString"].Value = RemarkString;
        comm.Parameters["@ID"].Value = ID;
        comm.Parameters["@Status"].Value = Status;


        MP.CloseConn();
        conn.Open();
        comm.ExecuteNonQuery();
        conn.Close();

        return true;
    }



    public static MP_Score[] GetMP_Score(string TeamName , string ScoreNo)
    {
        SqlConnection conn = MP.conn;
        SqlCommand comm = new SqlCommand("select ID from MS_Score where TeamName = '" + TeamName + "'  and  ScoreNo ='" + ScoreNo + "'", conn);
        DataTable TmpDataTable = new DataTable();
        SqlDataAdapter TmpSqlDataAdapter = new SqlDataAdapter();
        TmpSqlDataAdapter.SelectCommand = comm;
        conn.Open();
        TmpSqlDataAdapter.Fill(TmpDataTable);
        conn.Close();
        MP_Score[] ReturnMP_Score = new MP_Score[TmpDataTable.Rows.Count];
        for (int i = 0; i < TmpDataTable.Rows.Count; i++)
        {
            ReturnMP_Score[i] = new MP_Score(Convert.ToInt32(TmpDataTable.Rows[i]["ID"]));
        }

        return ReturnMP_Score;

    }


    public static bool UpdateMP_ScoreStatus(string TeamName, string ScoreNo, int Status)
    {
        SqlConnection conn = MP.conn;
        SqlCommand comm = new SqlCommand("update MS_Score set Status=@Status where ScoreNo = @ScoreNo and TeamName = @TeamName ", conn);

        
        comm.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@TeamName", SqlDbType.NVarChar, 50));
        comm.Parameters.Add(new SqlParameter("@ScoreNo", SqlDbType.VarChar, 6));



        comm.Parameters["@Status"].Value = Status;
        comm.Parameters["@ScoreNo"].Value = ScoreNo;
        comm.Parameters["@TeamName"].Value = TeamName;


        MP.CloseConn();
        conn.Open();
        comm.ExecuteNonQuery();
        conn.Close();

        return true;
    }



    public string GetUserName
    {
        get
        {
            return UserName;
        }
    }

    public int GetTotalScore
    {
        get
        {
            return TotalScore;
        }
    }

    public string GetScoreString
    {
        get
        {
            return ScoreString;
        }
    }

    public string GetRemarkString
    {
        get
        {
            return RemarkString;
        }
    }

    public string GetScoreNo
    {
        get
        {
            return ScoreNo;
        }
    }

    public DateTime GetStartDate
    {
        get
        {
            return StartDate;
        }
    }

    public DateTime GetEndDate
    {
        get
        {
            return EndDate;
        }
    }

    public int GetStructureID
    {
        get
        {
            return StructureID;
        }
    }

    public string GetTeamName
    {
        get
        {
            return TeamName;
        }
    }

    public int GetStatus
    {
        get
        {
            return Status;
        }
    }

    public bool GetFlag
    {
        get
        {
            return Flag;
        }
    }

    public int GetTemplateID
    {
        get
        {
            return TemplateID;
        }
    }

    public int GetID
    {
        get
        {
            return ID;
        }
    }


}






///public MP_SpItem
public class MP_SpItem
{
    private string UserName;
    private string Item;
    private DateTime StartDate;
    private DateTime EndDate;
    private int Score;
    private DateTime InsertDate;

    public MP_SpItem(string UserName, string Item, string StartDate, string EndDate, int Score)
    {
        this.UserName = UserName;
        this.Item = Item;
        this.StartDate = Convert.ToDateTime(StartDate);
        this.EndDate = Convert.ToDateTime(EndDate);
        this.Score = Score;
    }

    public MP_SpItem(int ID)
    {
        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("select * from MS_SPScore where ID = " + ID, conn);
        SqlDataReader TmpSqlDataReader;
        conn.Open();
        TmpSqlDataReader = comm.ExecuteReader();
        if (TmpSqlDataReader.HasRows)
        {
            TmpSqlDataReader.Read();
            UserName = TmpSqlDataReader["UserName"].ToString();
            Item = TmpSqlDataReader["Item"].ToString();
            StartDate = Convert.ToDateTime(TmpSqlDataReader["StartDate"]);
            EndDate = Convert.ToDateTime(TmpSqlDataReader["EndDate"]);
            Score = Convert.ToInt32(TmpSqlDataReader["Score"]);
        }
        conn.Close();
    }

    public int InsertSpItem()
    {

        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("Insert into MS_SPScore(UserName,Item,StartDate,EndDate,Score) values(@UserName,@Item,@StartDate,@EndDate,@Score) ; select @InsertID = @@identity", conn);
        comm.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 200));
        comm.Parameters.Add(new SqlParameter("@Item", SqlDbType.NVarChar, 200));
        comm.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.SmallDateTime));
        comm.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.SmallDateTime));
        comm.Parameters.Add(new SqlParameter("@Score", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@InsertID", SqlDbType.Int));

        comm.Parameters["@UserName"].Value = UserName;
        comm.Parameters["@Item"].Value = Item;
        comm.Parameters["@StartDate"].Value = StartDate;
        comm.Parameters["@EndDate"].Value = EndDate;
        comm.Parameters["@Score"].Value = Score;
        conn.Open();
        comm.Parameters["@InsertID"].Direction = ParameterDirection.Output;
        comm.ExecuteNonQuery();
        conn.Close();
        return Convert.ToInt32(comm.Parameters["@InsertID"].Value);

    }

    public static int DelSpItem(int ID, bool ScoreZeroFlag )
    {
        //ScoreZeroFlag true 代表  只能刪除 Score 為 0 的項目
        
        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("select Score from MS_SPScore where ID = " + ID.ToString(), conn);
        conn.Open();
        SqlDataReader TmpSqlDataReader = comm.ExecuteReader();
        if (!TmpSqlDataReader.HasRows)
        {
            conn.Close();
            return -1;
        }
        TmpSqlDataReader.Read();
        if (TmpSqlDataReader.GetInt32(0) != 0 && ScoreZeroFlag)
        {
            conn.Close();
            return 0;
        }
        conn.Close();

        conn.Open();
        comm = new SqlCommand("delete from MS_SPScore where ID = " + ID.ToString(), conn);
        comm.ExecuteNonQuery();
        conn.Close();
        return 1;

    }

    public static void UpdateScore(int ID, int Score)
    {
        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("update MS_SPScore set Score = " + Score + "  where  ID = " + ID, conn);
        //HttpContext.Current.Response.Write(comm.CommandText + "</br>");
        conn.Open();
        comm.ExecuteNonQuery();
        conn.Close();
    }
}

// Check Items Class

public class MP_CheckItems
{
    //
    private MP_CheckItemMulti[] CheckItems;
    private int CheckScoresLength;

    //
    public  MP_CheckItems(int TemplateID)
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(HttpContext.Current.Server.MapPath("~/MP/XML/CheckItem.xml"));

        XmlNodeList ItemNodeList = XmlDoc.SelectNodes("/root/Template[@ID='" + TemplateID + "']/item");
        CheckScoresLength = 0;

        
        CheckItems = new MP_CheckItemMulti[ItemNodeList.Count];
        for (int i = 0; i < ItemNodeList.Count; i++)
        {
            
            MP_CheckItemSignle[] TmpSubItem = new MP_CheckItemSignle[1];
            string title = ItemNodeList[i].Attributes.GetNamedItem("text").Value;
            if (ItemNodeList[i].HasChildNodes)
            {
                //獲得子節點
                XmlNodeList SubItemNodeList = ItemNodeList[i].ChildNodes;

                TmpSubItem = new MP_CheckItemSignle[SubItemNodeList.Count];
                //HttpContext.Current.Response.Write(TmpSubItem.Length);

                for (int j = 0; j < SubItemNodeList.Count; j++)
                {
                    string text = SubItemNodeList[j].Attributes.GetNamedItem("text").Value;
                    int MaxValue = SubItemNodeList[j].Attributes.GetNamedItem("MaxValue").Value == "" ? 1000 : Convert.ToInt32(SubItemNodeList[j].Attributes.GetNamedItem("MaxValue").Value);
                    int MinValue = SubItemNodeList[j].Attributes.GetNamedItem("MinValue").Value == "" ? 0 : Convert.ToInt32(SubItemNodeList[j].Attributes.GetNamedItem("MinValue").Value);
                    TmpSubItem[j] = new MP_CheckItemSignle(text, MaxValue, MinValue);
                    CheckScoresLength++;

                }


            }
            else
            {
                TmpSubItem = new MP_CheckItemSignle[1];
                int MaxValue = ItemNodeList[i].Attributes.GetNamedItem("MaxValue").Value == "" ? 1000 : Convert.ToInt32(ItemNodeList[i].Attributes.GetNamedItem("MaxValue").Value);
                int MinValue = ItemNodeList[i].Attributes.GetNamedItem("MinValue").Value == "" ? 0 : Convert.ToInt32(ItemNodeList[i].Attributes.GetNamedItem("MinValue").Value);
                TmpSubItem[0] = new MP_CheckItemSignle("", MaxValue, MinValue);
                CheckScoresLength++;

            }
            CheckItems[i] = new MP_CheckItemMulti(title, TmpSubItem);
         

        }

        //for (int i = 0; i < CheckItems.Length; i++)
        //{
        //    HttpContext.Current.Response.Write("title  " + CheckItems[i].GetTitle + "<br/>");
        //    HttpContext.Current.Response.Write("子項目  " + CheckItems[i].GetSubItems.Length + "<br/>");

        //    for (int j = 0; j < CheckItems[i].GetSubItems.Length; j++)
        //    {
        //        HttpContext.Current.Response.Write("子項目標題  " + CheckItems[i].GetSubItems[j].GetText + "<br/>");
        //        HttpContext.Current.Response.Write("最高  " + CheckItems[i].GetSubItems[j].GetMaxValue + "---->");
        //        HttpContext.Current.Response.Write("最低  " + CheckItems[i].GetSubItems[j].GetMinValue + "<br/>");
        //    }

        //    HttpContext.Current.Response.Write("###################################################<br/>");
        //}

       
    }

    public MP_CheckItemMulti[] GetCheckItems
    {
        get
        {
            return CheckItems;
        }
    }

    public int GetCheckScoreLength
    {
        get
        {
            return CheckScoresLength;
        }
    }
}

// Check Item Single 

public class MP_CheckItemSignle
{
    private string text;
    private int MaxValue;
    private int MinValue;

    public MP_CheckItemSignle(string text, int MaxValue , int MinValue)
    {
        this.text = text;
        this.MaxValue = MaxValue;
        this.MinValue = MinValue;
    }

    public string GetText
    {
        get
        {
            return text;
        }
    }

    public int GetMaxValue
    {
        get
        {
            return MaxValue;
        }
    }

    public int GetMinValue
    {
        get
        {
            return MinValue;
        }
    }


}

public class MP_CheckItemMulti
{
    private string title;
    private MP_CheckItemSignle[] subItems;
    public MP_CheckItemMulti(string title, MP_CheckItemSignle[] subItems)
    {
        this.title = title;
        this.subItems = subItems;
    }

    public string GetTitle
    {
        get
        {
            return title;
        }
    }

    public MP_CheckItemSignle[] GetSubItems
    {
        get
        {
            return subItems;
        }
    }
}


//Structure

public class MP_Strcuture
{
    private XmlDocument XMLDoc;
    public MP_Strcuture (string StructureFile)
    {
        XMLDoc = new XmlDocument();
        XMLDoc.Load(StructureFile);
    }

    public string GetUserLevel(string UserName)
    {
        XmlNode a = XMLDoc.SelectSingleNode("//*[@UserName='" + UserName + "']");

        if (a == null)
        {
            XmlNodeList allNodeList = XMLDoc.SelectNodes("//*[@UserName]");
            for (int i = 0; i < allNodeList.Count; i++)
            {
                if (allNodeList[i].Attributes.GetNamedItem("UserName").Value.ToLower() == UserName.ToLower())
                {
                    a = allNodeList[i];
                    break;
                }
            }
        }

       if (a == null)
            return "";

        switch (a.Name)
        {
            case "Member":
                return "Member";
                break;
            case "TeamLeader":
                return "TeamLeader";
                break;
            case "Manager":
                return "Manager" + a.Attributes.GetNamedItem("Level").Value;
                break;
            default:
                return "";
                break;
        }

    }


    public int GetUserLevelInt(string UserName)
    {
        int ReturnInt = -1;
        switch (GetUserLevel(UserName))
        {
            case "Member":
                ReturnInt = 0;
                break;
            case "TeamLeader":
                ReturnInt = 1;
                break;
            case "Manager1":
                ReturnInt = 2;
                break;
            case "Manager2":
                ReturnInt = 3;
                break;
            case "Manager3":
                ReturnInt = 4;
                break;

            default:
                break;
        }
        return ReturnInt;
    }


    public int GetMyDutyLevel(string UserName)
    {
        if (XMLDoc.SelectSingleNode("//*[@UserName='" + UserName + "']") != null)
        {
            if (XMLDoc.SelectSingleNode("//*[@UserName='" + UserName + "']").Attributes.GetNamedItem("DutyLevel") != null)
            {
                return Convert.ToInt32(XMLDoc.SelectSingleNode("//*[@UserName='" + UserName + "']").Attributes.GetNamedItem("DutyLevel").Value);
            }
        }
        return -1;

    }


    public string GetMyTeamName(string UserName)
    {
        if (XMLDoc.SelectSingleNode("//*[@UserName='" + UserName + "']") != null && XMLDoc.SelectSingleNode("//*[@UserName='" + UserName + "']").ParentNode != null)
        {
            if (XMLDoc.SelectSingleNode("//*[@UserName='" + UserName + "']").ParentNode.Name == "Team")
            {
                return XMLDoc.SelectSingleNode("//*[@UserName='" + UserName + "']").ParentNode.Attributes.GetNamedItem("Name").Value;
            }
        }
        return "";
    }


    /// <summary>
    /// 返回所有成員
    /// </summary>
    /// <returns>0,表示不包含Leader, 1 包含Leader</returns>
    public string[] GetTeamMember(string TeamName,int IncludeLeaderFlag)
    {
        string[] ReturnString;

        if (IncludeLeaderFlag == 0)
        {

            XmlNodeList MemberNodes = XMLDoc.SelectNodes("//Team[@Name='" + TeamName + "']/Member");
            ReturnString = new string[MemberNodes.Count];
            for (int i = 0; i < MemberNodes.Count; i++)
            {
                ReturnString[i] = MemberNodes[i].Attributes.GetNamedItem("UserName").Value;
            }
        }
        else
        {
            XmlNode TeamNode = XMLDoc.SelectSingleNode("//Team[@Name='" + TeamName + "']");
            ReturnString = new string[TeamNode.ChildNodes.Count];
            for (int i = 0; i < TeamNode.ChildNodes.Count; i++)
            {
                ReturnString[i] = TeamNode.ChildNodes[i].Attributes.GetNamedItem("UserName").Value;
            }

        }
        return ReturnString;
    }


    public string GetTeamLeader(string TeamName)
    {
        XmlNode TeamNode = XMLDoc.SelectSingleNode("//Team[@Name='" + TeamName + "']");
        if (TeamNode.SelectSingleNode("./TeamLeader") == null)
        {
            return TeamNode.ParentNode.Attributes.GetNamedItem("UserName").Value;

        }
        else
        {
            return TeamNode.SelectSingleNode("./TeamLeader").Attributes.GetNamedItem("UserName").Value;
        }

    }
   
   
    public string[] GetAllTeamLeader()
    {

        XmlNodeList TeamLeaderNodes = XMLDoc.SelectNodes("//TeamLeader");
        string[] ReturnString = new string[TeamLeaderNodes.Count];
        for (int i = 0; i < TeamLeaderNodes.Count; i++)
        {
            ReturnString[i] = TeamLeaderNodes[i].Attributes.GetNamedItem("UserName").Value;
        }
        return ReturnString;
    }

    public string[] GetAllTeam()
    {
        XmlNodeList TeamNodes = XMLDoc.SelectNodes("//Team");
        string[] ReturnString = new string[TeamNodes.Count];
        for (int i = 0; i < TeamNodes.Count; i++)
        {
            ReturnString[i] = TeamNodes[i].Attributes.GetNamedItem("Name").Value;
        }
        return ReturnString;
    }



    public bool IsMyUnderLing(string Uper, string Lower)
    {
        for (int i = 1; i < 4; i++)
        {
            if (GetMyManager(Lower, i).ToUpper() == Uper.ToUpper())
            {
                return true;
            }
        }

        if (GetMyTeamLeader(Lower).ToUpper() == Uper.ToUpper())
        {
            return true;
        }

        return false;
    }

    public string GetMyTeamLeader(string UserName)
    {
        if (GetMyTeamName(UserName) == "")
        {
            return "";
        }

        return (GetTeamLeader(GetMyTeamName(UserName)));
    }

    public bool  IsMyUnderTeam(string UserName, string TeamName)
    {

        if (GetTeamLeader(TeamName) == UserName)
            return true;
        if (IsMyUnderLing(UserName, GetTeamMember(TeamName, 0)[0]))
            return true;
        return false;
    }

    /// <summary>
    /// 獲得主管
    /// </summary>
    /// <param name="UserName"></param>
    /// <param name="Level"></param>
    /// <returns></returns>
    public string GetMyManager(string UserName, int Level)
    {
        XmlNodeList ManagerNodes = XMLDoc.SelectNodes("//Manager[@Level='" + Level.ToString() + "']");
        for (int i = 0; i < ManagerNodes.Count; i++)
        {
            if (ManagerNodes[i].SelectSingleNode(".//*[@UserName='" + UserName + "']") != null)
            {
                return ManagerNodes[i].Attributes.GetNamedItem("UserName").Value;
            }
        }
        return "";
    }

    /// <summary>
    /// 獲得在Level1和Level2之間的主管
    /// </summary>
    /// <param name="Level1">Lower Level</param>
    /// <param name="Level2">High Level</param>
    /// <returns></returns>
    public string[] GetManager(int Level1, int Level2)
    {
        XmlNodeList ManagerNodes = XMLDoc.SelectNodes("//Manager[@Level >= '" + Level1 + "' and @Level <= '" + Level2 + "' ]");
        string[] ReturnStringArray = new string[ManagerNodes.Count];
        for (int i = 0; i < ManagerNodes.Count; i++)
        {
            ReturnStringArray[i] = ManagerNodes[i].Attributes.GetNamedItem("UserName").Value;
        }

        return ReturnStringArray;
    }

    /// <summary>
    /// 獲得所有的成員
    /// </summary>
    /// <param name="Flag">1 表示不包含Michael</param>
    /// <returns></returns>
    public string[] GetAllUser(int Flag)
    {
        XmlNodeList UserNodes = XMLDoc.SelectNodes("//*[@UserName]");
        string[] ReturnString = new string[UserNodes.Count];
        for (int i = 0; i < UserNodes.Count; i++)
        {
            ReturnString[i] = UserNodes[i].Attributes.GetNamedItem("UserName").Value;
        }
        return ReturnString;
    }

    public string[] GetAllUser()
    {
        XmlNodeList UserNodes = XMLDoc.SelectNodes("//*[@UserName != 'Michael_Chen']");
        string[] ReturnString = new string[UserNodes.Count];
        for (int i = 0; i < UserNodes.Count; i++)
        {
            ReturnString[i] = UserNodes[i].Attributes.GetNamedItem("UserName").Value;
        }
        return ReturnString;
    }


}

//Team Score MP_TeamScore

public class MP_TeamScore
{
    private int id;
    private string TemplateID;
    private string Title;
    private int Score1 = 0;
    private int Score2 = 1;
    private int ScoreS = 0;
    private decimal TotalScore = 0M;
    private string Remark;
    private int Status;
    private string TeamName;
    private string ScoreNo;


    public int GetId
    {
        get
        {
            return id;
        }
    }

    public string GetTemplateID
    {
        get
        {
            return TemplateID;
        }
    }

    public string GetTitle
    {
        get
        {
            return Title;
        }
    }

    public int GetScore1
    {
        get
        {
            return Score1;
        }
    }

    public int GetScore2
    {
        get
        {
            return Score2;
        }
    }

    public int GetScoreS
    {
        get
        {
            return ScoreS;

        }
    }

    public decimal GetTotalScore
    {
        get
        {
            return TotalScore;
        }
    }

    public string GetRemark
    {
        get
        {
            return Remark;
        }
    }

    public string GetTeamName
    {
        get
        {
            return TeamName;
        }
    }

    public int GetStatus
    {
        get
        {
            return Status;
        }
    }

    public string GetScoreNo
    {
        get
        {
            return ScoreNo;
        }
    }

    

    public MP_TeamScore(string TemplateID, string Title, int Score1, int Score2, int ScoreS, decimal TotalScore, string Remark, string TeamName, string ScoreNo, int Status)
    {
        this.TemplateID = TemplateID;
        this.Title = Title;
        this.Score1 = Score1;
        this.Score2 = Score2;
        this.ScoreS = ScoreS;
        this.TotalScore = TotalScore;
        this.Remark = Remark;
        this.Status = Status;
        this.TeamName = TeamName;
        this.ScoreNo = ScoreNo;
        
    }

    public MP_TeamScore(int ID)
    {
        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("select * from MS_TeamScore where ID = " + ID, conn);
        conn.Open();
        SqlDataReader TmpSqlDataReader = comm.ExecuteReader();
        while (TmpSqlDataReader.Read())
        {
            this.id = ID;
            this.TemplateID = TmpSqlDataReader["TemplateID"].ToString();
            this.Title = TmpSqlDataReader["Title"].ToString();
            this.Score1 = Utils.StrToInt(TmpSqlDataReader["Score1"], 0);
            this.Score2 = Utils.StrToInt(TmpSqlDataReader["Score2"], 1);
            this.ScoreS = Utils.StrToInt(TmpSqlDataReader["ScoreS"], 0);
            this.TotalScore = Convert.ToDecimal(TmpSqlDataReader["TotalScore"]);
            this.Remark = TmpSqlDataReader["Remark"].ToString();
            this.Status = Utils.StrToInt(TmpSqlDataReader["Status"], 0);
            this.TeamName = TmpSqlDataReader["TeamName"].ToString();
            this.ScoreNo = TmpSqlDataReader["ScoreNo"].ToString();

        }
        conn.Close();
    }

    public int InsertScore()
    {
        //存在相同的記錄
     
        
        SqlConnection conn = MP.conn;
        SqlCommand comm = new SqlCommand("insert into MS_TeamScore(TemplateID,Title,ScoreNo,TeamName,Score1,Score2,ScoreS,TotalScore,Remark,Status) values(@TemplateID,@Title,@ScoreNo,@TeamName,@Score1,@Score2,@ScoreS,@TotalScore,@Remark,@Status) ; select @InsertedID = @@Identity", conn);
        
        //@TemplateID,@Title,@ScoreNo,@TeamName,@Score1,@Score2,@ScoreS,@TotalScore,@Remark,@Status
        comm.Parameters.Add(new SqlParameter("@TemplateID", SqlDbType.NVarChar, 50));
        comm.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200));
        comm.Parameters.Add(new SqlParameter("@ScoreNo", SqlDbType.VarChar, 6));
        comm.Parameters.Add(new SqlParameter("@TeamName", SqlDbType.NVarChar, 50));
        comm.Parameters.Add(new SqlParameter("@Score1", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@Score2", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@ScoreS", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@TotalScore", SqlDbType.Decimal));
        comm.Parameters.Add(new SqlParameter("@Remark", SqlDbType.NVarChar,1000));
        comm.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@InsertedID", SqlDbType.Int));

        comm.Parameters["@TemplateID"].Value = this.TemplateID;
        comm.Parameters["@Title"].Value = this.Title;
        comm.Parameters["@ScoreNo"].Value = this.ScoreNo;
        comm.Parameters["@TeamName"].Value = this.TeamName;
        comm.Parameters["@Score1"].Value = this.Score1;
        comm.Parameters["@Score2"].Value = this.Score2;
        comm.Parameters["@ScoreS"].Value = this.ScoreS;
        comm.Parameters["@TotalScore"].Value = this.TotalScore;
        comm.Parameters["@Remark"].Value = this.Remark;
        comm.Parameters["@Status"].Value = this.Status;
        
        comm.Parameters["@InsertedID"].Direction = ParameterDirection.Output;

        MP.CloseConn();
        conn.Open();
        comm.ExecuteNonQuery();
        conn.Close();

        return Utils.StrToInt(comm.Parameters["@InsertedID"].Value, 0);


    }


    

}


//Team 評分標準

public class MP_ScoreStandard
{
    private int type;
    private string Title;
    private decimal Score;
    private string unit;
    private bool active;
    private string id;

    public int GetType
    {
        get
        {
            return type;
        }
    }

    public string GetTitle
    {
        get
        {
            return Title;
        }
    }

    public decimal GetScore
    {
        get
        {
            return Score;
        }
    }

    public string GetUnit
    {
        get
        {
            return unit;
        }
    }

    public bool GetActive
    {
        get
        {
            return active;
        }
    }

    public string GetId
    {
        get
        {
            return id;
        }
    }

    public string[] GetUnitArray
    {
        get
        {
            return Utils.SplitString(unit, ".");
        }
    }

    public MP_ScoreStandard(string ID)
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(HttpContext.Current.Server.MapPath("~/MP/XML/StandardScore.xml"));
        XmlNode ItemNode = XmlDoc.SelectSingleNode("//item[@id='" + ID + "']");
        if (ItemNode != null)
        {
            this.id = ID;
            this.Title = ItemNode.InnerText;
            this.Score = Convert.ToDecimal(ItemNode.Attributes.GetNamedItem("score").Value);
            this.unit = ItemNode.Attributes.GetNamedItem("unit").Value;
            this.type = Convert.ToInt32(ItemNode.Attributes.GetNamedItem("type").Value);
            this.active = ItemNode.Attributes.GetNamedItem("active").Value == "0" ? true : false;
        }
    }
    
    public static MP_ScoreStandard[] GetStandardScoreTemplate(int ID)
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(HttpContext.Current.Server.MapPath("~/MP/XML/StandardScore.xml"));
        XmlNodeList ItemNodes = XmlDoc.SelectNodes("//TeamTemplate[@id='" + ID + "']/item");

        MP_ScoreStandard[] Return_MP_ScoreStandart = new MP_ScoreStandard[ItemNodes.Count];

        for (int i = 0; i < ItemNodes.Count; i++)
        {
            Return_MP_ScoreStandart[i] = new MP_ScoreStandard(ItemNodes[i].Attributes.GetNamedItem("id").Value);
        }

        return Return_MP_ScoreStandart;
    }

    public static ListItem[] GetTemplateListItem()
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(HttpContext.Current.Server.MapPath("~/MP/XML/StandardScore.xml"));
        XmlNodeList TemplateNodes = XmlDoc.SelectNodes("//TeamTemplate");
        ListItem[] ReturnListItem = new ListItem[TemplateNodes.Count];
        for (int i = 0; i < TemplateNodes.Count; i++)
        {
            ReturnListItem[i] = new ListItem(TemplateNodes[i].Attributes.GetNamedItem("name").Value, TemplateNodes[i].Attributes.GetNamedItem("id").Value);      
        }
        return ReturnListItem;

    }

}


/// <summary>
/// 特殊貢獻值
/// </summary>

public class MP_SPCon
{
    private string UserName;
    private string Item;
    private DateTime StartDate;
    private DateTime EndDate;
    private int Score;
    private DateTime InsertDate;
    private string ScoreNo;

    public MP_SPCon(string UserName, string Item, string StartDate, string EndDate, int Score, string ScoreNo)
    {
        this.UserName = UserName;
        this.Item = Item;
        this.StartDate = Convert.ToDateTime(StartDate);
        this.EndDate = Convert.ToDateTime(EndDate);
        this.Score = Score;
        this.ScoreNo = ScoreNo;
    }

    public MP_SPCon(int ID)
    {
        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("select * from MS_SPScore where ID = " + ID, conn);
        SqlDataReader TmpSqlDataReader;
        conn.Open();
        TmpSqlDataReader = comm.ExecuteReader();
        if (TmpSqlDataReader.HasRows)
        {
            TmpSqlDataReader.Read();
            UserName = TmpSqlDataReader["UserName"].ToString();
            Item = TmpSqlDataReader["Item"].ToString();
            StartDate = Convert.ToDateTime(TmpSqlDataReader["StartDate"]);
            EndDate = Convert.ToDateTime(TmpSqlDataReader["EndDate"]);
            Score = Convert.ToInt32(TmpSqlDataReader["Score"]);
            ScoreNo = TmpSqlDataReader["ScoreNo"].ToString();
        }
        conn.Close();
    }

    public int InsertSPCon()
    {

        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("Insert into MS_SPCon(UserName,Item,StartDate,EndDate,Score,ScoreNo) values(@UserName,@Item,@StartDate,@EndDate,@Score,@ScoreNo) ; select @InsertID = @@identity", conn);
        comm.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 200));
        comm.Parameters.Add(new SqlParameter("@Item", SqlDbType.NVarChar, 200));
        comm.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.SmallDateTime));
        comm.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.SmallDateTime));
        comm.Parameters.Add(new SqlParameter("@Score", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@InsertID", SqlDbType.Int));
        comm.Parameters.Add(new SqlParameter("@ScoreNo", SqlDbType.VarChar, 6));

        comm.Parameters["@UserName"].Value = UserName;
        comm.Parameters["@Item"].Value = Item;
        comm.Parameters["@StartDate"].Value = StartDate;
        comm.Parameters["@EndDate"].Value = EndDate;
        comm.Parameters["@Score"].Value = Score;
        comm.Parameters["@ScoreNo"].Value = ScoreNo;
        conn.Open();
        comm.Parameters["@InsertID"].Direction = ParameterDirection.Output;
        comm.ExecuteNonQuery();
        conn.Close();
        return Convert.ToInt32(comm.Parameters["@InsertID"].Value);

    }

    public static int DelSpCon(int ID)
    {
        //ScoreZeroFlag true 代表  只能刪除 Score 為 0 的項目

        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("select Score from MS_SPCon where ID = " + ID.ToString(), conn);
        conn.Open();
        SqlDataReader TmpSqlDataReader = comm.ExecuteReader();
        if (!TmpSqlDataReader.HasRows)
        {
            conn.Close();
            return -1;
        }
        conn.Close();
        conn.Open();
        comm = new SqlCommand("delete from MS_SPCon where ID = " + ID.ToString(), conn);
        comm.ExecuteNonQuery();
        conn.Close();
        return 1;

    }

    public static int DelSpCon(int ID, bool ScoreZeroFlag)
    {
        //ScoreZeroFlag true 代表  只能刪除 Score 為 0 的項目

        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("select Score from MS_SPCon where ID = " + ID.ToString(), conn);
        conn.Open();
        SqlDataReader TmpSqlDataReader = comm.ExecuteReader();
        if (!TmpSqlDataReader.HasRows)
        {
            conn.Close();
            return -1;
        }

        TmpSqlDataReader.Read();
        if (TmpSqlDataReader.GetInt32(0) != 0 && ScoreZeroFlag)
        {
            conn.Close();
            return 0;
        }

        conn.Close();
        conn.Open();
        comm = new SqlCommand("delete from MS_SPCon where ID = " + ID.ToString(), conn);
        comm.ExecuteNonQuery();
        conn.Close();
        return 1;

    }


    public static void UpdateScore(int ID, int Score)
    {
        SqlConnection conn = MP.conn;
        MP.CloseConn();
        SqlCommand comm = new SqlCommand("update MS_SPCon set Score = " + Score + "  where  ID = " + ID, conn);
        //HttpContext.Current.Response.Write(comm.CommandText + "</br>");
        conn.Open();
        comm.ExecuteNonQuery();
        conn.Close();
    }
}