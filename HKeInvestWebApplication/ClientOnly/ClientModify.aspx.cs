﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using HKeInvestWebApplication.Code_File;
using HKeInvestWebApplication.ExternalSystems.Code_File;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Text;


namespace HKeInvestWebApplication.ClientOnly
{
    public partial class ClientModify : System.Web.UI.Page
    {
        HKeInvestData myHKeInvestData = new HKeInvestData();
        HKeInvestCode myHKeInvestCode = new HKeInvestCode();
        ExternalFunctions myExternalFunctions = new ExternalFunctions();
        static string accountNumber;
        int i = 1;
        static string last2;

        protected void Page_Load(object sender, EventArgs e)
        {
            //get the account number of the current logged in user
            string username = User.Identity.Name;
            string sql = "select accountNumber from LoginAccount where username ='" + username + "'";

            DataTable dtclient = myHKeInvestData.getData(sql);
            if (dtclient == null) { return; } // if the dataset is null, a sql error occurred.

            foreach (DataRow row in dtclient.Rows)
            {
                accountNumber = (string)row["accountNumber"];
            }
            lblAccountNumber.Text = "account number: " + accountNumber;
            lblAccountNumber.Visible = true;

            string userName = User.Identity.Name;
            sql = "SELECT lastName, firstName FROM Client WHERE accountNumber = (SELECT accountNumber FROM LoginAccount WHERE userName ='" + userName + "')"; // Complete the SQL statement.

            DataTable dtClient = myHKeInvestData.getData(sql);
            if (dtClient == null) { return; } // If the DataSet is null, a SQL error occurred.

            // If no result is returned by the SQL statement, then display a message.
            if (dtClient.Rows.Count == 0)
            {
                lblResultMessage.Text = "No such account number.";
                lblResultMessage.Visible = true;
                lblClientName.Visible = false;
                return;
            }

            // Show the client name(s) on the web page.
            string clientName = "Client(s): ";
            foreach (DataRow row in dtClient.Rows)
            {
                clientName = clientName + row["lastName"] + ", " + row["firstName"];
                if (dtClient.Rows.Count != 1 && dtClient.Rows.Count == i)
                {
                    last2 = last2 + row["lastName"];
                }
                if (dtClient.Rows.Count != i)
                {
                    clientName = clientName + "and ";
                }
                i = i + 1;
            }
            lblClientName.Text = clientName;
            lblClientName.Visible = true;
        }

