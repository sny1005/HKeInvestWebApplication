﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using HKeInvestWebApplication.Code_File;
using HKeInvestWebApplication.ExternalSystems.Code_File;

namespace HKeInvestWebApplication.ClientOnly
{
    public partial class ClientSecurityHoldingsDetails : System.Web.UI.Page
    {
        HKeInvestData myHKeInvestData = new HKeInvestData();
        HKeInvestCode myHKeInvestCode = new HKeInvestCode();
        ExternalFunctions myExternalFunctions = new ExternalFunctions();
        static string accountNumber;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Session["CurrencyData"] == null)
                {
                    DataTable CurrencyTable = myHKeInvestCode.CurrencyData();
                    string[,] CurrencyData = new string[CurrencyTable.Columns.Count, CurrencyTable.Rows.Count];

                    int i = 0;
                    foreach (DataRow row in CurrencyTable.Rows)
                    {
                        CurrencyData[0, i] = Convert.ToString(row["currency"]);
                        CurrencyData[1, i] = Convert.ToString(row["rate"]);
                        i++;
                    }

                    Session.Add("CurrencyData", CurrencyData);
                }

                string[,] currencies = (string[,])Session["CurrencyData"];
                for (int j = 0; j < currencies.GetLength(1); j++)
                {
                    ddlCurrency.Items.Add(currencies[0, j]);
                }

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

