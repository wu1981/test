using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Moldau.TPP;
using Moldau.Common;
using Moldau.Page;
using System.IO;

public partial class TPP_Tpp_refer : Moldau.TPP.TPPPage
{
  
    //分數顯示TeamlistBox
    protected List<CheckBox> teamListBoxLists = new List<CheckBox>();

    //Project 顯示check listbox


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


        for(int i =0 ;i<teamListBoxLists.Count;i++)
        {
            teamListBoxLists[i].Visible = false;
            
        }

        string[] allTeam = str.GetAllTeamName();

        for (int i = 0; i < allTeam.Length; i++)
        {
            teamListBoxLists[i].Visible = true;
            teamListBoxLists[i].Text = allTeam[i];
        }


        


        if (!IsPostBack)
        {
            for (int i = 0; i < allTeam.Length; i++)
            {
                teamListBoxLists[i].Checked = false;
                
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

            
        }


    }


    protected void Button1_Click(object sender, EventArgs e)
    {
        string startString = "";
        string tmpString1 = "";
        string tmpString2 = "";
        string tmpString3 = "";

        //統計開始和結束時間
        DateTime sumStartDate = PageCommon.StrToDateTime(startDateTextBox.Text, DateTime.Now);
        DateTime sumEndDate = PageCommon.StrToDateTime(endDateTextBox.Text, DateTime.Now);

        if (startDateTextBox.Text == "" || endDateTextBox.Text == "" || DateTime.Compare(sumStartDate,sumEndDate) >0)
        {
            Response.Write(JSCode.JSBack("時間錯誤"));
            return;
        }

        
        List<TestTPP>  tmpdisplayTppList;

        //選出所有符合這個時間段的Project

        if (!ByProjectCheckBox.Checked)
        {
            tmpdisplayTppList = TestTPP.GetTestTPPList(sumStartDate, sumEndDate);
        }
        else
        {
            tmpdisplayTppList = TestTPP.GetTestTPPList(sumStartDate, sumEndDate,1);
        }



        //去掉沒有選中的組別的Project
        List<string> projectdisplayTeamString = new List<string>();

        int checkedFlag = 0;

        for (int i = 0; i < teamListBoxLists.Count; i++)
        {
            if(teamListBoxLists[i].Checked)
            {
                projectdisplayTeamString.Add(teamListBoxLists[i].Text);
                checkedFlag++;
            }
        }

        if (checkedFlag != 1)
        {
            Response.Write(JSCode.JSBack("只能選擇一個組別"));
            return;
        }

       

        List<string> allSelectedUser = new List<string>();

        //找出所有選中組別的成員
        for (int i = 0; i < projectdisplayTeamString.Count; i++)
        {
            if (!allSelectedUser.Contains(str.GetTeamManager(projectdisplayTeamString[i])))
            {
                allSelectedUser.Add(str.GetTeamManager(projectdisplayTeamString[i]));
            }
            for (int j = 0; j < str.GetTeamUser(projectdisplayTeamString[i], 1).Length; j++)
            {
                allSelectedUser.Add(str.GetTeamUser(projectdisplayTeamString[i], 1)[j]);
            }
           
        }

        //
        for (int i = 0; i < tmpdisplayTppList.Count; i++)
        {
            for (int j = 0; j < allSelectedUser.Count; j++)
            {
                if (tmpdisplayTppList[i].TestUserStr.Contains(allSelectedUser[j]))
                {
                    displayTppList.Add(tmpdisplayTppList[i]);
                    break;
                }
            }
        }



        //找出需要顯示的用戶名,組別

        //找出組別
        List<string> displayTeamString = new List<string>();
        List<string> displayUserString = new List<string>();
        
        decimal[] displayUserTotalTpp;

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

            //所有組員名字
            for (int j = 0; j < str.GetTeamUser(displayTeamString[i], 0).Length; j++)
            {
                tmpString1 += "<td style=\"witdh:65px\">" + str.GetTeamUser(displayTeamString[i], 0)[j] + "</td>";
                //初始化顯示組別
                displayUserString.Add(str.GetTeamUser(displayTeamString[i], 0)[j]);
            }

        }

        //開始字符串
        startString = "<table id=\"FormTable\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\" class=\"ContentTable_NoPadding\"  style=\"width:" +
                             (605 + allSelectedUser.Count * 100).ToString() +
                            "px; text-align: center; margin-top:10px; \">" +
                            "<tr class=\"GridColHeader\" >" +
                            "<td rowspan=\"2\"  width=\"55px\">WeekLy</td> " +
                            "<td  rowspan=\"2\" width=\"60px\"> Project</td>" +
                            "<td  rowspan=\"2\" width=\"80px\">Leader</td>" +
                            "<td  rowspan=\"2\" width=\"80px\">Test Type</td>" +
                            "<td  rowspan=\"2\" width=\"250px\">Test Item</td>" +
                            "<td  rowspan=\"2\" width=\"65px\">Start Date</td> " +
                            "<td rowspan=\"2\" width=\"60px\">End Date</td>" +
                            "<td rowspan=\"2\" width=\"35px\"> Total TPP</td>";
                            

