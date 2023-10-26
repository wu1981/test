using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Moldau.TPP;
using Moldau.Common;
using Moldau.Page;
using System.IO;


public partial class TPP_Search : Moldau.TPP.TPPPage
{
  
    //分數顯示TeamlistBox
    protected List<CheckBox> teamListBoxLists = new List<CheckBox>();

    //Project 顯示check listbox
    protected List<CheckBox> teamProjectlistBoxLists = new List<CheckBox>();

    protected List<TestTPP> displayTppList = new List<TestTPP>();

    protected string tableString = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        
        teamListBoxLists.Add(CheckBox1);
        teamListBoxLists.Add(CheckBox2);
        teamListBoxLists.Add(CheckBox3);
        teamListBoxLists.Add(CheckBox4);
        teamListBoxLists.Add(CheckBox5);
        teamListBoxLists.Add(CheckBox6);
        teamListBoxLists.Add(CheckBox7);
        teamListBoxLists.Add(CheckBox8);
        teamListBoxLists.Add(CheckBox9);
        teamListBoxLists.Add(CheckBox10);

        teamProjectlistBoxLists.Add(CheckBox11);
        teamProjectlistBoxLists.Add(CheckBox12);
        teamProjectlistBoxLists.Add(CheckBox13);
        teamProjectlistBoxLists.Add(CheckBox14);
        teamProjectlistBoxLists.Add(CheckBox15);
        teamProjectlistBoxLists.Add(CheckBox16);
        teamProjectlistBoxLists.Add(CheckBox17);
        teamProjectlistBoxLists.Add(CheckBox18);
        teamProjectlistBoxLists.Add(CheckBox19);
        teamProjectlistBoxLists.Add(CheckBox20);


        for(int i =0 ;i<teamListBoxLists.Count;i++)
        {
            teamListBoxLists[i].Visible = false;
            teamProjectlistBoxLists[i].Visible = false;
        }

        string[] allTeam = str.GetAllTeamName();

        for (int i = 0; i < allTeam.Length; i++)
        {
            teamListBoxLists[i].Visible = true;
            teamListBoxLists[i].Text = allTeam[i];

            teamProjectlistBoxLists[i].Visible = true;
            teamProjectlistBoxLists[i].Text = allTeam[i];
            
            
        }


        