                // load current date into date range for 6d
                string today = DateTime.Today.ToString("dd/MM/yyyy");
                startDate.Text = today;
                endDate.Text = today;
            }
        }

        protected void ddlSecurityType_SelectedIndexChanged()
        {
            // Reset visbility of controls and initialize values.
            lblResultMessage.Visible = false;
            ddlCurrency.Visible = false;
            gvSecurityHolding.Visible = false;
            ddlCurrency.SelectedIndex = 0;
            string sql = "";

            // *******************************************************************
            // Set the account number and security type from the web page. *
            // *******************************************************************
            string securityType = ddlSecurityType.SelectedValue; // Set the securityType from a web form control!

            // No action when the first item in the DropDownList is selected.
            if (securityType == "0") { return; }

            // *****************************************************************************************
            // Construct the SQL statement to retrieve the first and last name of the client(s). *
            // *****************************************************************************************
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
                gvSecurityHolding.Visible = false;
                return;
            }

            // Show the client name(s) on the web page.
            string clientName = "Client(s): ";
            int i = 1;
            foreach (DataRow row in dtClient.Rows)
            {
                clientName = clientName + row["lastName"] + ", " + row["firstName"];
                if (dtClient.Rows.Count != i)
                {
                    clientName = clientName + "and ";
                }
                i = i + 1;
            }
            lblClientName.Text = clientName;
            lblClientName.Visible = true;

            // *****************************************************************************************************************************
            //       Construct the SQL select statement to get the code, name, shares and base of the security holdings of a specific type *
            //       in an account. The select statement should also return three additonal columns -- price, value and convertedValue --  *
            //       whose values are not actually in the database, but are set to the constant 0.00 by the select statement. (HINT: see   *
            //       http://stackoverflow.com/questions/2504163/include-in-select-a-column-that-isnt-actually-in-the-database.)            *   
            // *****************************************************************************************************************************
            sql = "SELECT code, name, shares, base, '0.00' as price, '0.00' AS value, '0.00' AS convertedValue FROM dbo.SecurityHolding WHERE accountNumber= (SELECT accountNumber FROM LoginAccount WHERE userName ='" + userName + "') AND type='" + securityType + "'"; // Complete the SQL statement.

            DataTable dtSecurityHolding = myHKeInvestData.getData(sql);
            if (dtSecurityHolding == null) { return; } // If the DataSet is null, a SQL error occurred.

            // If no result is returned, then display a message that the account does not hold this type of security.
            if (dtSecurityHolding.Rows.Count == 0)
            {
                lblResultMessage.Text = "No " + securityType + "s held in this account.";
                lblResultMessage.Visible = true;
                gvSecurityHolding.Visible = false;
                return;
            }

            // For each security in the result, get its current price from an external system, calculate the total value
            // of the security and change the current price and total value columns of the security in the result.
            int dtRow = 0;
            foreach (DataRow row in dtSecurityHolding.Rows)
            {
                string securityCode = row["code"].ToString();
                decimal shares = Convert.ToDecimal(row["shares"]);
                decimal price = myExternalFunctions.getSecuritiesPrice(securityType, securityCode);
                decimal value = Math.Round(shares * price - (decimal).005, 2);
                dtSecurityHolding.Rows[dtRow]["price"] = price;
                dtSecurityHolding.Rows[dtRow]["value"] = value;
                dtRow = dtRow + 1;
            }

            // Set the initial sort expression and sort direction for sorting the GridView in ViewState.
            ViewState["SortExpression"] = "name";
            ViewState["SortDirection"] = "ASC";

            // Bind the GridView to the DataTable.
            gvSecurityHolding.DataSource = dtSecurityHolding;
            gvSecurityHolding.DataBind();

            // Set the visibility of controls and GridView data.
            gvSecurityHolding.Visible = true;
            ddlCurrency.Visible = true;
            gvSecurityHolding.Columns[myHKeInvestCode.getColumnIndexByName(gvSecurityHolding, "convertedValue")].Visible = false;
        }

        protected void ddlCurrency_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the index value of the convertedValue column in the GridView using the helper method "getColumnIndexByName".
            int convertedValueIndex = myHKeInvestCode.getColumnIndexByName(gvSecurityHolding, "convertedValue");

            // Get the currency to convert to from the ddlCurrency dropdownlist.
            // Hide the converted currency column if no currency is selected.
            string toCurrency = ddlCurrency.SelectedValue;
            if (toCurrency == "0")
            {
                gvSecurityHolding.Columns[convertedValueIndex].Visible = false;
                return;
            }

            // Make the convertedValue column visible and create a DataTable from the GridView.
            // Since a GridView cannot be updated directly, it is first loaded into a DataTable using the helper method 'unloadGridView'.
            gvSecurityHolding.Columns[convertedValueIndex].Visible = true;
            DataTable dtSecurityHolding = myHKeInvestCode.unloadGridView(gvSecurityHolding);

            // ***********************************************************************************************************
            //       For each row in the DataTable, get the base currency of the security, convert the current value to  *
            //       the selected currency and assign the converted value to the convertedValue column in the DataTable. *
            // ***********************************************************************************************************
            int toCurrencyIndex = ddlCurrency.SelectedIndex - 1;
            string[,] currency = (string[,])Session["CurrencyData"];

            foreach (DataRow row in dtSecurityHolding.Rows)
            {
                // Add your code here!
                string fromRate = myHKeInvestCode.findCurrencyRate(currency, (string)row["base"]);
                row["convertedValue"] = myHKeInvestCode.convertCurrency((string)row["base"], fromRate, toCurrency, currency[1, toCurrencyIndex], (decimal)row["value"]);
            }

            // Change the header text of the convertedValue column to indicate the currency. 
            gvSecurityHolding.Columns[convertedValueIndex].HeaderText = "Value in " + toCurrency;

            // Bind the DataTable to the GridView.
            gvSecurityHolding.DataSource = dtSecurityHolding;
            gvSecurityHolding.DataBind();
        }

        protected void gvSecurityHolding_Sorting(object sender, GridViewSortEventArgs e)
        {
            // Since a GridView cannot be sorted directly, it is first loaded into a DataTable using the helper method 'unloadGridView'.
            // Create a DataTable from the GridView.
            DataTable dtSecurityHolding = myHKeInvestCode.unloadGridView(gvSecurityHolding);

            // Set the sort expression in ViewState for correct toggling of sort direction,
            // Sort the DataTable and bind it to the GridView.
            string sortExpression = e.SortExpression.ToLower();
            ViewState["SortExpression"] = sortExpression;
            dtSecurityHolding.DefaultView.Sort = sortExpression + " " + myHKeInvestCode.getSortDirection(ViewState, e.SortExpression);
            dtSecurityHolding.AcceptChanges();

            // Bind the DataTable to the GridView.
            gvSecurityHolding.DataSource = dtSecurityHolding.DefaultView;
            gvSecurityHolding.DataBind();
        }

        protected void gvActive_Sorting(object sender, GridViewSortEventArgs e)
        {
            // Since a GridView cannot be sorted directly, it is first loaded into a DataTable using the helper method 'unloadGridView'.
            // Create a DataTable from the GridView.
            GridView gv = (GridView)sender;
            DataTable dtActive = myHKeInvestCode.unloadGridView(gv);

            // Set the sort expression in ViewState for correct toggling of sort direction,
            // Sort the DataTable and bind it to the GridView.
            string sortExpression = e.SortExpression.ToLower();
            ViewState["SortExpression"] = sortExpression;
            dtActive.DefaultView.Sort = sortExpression + " " + myHKeInvestCode.getSortDirection(ViewState, e.SortExpression);
            dtActive.AcceptChanges();

            // Bind the DataTable to the GridView.
            gv.DataSource = dtActive.DefaultView;
            gv.DataBind();
        }

        protected void gvHistory_Sorting(object sender, GridViewSortEventArgs e)
        {
            // Since a GridView cannot be sorted directly, it is first loaded into a DataTable using the helper method 'unloadGridView'.
            // Create a DataTable from the GridView.
            DataTable dtHistory = myHKeInvestCode.unloadGridView(gvHistory);

            // Set the sort expression in ViewState for correct toggling of sort direction,
            // Sort the DataTable and bind it to the GridView.
            string sortExpression = e.SortExpression.ToLower();
            ViewState["SortExpression"] = sortExpression;
            dtHistory.DefaultView.Sort = sortExpression + " " + myHKeInvestCode.getSortDirection(ViewState, e.SortExpression);
            dtHistory.AcceptChanges();

            // Bind the DataTable to the GridView.
            gvHistory.DataSource = dtHistory.DefaultView;
            gvHistory.DataBind();
        }

        protected void generate6a_Click()
        {
            // retrieve account free balance
            string sql = "SELECT [balance] FROM [LoginAccount] WHERE [accountNumber] = '" + accountNumber + "'";
            DataTable dtBalance = myHKeInvestData.getData(sql);

            if (dtBalance == null || dtBalance.Rows.Count == 0)
                return;

            DataRow[] record = dtBalance.Select();
            string freeBalance = record[0]["balance"].ToString().Trim();
            lblFreeBalance.Text = "The account free balance is: " + freeBalance;
            lblFreeBalance.Visible = true;

            // calculate total monetary value of securities held by the account
            sql = "SELECT [shares], [base], [type], [code] FROM [SecurityHolding] WHERE [accountNumber] = '" + accountNumber + "'";
            DataTable dtHolding = myHKeInvestData.getData(sql);

            if (dtHolding == null || dtHolding.Rows.Count == 0)
                return;

            decimal totalValue = 0, bondValue = 0, stockValue = 0, trustValue = 0;
            foreach (DataRow row in dtHolding.Rows)
            {
                decimal rate = myExternalFunctions.getCurrencyRate(row["base"].ToString().Trim());
                decimal price = myExternalFunctions.getSecuritiesPrice(row["type"].ToString().Trim(), row["code"].ToString().Trim());
                decimal value = rate * Convert.ToDecimal(row["shares"]) * price;

                if (row["type"].ToString().Trim() == "bond")
                    bondValue += value;
                else if (row["type"].ToString().Trim() == "stock")
                    stockValue += value;
                else if (row["type"].ToString().Trim() == "unit trust")
                    trustValue += value;

                totalValue += value;
            }
            lblTotalValue.Text = "The total monetary value of securities held by the account is: " + totalValue.ToString();
            lblTotalValue.Visible = true;

            // prepare datatable for securit summary
            DataTable dtSummary = new DataTable();
            dtSummary.Columns.Add("type");
            dtSummary.Columns.Add("totalValue");
            dtSummary.Columns.Add("lastDate", typeof(DateTime));
            dtSummary.Columns.Add("lastDollar");

            // data retrieved above
            string[] types = { "Bond", "Stock", "Unit Trust" };
            decimal[] values = { bondValue, stockValue, trustValue };
            DateTime[] dates = new DateTime[3];
            decimal[] dollars = new decimal[3];

            // retreive data
            for (int k=0; k<3; k++)
            {
                sql = "SELECT MAX(orderNumber) FROM [Order] WHERE [accountNumber] = '" + accountNumber + "' AND [securityType] = '" + types[k] + "'";
                sql = "SELECT [orderNumber], [dateSubmitted], [securityType], [securityCode], [buyOrSell] FROM [Order] WHERE [orderNumber] = (" + sql + ")";
                DataTable dt = myHKeInvestData.getData(sql);
                if (dt == null || dt.Rows.Count == 0)
                    return;

                // store the date value
                DataRow[] dr = dt.Select();
                dates[k] = Convert.ToDateTime(dr[0]["dateSubmitted"]);

                // get dollar amount
                if(dr[0]["securityType"].ToString().Trim() == "stock")
                {
                    decimal price = myExternalFunctions.getSecuritiesPrice("stock", dr[0]["securityCode"].ToString().Trim());

                    sql = "SELECT [shares] FROM [StockOrder] WHERE [orderNumber] = '" + dr[0]["orderNumber"].ToString().Trim() + "'";
                    DataTable dtShares = myHKeInvestData.getData(sql);
                    if (dtShares == null || dtShares.Rows.Count == 0)
                        return;
                    DataRow[] drShares = dtShares.Select();

                    dollars[k] = Convert.ToDecimal(drShares[0]["shares"]) * price;
                }
                //bond or trust
                else if(dr[0]["buyOrSell"].ToString().Trim() == "buy")
                {
                    sql = "SELECT [amount] FROM [BuyBondOrder] WHERE [orderNumber] = '" + dr[0]["orderNumber"].ToString().Trim() + "'";
                    DataTable dtAmount = myHKeInvestData.getData(sql);
                    if (dtAmount == null || dtAmount.Rows.Count == 0)
                        return;
                    DataRow[] drAmount = dtAmount.Select();

                    dollars[k] = Convert.ToDecimal(drAmount[0]["amount"]);
                }
                // selling bond or trust
                else
                {
                    string name, currency;      //name is dummy variable
                    myHKeInvestCode.getSecurityNameBase("unit trust", dr[0]["securityCode"].ToString().Trim(), out name, out currency);
                    decimal rate = myExternalFunctions.getCurrencyRate(currency);
                    decimal price = myExternalFunctions.getSecuritiesPrice("unit trust", dr[0]["securityCode"].ToString().Trim());

                    sql = "SELECT [shares] FROM [SellBondOrder] WHERE [orderNumber] = '" + dr[0]["orderNumber"].ToString().Trim() + "'";
                    DataTable dtShares = myHKeInvestData.getData(sql);
                    if (dtShares == null || dtShares.Rows.Count == 0)
                        return;
                    DataRow[] drShares = dtShares.Select();

                    dollars[k] = Convert.ToDecimal(drShares[0]["shares"]) * price * rate;
                }


                DataRow row = dtSummary.NewRow();
                row["type"] = types[k];
                row["totalValue"] = values[k];
                row["lastDate"] = dates[k];
                row["lastDollar"] = dollars[k];

                dtSummary.Rows.Add(row);
            }
            dtSummary.AcceptChanges();

            gvSecuritySummary.DataSource = dtSummary;
            gvSecuritySummary.DataBind();
            gvSecuritySummary.Visible = true;
        }

        protected void generate6c_Click()
        {
            // REQUIREMENT 6C IMPLEMENTATION
            // Bond/Unit trust listings
            lblActiveBond.Visible = true;
            string sql = "SELECT [Order].*, [BuyBondOrder].amount FROM [Order] INNER JOIN [BuyBondOrder] ON [Order].[orderNumber] = [BuyBondOrder].[orderNumber] WHERE [status] <> 'completed' AND [accountNumber] = '" + accountNumber + "'";
            DataTable activeBondOrder = myHKeInvestData.getData(sql);
            sql = "SELECT [Order].*, [SellBondOrder].shares AS amount FROM [Order] INNER JOIN [SellBondOrder] ON [Order].[orderNumber] = [SellBondOrder].[orderNumber] WHERE [status] <> 'completed' AND [accountNumber] = '" + accountNumber + "'";
            activeBondOrder.Merge(myHKeInvestData.getData(sql));
            activeBondOrder.Columns.Add("name");

            if (activeBondOrder.Rows.Count == 0)
            {
                lblActiveBond.Visible = false;
                gvActiveBond.Visible = false;
                return;
            }
                

            string name, baseCurrency;          // baseCurrency is just a dummy variable
            foreach (DataRow row in activeBondOrder.Rows)
            {
                if (Convert.ToString(row["securityType"]).Trim() == "bond")
                    myHKeInvestCode.getSecurityNameBase("bond", (string)row["securityCode"], out name, out baseCurrency);
                else
                    myHKeInvestCode.getSecurityNameBase("unit trust", (string)row["securityCode"], out name, out baseCurrency);
                row["name"] = name;
            }

            activeBondOrder.AcceptChanges();

            // Set the initial sort expression and sort direction for sorting the GridView in ViewState.
            ViewState["SortExpression"] = "datesubmitted";
            ViewState["SortDirection"] = "DESC";

            gvActiveBond.DataSource = activeBondOrder;
            gvActiveBond.DataBind();
            gvActiveBond.Sort("datesubmitted", SortDirection.Descending);
            gvActiveBond.Visible = true;

            // Stock listings
            lblActiveStock.Visible = true;
            sql = "SELECT [Order].*, [StockOrder].shares, [StockOrder].orderType, [StockOrder].expiaryDay, [StockOrder].limitPrice, [StockOrder].stopPrice FROM [Order] INNER JOIN [StockOrder] ON [Order].[orderNumber] = [StockOrder].[orderNumber] WHERE [status] <> 'completed' AND [status] <> 'cancelled' AND [accountNumber] = '" + accountNumber + "'";
            DataTable activeStockOrder = myHKeInvestData.getData(sql);
            activeStockOrder.Columns.Add("name");

            if (activeStockOrder.Rows.Count == 0)
            {
                lblActiveStock.Visible = false;
                gvActiveStock.Visible = false;
                return;
            }

            foreach (DataRow row in activeStockOrder.Rows)
            {
                if (Convert.ToString(row["limitPrice"]).Trim() == "")
                    row["limitPrice"] = 0.00;
                if (Convert.ToString(row["stopPrice"]).Trim() == "")
                    row["stopPrice"] = 0.00;

                myHKeInvestCode.getSecurityNameBase("stock", (string)row["securityCode"], out name, out baseCurrency);
                row["name"] = name;
            }

            activeStockOrder.AcceptChanges();

            gvActiveStock.DataSource = activeStockOrder;
            gvActiveStock.DataBind();
            gvActiveStock.Visible = true;
        }

        protected void generate6d_Click()
        {
            // REQUIREMENT 6D IMPLEMENTATION
            string start = startDate.Text;
            string end = endDate.Text;

            if (start == "")
                start = "01/01/0001";
            if (end == "")
                end = "31/12/9999";

            string sql = "SELECT [Order].*, [BuyBondOrder].amount FROM [Order] INNER JOIN [BuyBondOrder] ON [Order].[orderNumber] = [BuyBondOrder].[orderNumber] WHERE [accountNumber] = '" + accountNumber + "' AND CAST([dateSubmitted] AS DATE) BETWEEN CONVERT(DATE, '" + start + "', 103) AND CONVERT(DATE, '" + end + "', 103)";
            DataTable orderHistory = myHKeInvestData.getData(sql);
            sql = "SELECT [Order].*, [SellBondOrder].shares AS amount FROM [Order] INNER JOIN [SellBondOrder] ON [Order].[orderNumber] = [SellBondOrder].[orderNumber] WHERE [accountNumber] = '" + accountNumber + "' AND CAST([dateSubmitted] AS DATE) BETWEEN CONVERT(DATE, '" + start + "', 103) AND CONVERT(DATE, '" + end + "', 103)";
            orderHistory.Merge(myHKeInvestData.getData(sql));
            sql = "SELECT [Order].*, [StockOrder].shares AS amount FROM [Order] INNER JOIN [StockOrder] ON [Order].[orderNumber] = [StockOrder].[orderNumber] WHERE [accountNumber] = '" + accountNumber + "' AND CAST([dateSubmitted] AS DATE) BETWEEN CONVERT(DATE, '" + start + "', 103) AND CONVERT(DATE, '" + end + "', 103)";
            orderHistory.Merge(myHKeInvestData.getData(sql));
            orderHistory.Columns.Add("name");
            orderHistory.Columns.Add("totalShares", typeof(Decimal));
            orderHistory.Columns.Add("totalAmount", typeof(Decimal));

            // apply filter
            for (int i=0; i < orderHistory.Rows.Count; i++)
            {
                DataRow row = orderHistory.Rows[i];

                if (Convert.ToString(row["status"]).Trim() == "pending")
                {
                    row["feeCharged"] = (decimal)0.00;
                    row["totalShares"] = (decimal)0.00;
                    row["totalAmount"] = (decimal)0.00;
                }

                if (ddlOrderFilter.SelectedValue != row["buyOrSell"].ToString().Trim() && ddlOrderFilter.SelectedValue != "0")
                {
                    row.Delete();
                    continue;
                }
                if (ddlTypeFilter.SelectedValue != row["securityType"].ToString().Trim() && ddlTypeFilter.SelectedValue != "0")
                {
                    row.Delete();
                    continue;
                }
                if (ddlStatusFilter.SelectedValue != row["status"].ToString().Trim() && ddlStatusFilter.SelectedValue != "0")
                {
                    row.Delete();
                    continue;
                }
                if (codeFilter.Text != row["securityCode"].ToString().Trim() && codeFilter.Text != "")
                {
                    row.Delete();
                    continue;
                }
            }

            // commit changes to the datatable
            orderHistory.AcceptChanges();

            //get security name and transaction data
            string name, baseCurrency;          // baseCurrency is just a dummy variable
            DataTable trasantionTable = new DataTable();
            foreach (DataRow row in orderHistory.Rows)
            {
                // get name of security
                if (Convert.ToString(row["securityType"]).Trim() == "bond")
                    myHKeInvestCode.getSecurityNameBase("bond", (string)row["securityCode"], out name, out baseCurrency);
                else if (Convert.ToString(row["securityType"]).Trim() == "unit trust")
                    myHKeInvestCode.getSecurityNameBase("unit trust", (string)row["securityCode"], out name, out baseCurrency);
                else
                    myHKeInvestCode.getSecurityNameBase("stock", (string)row["securityCode"], out name, out baseCurrency);
                row["name"] = name;

                // calculate order statistics
                sql = "SELECT * FROM [Transaction] WHERE [orderNumber] = '" + row["orderNumber"].ToString().Trim() + "'";
                DataTable transactions = myHKeInvestData.getData(sql);
                if (transactions.Rows.Count < 1)
                    continue;

                // add extra transactions into the table
                trasantionTable.Merge(transactions);

                // get total shares and dollar amount
                decimal shares = 0, price = 0;
                foreach (DataRow transaction in transactions.Rows)
                {
                    shares += (decimal)transaction["executeShares"];
                    price += (decimal)transaction["executePrice"];
                }
                row["totalShares"] = shares;
                row["totalAmount"] = price;
            }

            gvHistory.DataSource = orderHistory;
            gvHistory.DataBind();
            gvHistory.Visible = true;

            gvTransaction.DataSource = trasantionTable;
            gvTransaction.DataBind();
            gvTransaction.Visible = true;

            /*
            // Bond/Unit trust listings
            lblActiveBond.Visible = true;
            string sql = "SELECT [Order].*, [BuyBondOrder].amount FROM [Order] INNER JOIN [BuyBondOrder] ON [Order].[orderNumber] = [BuyBondOrder].[orderNumber] WHERE [status] <> 'completed' AND [accountNumber] = '" + accountNumber + "'";
            DataTable activeBondOrder = myHKeInvestData.getData(sql);
            sql = "SELECT [Order].*, [SellBondOrder].shares AS amount FROM [Order] INNER JOIN [SellBondOrder] ON [Order].[orderNumber] = [SellBondOrder].[orderNumber] WHERE [status] <> 'completed' AND [accountNumber] = '" + accountNumber + "'";
            activeBondOrder.Merge(myHKeInvestData.getData(sql));
            activeBondOrder.Columns.Add("name");

            string name, baseCurrency;          // baseCurrency is just a dummy variable
            foreach (DataRow row in activeBondOrder.Rows)
            {
                if (Convert.ToString(row["securityType"]).Trim() == "bond")
                    myHKeInvestCode.getSecurityNameBase("bond", (string)row["securityCode"], out name, out baseCurrency);
                else
                    myHKeInvestCode.getSecurityNameBase("unit trust", (string)row["securityCode"], out name, out baseCurrency);
                row["name"] = name;
            }

            gvActiveBond.DataSource = activeBondOrder;
            gvActiveBond.DataBind();

            // Stock listings
            lblActiveStock.Visible = true;
            sql = "SELECT [Order].*, [StockOrder].shares, [StockOrder].orderType, [StockOrder].expiaryDay, [StockOrder].limitPrice, [StockOrder].stopPrice FROM [Order] INNER JOIN [StockOrder] ON [Order].[orderNumber] = [StockOrder].[orderNumber] WHERE [status] <> 'completed' AND [status] <> 'cancelled' AND [accountNumber] = '" + accountNumber + "'";
            DataTable activeStockOrder = myHKeInvestData.getData(sql);
            activeStockOrder.Columns.Add("name");

            foreach (DataRow row in activeStockOrder.Rows)
            {
                myHKeInvestCode.getSecurityNameBase("stock", (string)row["securityCode"], out name, out baseCurrency);
                row["name"] = name;
            }

            gvActiveStock.DataSource = activeStockOrder;
            gvActiveStock.DataBind();*/
        }

        protected void genReport_Click(object sender, EventArgs e)
        {
            //get client name
            string sql = "SELECT lastName, firstName FROM Client WHERE accountNumber = '" + accountNumber + "'";

            DataTable dtClient = myHKeInvestData.getData(sql);
            if (dtClient == null || dtClient.Rows.Count == 0) {
                lbl6b.Visible = false;
                lbl6c.Visible = false;
                lbl6d.Visible = false;
                return; } // If the DataSet is null, a SQL error occurred.

            // Show the client name(s) on the web page.
            string clientName = "Client(s): ";
            int i = 1;
            foreach (DataRow row in dtClient.Rows)
            {
                clientName = clientName + row["lastName"] + ", " + row["firstName"];
                if (dtClient.Rows.Count != i)
                {
                    clientName = clientName + "and ";
                }
                i = i + 1;
            }
            lblClientName.Text = clientName;
            lblClientName.Visible = true;

            generate6a_Click();
            ddlSecurityType_SelectedIndexChanged();
            generate6c_Click();
            generate6d_Click();

            lbl6b.Visible = true;
            lbl6c.Visible = true;
            lbl6d.Visible = true;
        }


    }
}