        tableString = startString + tableString + 
                            "<td rowspan=\"2\" width=\"60px\"> 組內TPP總和</td>" +
                            "<td  width=\"80px\" style=\"background:yellow;\">Manager TPP</td>" 
                             + "</tr>";
        tableString += "<tr>" + tmpString1 + "<td>" + allSelectedUser[0] + "</td>" + "</tr>";

        //初始化displayUserTotalTpp
        displayUserTotalTpp = new decimal[displayUserString.Count];
        

        //初始化每個測試項目組內人員的TPP

        decimal[] eachItemTeamInTpp = new decimal[displayTppList.Count];

        //初始化Manager TPP
        decimal[] eachItemManagerTpp = new decimal[displayTppList.Count];

        
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
            tableString += "<td>" + tmpRate.TPPRateName + "</td>";
            tableString += "<td>" + displayTppList[i].ItemName + "</td>";

            if (DateTime.Compare(displayTppList[i].StartDate, sumStartDate) >= 0 && DateTime.Compare(displayTppList[i].EndDate, sumEndDate) <= 0)
            {
                tableString += "<td>" + displayTppList[i].StartDate.ToShortDateString() + "</td>";
                tableString += "<td>" + displayTppList[i].EndDate.ToShortDateString() + "</td>";
                tableString += "<td>" + displayTppList[i].TotalTPP.ToString() + "</td>";
            }
            else
            {
                tableString += "<td style=\"background:#C0504D\">" + displayTppList[i].StartDate.ToShortDateString() + "</td>";
                tableString += "<td style=\"background:#C0504D\">" + displayTppList[i].EndDate.ToShortDateString() + "</td>";
                tableString += "<td style=\"background:#C0504D\">" + displayTppList[i].TotalTPP.ToString() + "<br/>" + TPPTool.SplitSumTpp(displayTppList[i].StartDate, displayTppList[i].EndDate, sumStartDate, sumEndDate, (decimal)displayTppList[i].TotalTPP).ToString() +"</td>";
            }


            

            //顯示每個用戶的TPP點數
            
            //該行是否包含主管
            bool hasManger = false;

            for (int j = 0; j < displayUserString.Count; j++)
            {
                decimal currentTTP = (decimal)0;
                bool hasThisUser = false;



                //校驗是否有主管的TPP
                for (int k = 0; k < displayTppList[i].PersonalTPPList.Count; k++)
                {
                    
                    if (displayTppList[i].PersonalTPPList[k].UserName == allSelectedUser[0])
                    {
                        hasManger = true;
                        currentTTP = TPPTool.SplitSumTpp(displayTppList[i].StartDate, displayTppList[i].EndDate, sumStartDate, sumEndDate, displayTppList[i].PersonalTPPList[k].TPPCount);
                        eachItemManagerTpp[i] = currentTTP;
                        break;
                    }
                }


                
                for (int k = 0; k < displayTppList[i].PersonalTPPList.Count; k++)
                {

                    if (displayTppList[i].PersonalTPPList[k].UserName == displayUserString[j])
                    {
                        hasThisUser = true;
                        currentTTP = TPPTool.SplitSumTpp(displayTppList[i].StartDate, displayTppList[i].EndDate, sumStartDate, sumEndDate, displayTppList[i].PersonalTPPList[k].TPPCount);
                        displayUserTotalTpp[j] += currentTTP;

                        break;
                        
                    }


                }

                if (hasThisUser)
                {
                    tableString += "<td>" + currentTTP.ToString() + "</td>";
                    eachItemTeamInTpp[i] += currentTTP;
                }
                else
                {
                    tableString += "<td></td>";
                }
  
            }



            //該項目的總TPP
            tableString += "<td  >" + eachItemTeamInTpp[i].ToString() + "</td>";
            //該項目主管的TPP
            tableString += "<td style=\"background:#555\" >" + eachItemManagerTpp[i].ToString() + "</td>";


            tableString += "</tr>";
            
        }

        //重寫一遍User列表

        tableString += "<tr style=\"background:#99FF99\"><td colspan=\"8\">User Message</td>" + tmpString1 + "</tr>";


        //個人總的TPP點數
        tableString += "<tr style=\"background:#99FF99;font-weight:bold\"><td colspan=\"8\">Personal Total TPP</td>";

        for (int i = 0; i < displayUserTotalTpp.Length; i++)
        {
            tableString += "<td>" + displayUserTotalTpp[i].ToString() + "</td>";
            
        }
        tableString += "</tr>";

        //Team總的TPP點數
        tableString += "<tr style=\"background:#FFCC00;font-weight:bold\"><td colspan=\"8\">Team Total TPP</td>";

        
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

        //Section的總點數,該地方顯示每個Section的總點數,其中Section包括主管的測試點數,
        



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
                    "<link href=\"http://atsz-nbqtc-serv/tpp/CSS/Main.css\" media=\"all\" rel=\"stylesheet\" type=\"text/css\" />" + System.Environment.NewLine +
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