        if (!IsPostBack)
        {
            for (int i = 0; i < allTeam.Length; i++)
            {
                teamListBoxLists[i].Checked = true;
                teamProjectlistBoxLists[i].Checked = true;
            }


            

            //初始化testBox
            startDateTextBox.Attributes.Add("readonly", "true");
            endDateTextBox.Attributes.Add("readonly", "true");
            //初始化TestBox數據

            int tDay = DateTime.Now.Day;

            if (tDay <= 28 && tDay >= 0)
            {
                startDateTextBox.Text = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 28).AddDays(1).ToShortDateString();
                endDateTextBox.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 28).ToShortDateString();
            }
            else
            {
                startDateTextBox.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 28).AddDays(1).ToShortDateString();
                endDateTextBox.Text = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 28).ToShortDateString();
            }

            //算週數
            DateTime startDate = DateTime.Parse(startDateTextBox.Text);
            DateTime endDate = DateTime.Parse(endDateTextBox.Text);

            WeekTextBox.Text = WorkDate.GetWeekOfYear(startDate).ToString() + "~" + WorkDate.GetWeekOfYear(endDate).ToString();
            
        }


    }


    protected void Button1_Click(object sender, EventArgs e)
    {
        string startString = "";
        string tmpString1 = "";
        string tmpString2 = "";
        string tmpString3 = "";
        DateTime sumStartDate;
        DateTime sumEndDate;
        
        //如果以週數統計,
        //1.檢驗填寫格式
        //同步填寫時間
        if (WeekSearchCheckBox.Checked)
        {
            if (WeekTextBox.Text == "")
            {
                Response.Write(PageJS.JSBack("調整週數時,週數不能為空"));
                return;
            }

            string[] tmpWeekString = PageCommon.SplitString(WeekTextBox.Text, "~");

            if (tmpWeekString.Length == 1)
            {
                if (!PageCommon.IsInt(tmpWeekString[0]))
                {
                    Response.Write(PageJS.JSBack("請注意書寫格式"));
                    return;
                }

                startDateTextBox.Text = WorkDate.GetFirstDateOfWeek(DateTime.Now, PageCommon.StrToInt(tmpWeekString[0], 1)).ToShortDateString();
                endDateTextBox.Text = WorkDate.GetLastDateOfWeek(DateTime.Now, PageCommon.StrToInt(tmpWeekString[0], 1)).ToShortDateString();
            }

            if (tmpWeekString.Length == 2)
            {
                if (!PageCommon.IsInt(tmpWeekString[0]) || !PageCommon.IsInt(tmpWeekString[1]))
                {
                    Response.Write(PageJS.JSBack("請注意書寫格式"));
                    return;
                }
                startDateTextBox.Text = WorkDate.GetFirstDateOfWeek(DateTime.Now, PageCommon.StrToInt(tmpWeekString[0], 1)).ToShortDateString();
                endDateTextBox.Text = WorkDate.GetLastDateOfWeek(DateTime.Now, PageCommon.StrToInt(tmpWeekString[1], 1)).ToShortDateString();
            }
        }


        //統計開始和結束時間
        sumStartDate = PageCommon.StrToDateTime(startDateTextBox.Text, DateTime.Now);
        sumEndDate = PageCommon.StrToDateTime(endDateTextBox.Text, DateTime.Now);

        if (startDateTextBox.Text == "" || endDateTextBox.Text == "" || DateTime.Compare(sumStartDate,sumEndDate) >0)
        {
            Response.Write(JSCode.JSBack("時間錯誤"));
            return;
        }

        //如果以日期為查詢時間的話,同步週數
        if (!WeekSearchCheckBox.Checked)
        {
            WeekTextBox.Text = WorkDate.GetWeekOfYear(sumStartDate).ToString() + "~" + WorkDate.GetWeekOfYear(sumEndDate).ToString();
        }


        //選出所有符合這個時間段的Project
        List<TestTPP>  tmpdisplayTppList = TestTPP.GetTestTPPList(sumStartDate, sumEndDate);
        List<TestTPP> tmpdisplayTppList1 = new List<TestTPP>();


        //去掉沒有選中的組別的Project
        List<string> projectdisplayTeamString = new List<string>();
        List<string> projectdisplayUserString = new List<string>();

        for (int i = 0; i < teamProjectlistBoxLists.Count; i++)
        {
            if (teamProjectlistBoxLists[i].Checked && teamProjectlistBoxLists[i].Visible == true)
            {
                projectdisplayTeamString.Add(teamProjectlistBoxLists[i].Text);
            }
        }

        //找出所有的選中的User
        for (int i = 0; i < projectdisplayTeamString.Count; i++)
        {
            for (int j = 0; j < str.GetTeamUser(projectdisplayTeamString[i], 0).Length; j++)
            {
                projectdisplayUserString.Add(str.GetTeamUser(projectdisplayTeamString[i], 0)[j]);
            }
        }
        //找出所有的Project,這個地方僅僅是過濾到 “Project     顯示組別”
        
        for (int i = 0; i < tmpdisplayTppList.Count; i++)
        {
            for (int j = 0; j < projectdisplayUserString.Count; j++)
            {
                if (tmpdisplayTppList[i].ProjectLeader == projectdisplayUserString[j])
                {
                    tmpdisplayTppList1.Add(tmpdisplayTppList[i]);
                    continue;
                }
                
            }
        }

        //這裡過濾掉SR/ER/PR 的 Project選項
        if (SRCheckBox.Checked || ERCheckBox.Checked || PRCheckBox.Checked)
        {
            List<int> checkedTestItemID = new List<int>();
            if(SRCheckBox.Checked)
                checkedTestItemID.Add(TPPRate.GetTestIDfromTestName("SR測試"));
            if(ERCheckBox.Checked)
                checkedTestItemID.Add(TPPRate.GetTestIDfromTestName("ER測試"));
            if (PRCheckBox.Checked)
                checkedTestItemID.Add(TPPRate.GetTestIDfromTestName("PR測試"));

            for (int i = 0; i < tmpdisplayTppList1.Count; i++)
            {
                for (int j = 0; j < checkedTestItemID.Count; j++)
                {
                    if (tmpdisplayTppList1[i].TestItem == checkedTestItemID[j])
                    {
                        displayTppList.Add(tmpdisplayTppList1[i]);
                    }
                }
            }
        }
        else
        {
            displayTppList = tmpdisplayTppList1;
        }


        //找出需要顯示的用戶名,組別

        //找出組別
        List<string> displayTeamString = new List<string>();
        List<string> displayUserString = new List<string>();
        
        decimal[] displayUserTotalTpp;

        ////actualtpp 定義////////
        decimal[] displayUserTotalActualTPP;
        ////actualtpp 定義////////

        //初始化顯示Team
        for (int i = 0; i < teamListBoxLists.Count; i++)
        {
            if (teamListBoxLists[i].Checked && teamListBoxLists[i].Visible == true)
            {
                displayTeamString.Add(teamListBoxLists[i].Text);
            }
        }

                
        
        
        //找出所有顯示用戶,並且寫出顯示String  tmpString1
        for (int i = 0; i < displayTeamString.Count; i++)
        {
            
            //顯示組的名稱
            tableString += "<td colspan=\"" + str.GetTeamUser(displayTeamString[i], 0).Length + "\">" + displayTeamString[i] + "</td>";


            //tmpString為用戶名稱的顯示字符串
            for (int j = 0; j < str.GetTeamUser(displayTeamString[i], 0).Length; j++)
            {
                tmpString1 += "<td style=\"width:65px\">" + str.GetTeamUser(displayTeamString[i], 0)[j] + "</td>";
                //初始化顯示組別
                displayUserString.Add(str.GetTeamUser(displayTeamString[i], 0)[j]);
            }

        }

        //開始字符串
        startString = "<table id=\"FormTable\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\" class=\"ContentTable_NoPadding\"  style=\"width:" +
                             (605 + displayUserString.Count * 65).ToString() +
                            "px; text-align: center; margin-top:10px; \">" +
                            "<tr class=\"GridColHeader\" >" +
                            "<td rowspan=\"2\"  width=\"55px\">WeekLy</td> " +
                            "<td  rowspan=\"2\" width=\"60px\"> Project</td>" +
                            "<td  rowspan=\"2\" width=\"80px\">Leader</td>" +
                            "<td  rowspan=\"2\" width=\"80px\">Test Type </td>" +
                            "<td  rowspan=\"2\" width=\"80px\">Sub Type </td>" +
                            "<td  rowspan=\"2\" width=\"250px\">Test Item</td>" +
                            "<td  rowspan=\"2\" width=\"65px\">Start Date</td> " +
                            "<td rowspan=\"2\" width=\"60px\">End Date</td>" +
                            "<td colspan=\"2\"  > Total TPP</td>" +
                            "<td colspan=\"2\" > Actual TPP</td>";



        tableString = startString + tableString + "</tr>";



        tableString += "<tr>" + "<td style=\"width:65px\">All</td>  <td style=\"width:65px\">Part</td>  <td style=\"width:65px\">All</td>  <td style=\"width:65px\">Part</td>" + tmpString1 + "</tr>";

        

        //初始化displayUserTotalTpp
        displayUserTotalTpp = new decimal[displayUserString.Count];

        ///////////////
        //actual tpp 初始化
        displayUserTotalActualTPP = new decimal[displayUserString.Count];       
        ///////////////
        
        //
        //顯示TPP數據


        for (int i = 0; i < displayTppList.Count; i++)
        {
            if (DateTime.Compare(displayTppList[i].EndDate, DateTime.Now.Date) < 0)
            {
                tableString += "<tr style=\"background:#ccc;\">";
            }
            else if (DateTime.Compare(displayTppList[i].EndDate, DateTime.Now.Date) >= 0 && DateTime.Compare(displayTppList[i].StartDate, DateTime.Now.Date) <= 0)
            {
                tableString += "<tr style=\"background:#95B3D7;\">";
            }
            else
            {
                tableString += "<tr>";
            }
            tableString += "<td>" + WorkDate.GetWeekOfYear(displayTppList[i].StartDate).ToString() + "---" +  WorkDate.GetWeekOfYear(displayTppList[i].EndDate).ToString() + "</td>";
            tableString += "<td>" + displayTppList[i].ProjectName + "</td>";
            tableString += "<td>" + displayTppList[i].ProjectLeader + "</td>";

            TPPRate tmpRate = new TPPRate(displayTppList[i].TestItem);
            tableString += "<td>" + new TPPRateType(tmpRate.TPPRateType, Server.MapPath("~/TPP/Config/TPPRateType.xml")).RateType + "</td>";
            tableString += "<td>" + tmpRate.TPPRateName + "</td>";
            tableString += "<td>" + displayTppList[i].ItemName + "</td>";

            if (DateTime.Compare(displayTppList[i].StartDate, sumStartDate) >= 0 && DateTime.Compare(displayTppList[i].EndDate, sumEndDate) <= 0)
            {
                tableString += "<td>" + displayTppList[i].StartDate.ToShortDateString() + "</td>";
                tableString += "<td>" + displayTppList[i].EndDate.ToShortDateString() + "</td>";


                ///顯示兩次
                
                
                tableString += "<td>" + displayTppList[i].TotalTPP.ToString() + "</td>";

                tableString += "<td>" + displayTppList[i].TotalTPP.ToString() + "</td>";

                ////actual tpp
                tableString += "<td>" + displayTppList[i].ActualTPP.ToString() + "</td>";
                ////actualtpp
                
                ////actual tpp
                tableString += "<td>" + displayTppList[i].ActualTPP.ToString() + "</td>";
                ////actualtpp
                  
                 
            }
            else
            {
                tableString += "<td style=\"background:#C0504D\">" + displayTppList[i].StartDate.ToShortDateString() + "</td>";
                tableString += "<td style=\"background:#C0504D\">" + displayTppList[i].EndDate.ToShortDateString() + "</td>";


                tableString += "<td style=\"background:#C0504D\">" + displayTppList[i].TotalTPP.ToString() + "</td> <td style=\"background:#C0504D\">" + TPPTool.SplitSumTpp(displayTppList[i].StartDate, displayTppList[i].EndDate, sumStartDate, sumEndDate, (decimal)displayTppList[i].TotalTPP).ToString() + "</td>";
                ////actual tpp
                tableString += "<td style=\"background:#C0504D\">" + displayTppList[i].ActualTPP.ToString() + "</td> <td style=\"background:#C0504D\">" + TPPTool.SplitSumTpp(displayTppList[i].StartDate, displayTppList[i].EndDate, sumStartDate, sumEndDate, (decimal)displayTppList[i].ActualTPP).ToString() + "</td>";
                ////actualtpp
                 
            }


            

            //顯示每個用戶的TPP點數


            for (int j = 0; j < displayUserString.Count; j++)
            {

                

                bool hasThisUser = false;
                decimal currentTTP = (decimal)0;
                //實際actualtpp
                decimal currentActualTpp = (decimal)0;
                for (int k = 0; k < displayTppList[i].PersonalTPPList.Count; k++)
                {
                    if (displayTppList[i].PersonalTPPList[k].UserName == displayUserString[j])
                    {
                        hasThisUser = true;
                        currentTTP = TPPTool.SplitSumTpp(displayTppList[i].StartDate, displayTppList[i].EndDate, sumStartDate, sumEndDate, displayTppList[i].PersonalTPPList[k].TPPCount);

                        displayUserTotalTpp[j] += currentTTP;
                        
                        ////actualTPP
                        ////按照百分比來計算每個人的實際TPP分
                        if (displayTppList[i].TotalTPP != 0)
                        {
                            currentActualTpp = TestTPP.FormatDecmialF1(displayTppList[i].ActualTPP * currentTTP / displayTppList[i].TotalTPP);
                            displayUserTotalActualTPP[j] += currentActualTpp;
                        }
                        
                        ////actualTPP

                    }
                }

                if (hasThisUser)
                {
                    //將用戶的tpp點數顯示為actual tpp點數//
                    //tableString += "<td>" + currentTTP.ToString() + "</td>";
                    //將用戶的tpp點數顯示為actual tpp點數//
                    tableString += "<td>" + currentActualTpp.ToString() + "</td>";
                }
                else
                {
                    tableString += "<td></td>";
                }
            }

            tableString += "</tr>";
            
        }

       
        //個人總的TPP點數
        tableString += "<tr style=\"background:#99FF99;font-weight:bold\"><td colspan=\"12\">Personal Total TPP</td>";

        for (int i = 0; i < displayUserTotalTpp.Length; i++)
        {
            tableString += "<td>" + displayUserTotalTpp[i].ToString() + "</td>";
            
        }
        tableString += "</tr>";


        /////Actual TPP 顯示

        tableString += "<tr style=\"background:#99FF99;font-weight:bold\"><td colspan=\"12\">Personal Actual Total TPP</td>";

        for (int i = 0; i <  displayUserTotalActualTPP.Length; i++)
        {
            tableString += "<td>" + TestTPP.FormatDecmialF1(displayUserTotalActualTPP[i]).ToString() + "</td>";

        }
        tableString += "</tr>";

        /////Actual TPP 顯示

        //Team總的TPP點數
        tableString += "<tr style=\"background:#FFCC00;font-weight:bold\"><td colspan=\"12\">Team Total TPP</td>";

        
        int tmpCurrent=0;
        decimal departmentTotalTPP = (decimal)0;

        for(int i =0; i<displayTeamString.Count;i++)
        {
            tableString += "<td colspan=\"" + str.GetTeamUser(displayTeamString[i], 0).Length + "\">";
            decimal teamTotalTpp = (decimal)0;
            for (int j = tmpCurrent; j < tmpCurrent + str.GetTeamUser(displayTeamString[i], 0).Length; j++)
            {
                teamTotalTpp += displayUserTotalTpp[j];
            }

            tableString += teamTotalTpp.ToString() + "</td>"; 
            tmpCurrent += str.GetTeamUser(displayTeamString[i], 0).Length;

            departmentTotalTPP += teamTotalTpp;
        }

        tableString += "</tr>";

        ////Section的總點數,該地方顯示每個Section的總點數,其中Section包括主管的測試點數,
        



        ////部門TPP總數

        //tableString += "<tr style=\"background:#FFCC00;font-weight:bold\"><td colspan=\"7\">Department Total TPP</td>";
        //tableString += "<td style=\"background:#FFCC00;font-weight:bold\" colspan=\"" + displayUserString.Count.ToString() + "\">" + departmentTotalTPP.ToString() + "</td></tr>";
        

        tableString += "</table>";

        if( ((Button)sender).ID == Button2.ID)
        {
            
            //加上前面顯示的html文檔
            tmpString2 += "<html xmlns=\"http://www.w3.org/1999/xhtml\">" + System.Environment.NewLine +
                    "<head>" + System.Environment.NewLine +
                    "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />" + System.Environment.NewLine +
                    "<title>填寫申請單</title>" + System.Environment.NewLine +
                    "<link href=\"http://hd-qt2/tpp/CSS/Main.css\" media=\"all\" rel=\"stylesheet\" type=\"text/css\" />" + System.Environment.NewLine +
                    "</head>" + System.Environment.NewLine +
                    "<body>" + System.Environment.NewLine;
            tmpString3 = "</body>";
            
            tmpString2 += "<font style=\"font-size:15pt; font-weight:bold\">Sum Date:" + startDateTextBox.Text + "~" + endDateTextBox.Text + "</font><br/>";

            Response.Clear();
            Response.Buffer = true;


            Response.AppendHeader("Content-Disposition", "attachment;filename=" + DateTime.Now.ToString("yyyyMMdd") + ".xls");
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.ContentType = "application/vnd.ms-excel";

            //Response.Write(tableString);
            //Response.End();




            System.IO.StringWriter oStringWriter = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter oHtmlTextWriter = new System.Web.UI.HtmlTextWriter(oStringWriter);

            oHtmlTextWriter.Write(tmpString2 + tableString + tmpString3);
            Response.Output.Write(oStringWriter.ToString());

            Response.Flush();
            Response.End();

        }
    }


  


    protected void Button2_Click(object sender, EventArgs e)
    {
        

        //Response.Clear();
        //Response.Buffer = true;
        //Response.AppendHeader("Content-Disposition", "attachment;filename=" + DateTime.Now.ToString("yyyyMMdd") + ".xls");
        //Response.ContentEncoding = System.Text.Encoding.UTF8;
        //Response.ContentType = "application/vnd.ms-excel";
        
        ////Response.Write(tableString);
        ////Response.End();




        //System.IO.StringWriter oStringWriter = new System.IO.StringWriter();
        //System.Web.UI.HtmlTextWriter oHtmlTextWriter = new System.Web.UI.HtmlTextWriter(oStringWriter);

        //oHtmlTextWriter.Write(tableString);
        //Response.Output.Write(oStringWriter.ToString());

        //Response.Flush();
        //Response.End();


    }
   

}