        protected void cvOccupation_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (EmpStatus.SelectedValue == "Employed")
            {
                if (args.Value == "") args.IsValid = false;
            }
        }
        protected void cvOccupation2_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (EmpStatus2.SelectedValue == "Employed")
            {
                if (args.Value == "") args.IsValid = false;
            }
        }

        protected void cvSpecificSource_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (PrimarySource.SelectedValue == "other" && args.Value == "") args.IsValid = false;
        }

        protected void Register_Click(object sender, EventArgs e)
        {
            string sql;
            if (Page.IsValid)
            {
                HKeInvestData myHKeInvest = new HKeInvestData();
                SqlTransaction myTransaction = myHKeInvest.beginTransaction();
                string acNo = accountNumber;

                if (Email.Text != null)
                {
                    sql = "UPDATE [Client] SET email = '" + Email.Text + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (Building.Text != null)
                {
                    sql = "UPDATE [Client] SET building = '" + Building.Text + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (Street.Text != null)
                {
                    sql = "UPDATE [Client] SET street = '" + Street.Text + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (District.Text != null)
                {
                    sql = "UPDATE [Client] SET district = '" + District.Text + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (EmpStatus.SelectedValue != null)
                {
                    sql = "UPDATE [Client] SET employmentStatus = '" + EmpStatus.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (EmpByBroker.SelectedValue != null)
                {
                    sql = "UPDATE [Client] SET employByBroker = '" + EmpByBroker.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (CompanyDirector.SelectedValue != null)
                {
                    sql = "UPDATE [Client] SET publiclyTradedCompany = '" + CompanyDirector.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (PrimarySource.SelectedValue != null)
                {
                    sql = "UPDATE [Client] SET primarySourceFund = '" + PrimarySource.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (Objective.SelectedValue != null)
                {
                    sql = "UPDATE [Client] SET investObjective = '" + Objective.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (Knowledge.SelectedValue != null)
                {
                    sql = "UPDATE [Client] SET investKnowledge = '" + Knowledge.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (Experience.SelectedValue != null)
                {
                    sql = "UPDATE [Client] SET investExperience = '" + Experience.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (Income.SelectedValue != null)
                {
                    sql = "UPDATE [Client] SET annualIncome = '" + Income.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (NetWorth.SelectedValue != null)
                {
                    sql = "UPDATE [Client] SET liquidNetWorth = '" + NetWorth.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                if (Fund.Checked)
                {
                    sql = "UPDATE [Client] SET freeBalanceToFund = '" + Fund.Checked + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                //insert phone numbers
                if (hPhone.Text != "")
                {
                    sql = "UPDATE [Client] SET homePhone = '" + hPhone.Text + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }
                if (hFax.Text != "")
                {
                    sql = "UPDATE [Client] SET homeFax = '" + hFax.Text + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }
                if (bPhone.Text != "")
                {
                    sql = "UPDATE [Client] SET businessPhone = '" + bPhone.Text + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }
                if (bFax.Text != "")
                {
                    sql = "UPDATE [Client] SET businessFax = '" + bFax.Text + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                //insert employmeny information
                if (EmpStatus.SelectedValue == "Employed")
                {
                    sql = "UPDATE [Client] SET occupation = '" + Occupation.Text + "', yearsWithEmployer = '" + yrWithEmp.Text + "', employerName = '" + Employer.Text + "', employerPhone = '" + EmployerPhone.Text + "', businessNature = '" + Business.Text + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                //insert specific primary source fof fund
                if (PrimarySource.SelectedValue == "other")
                {
                    sql = "UPDATE [Client] SET specificSource = '" + SpecificSource.Text + "' WHERE accountNumber = '" + acNo + "'";
                    myHKeInvest.setData(sql, myTransaction);
                }

                //end of primary account holder infomation
                myHKeInvest.commitTransaction(myTransaction);


                //INSERT CO HOLDER'S INFORMATION
                if (i != 0)
                {
                    CoHolderPanel.Visible = true;
                    myTransaction = myHKeInvest.beginTransaction();
                    ////get co-ac holder's clientNumber
                    //sql = "SELECT clientNumber from Client WHERE accountNumber = '" + acNo + "' AND lastName = '" + last2 + "'";
                    //DataTable dtClient = myHKeInvest.getData(sql);
                    //string cNo = "";
                    //foreach (DataRow row in dtClient.Rows)
                    //{
                    //    cNo = Convert.ToString(row["clientNumber"]);
                    //}

                    if (Email2.Text != null)
                    {
                        sql = "UPDATE [Client] SET email = '" + Email2.Text + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }

                    if (Building2.Text != null)
                    {
                        sql = "UPDATE [Client] SET building = '" + Building2.Text + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }

                    if (Street2.Text != null)
                    {
                        sql = "UPDATE [Client] SET street = '" + Street2.Text + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }

                    if (District2.Text != null)
                    {
                        sql = "UPDATE [Client] SET district = '" + District2.Text + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }

                    if (EmpStatus2.SelectedValue != null)
                    {
                        sql = "UPDATE [Client] SET employmentStatus = '" + EmpStatus2.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }

                    if (EmpByBroker2.SelectedValue != null)
                    {
                        sql = "UPDATE [Client] SET employByBroker = '" + EmpByBroker2.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }

                    if (CompanyDirector2.SelectedValue != null)
                    {
                        sql = "UPDATE [Client] SET publiclyTradedCompany = '" + CompanyDirector2.SelectedValue + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }

                    // myHKeInvest.setData(sql, myTransaction);
                    // myHKeInvest.commitTransaction(myTransaction);       //need to commit transaction before being able to retreive information from the database
                    // myTransaction = myHKeInvest.beginTransaction();

                    //insert phone numbers
                    if (hPhone2.Text != "")
                    {
                        sql = "UPDATE [Client] SET homePhone = '" + hPhone2.Text + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }
                    if (hFax2.Text != "")
                    {
                        sql = "UPDATE [Client] SET homeFax = '" + hFax2.Text + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }
                    if (bPhone2.Text != "")
                    {
                        sql = "UPDATE [Client] SET businessPhone = '" + bPhone2.Text + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }
                    if (bFax2.Text != "")
                    {
                        sql = "UPDATE [Client] SET businessFax = '" + bFax2.Text + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }

                    //insert employmeny information
                    if (EmpStatus2.SelectedValue == "Employed")
                    {
                        sql = "UPDATE [Client] SET occupation = '" + Occupation2.Text + "', yearsWithEmployer = '" + yrWithEmp2.Text + "', employerName = '" + Employer2.Text + "', employerPhone = '" + EmployerPhone2.Text + "', businessNature = '" + Business2.Text + "' WHERE accountNumber = '" + acNo + "'";
                        myHKeInvest.setData(sql, myTransaction);
                    }

                    //END of optional fields for co-ac holder
                    myHKeInvest.commitTransaction(myTransaction);
                }

                lblmsg.Visible = true;
                lblmsg.Text = "Account info updated successfully!";
                return;
            }
            lblmsg.Visible = false;
        }
    }
}