﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Data;
using HKeInvestWebApplication.ExternalSystems.Code_File;

namespace HKeInvestWebApplication.Code_File
{
    //**********************************************************
    //*  THE CODE IN THIS CLASS CAN BE MODIFIED AND ADDED TO.  *
    //**********************************************************
    public class HKeInvestCode
    {
        public string getDataType(string value)
        {
            // Returns the data type of value. Tests for more types can be added if needed.
            if (value != null)
            {
                int n; decimal d; DateTime dt;
                if (int.TryParse(value, out n)) { return "System.Int32"; }
                else if (decimal.TryParse(value, out d)) { return "System.Decimal"; }
                else if (DateTime.TryParse(value, out dt)) { return "System.DataTime"; }
            }
            return "System.String";
        }

        public string getSortDirection(System.Web.UI.StateBag viewState, string sortExpression)
        {
            // If the GridView is sorted for the first time or sorting is being done on a new column, 
            // then set the sort direction to "ASC" in ViewState.
            if (viewState["SortDirection"] == null || viewState["SortExpression"].ToString() != sortExpression)
            {
                viewState["SortDirection"] = "ASC";
            }
            // Othewise if the same column is clicked for sorting more than once, then toggle its SortDirection.
            else if (viewState["SortDirection"].ToString() == "ASC")
            {
                viewState["SortDirection"] = "DESC";
            }
            else if (viewState["SortDirection"].ToString() == "DESC")
            {
                viewState["SortDirection"] = "ASC";
            }
            return viewState["SortDirection"].ToString();
        }

        public DataTable unloadGridView(GridView gv)
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < gv.Columns.Count; i++)
            {
                dt.Columns.Add(((BoundField)gv.Columns[i]).DataField);
            }

            // For correct sorting, set the data type of each DataTable column based on the values in the GridView.
            gv.SelectedIndex = 0;
            for (int i = 0; i < gv.Columns.Count; i++)
            {
                dt.Columns[i].DataType = Type.GetType(getDataType(gv.SelectedRow.Cells[i].Text));
            }

            // Load the GridView data into the DataTable.
            foreach (GridViewRow row in gv.Rows)
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < gv.Columns.Count; j++)
                {
                    dr[((BoundField)gv.Columns[j]).DataField.ToString().Trim()] = row.Cells[j].Text;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public int getColumnIndexByName(GridView gv, string columnName)
        {
            // Helper method to get GridView column index by a column's DataField name.
            for (int i = 0; i < gv.Columns.Count; i++)
            {
                if (((BoundField)gv.Columns[i]).DataField.ToString().Trim() == columnName.Trim())
                { return i; }
            }
            MessageBox.Show("Column '" + columnName + "' was not found \n in the GridView '" + gv.ID.ToString() + "'.");
            return -1;
        }

        public decimal convertCurrency(string fromCurrency, string fromCurrencyRate, string toCurrency, string toCurrencyRate, decimal value)
        {
            if(fromCurrency == toCurrency)
            {
                return value;
            }
            else
            {
                return Math.Round(Convert.ToDecimal(fromCurrencyRate) / Convert.ToDecimal(toCurrencyRate) * value - (decimal).005, 2);
            }
        }

        //addtional helper function to get the exchange rate for the target currency
        public string getCurrencyRate(List<string> currency, List<string> rate, string target)
        {
            for(int i=0; i<currency.Count; i++)
            {
                if (currency[i] == target.Trim())
                    return rate[i];
            }
            return "-1";
        }

        //access the external system and put the currency data into view state
        public List<string>[] CurrencyData()
        {
            //declare local objects;
            ExternalFunctions myExternalFunctions = new ExternalFunctions();

            // Get the available currencies to populate the DropDownList.
            DataTable dtCurrency = myExternalFunctions.getCurrencyData();

            //Load currency data into 2 list
            List<string> rate = new List<string>();
            List<string> currency = new List<string>();
            foreach (DataRow row in dtCurrency.Rows)
            {
                rate.Add(Convert.ToString(row["rate"]));
                currency.Add(Convert.ToString(row["currency"]));
            }

            List<string>[] output = new List<string>[2];
            output[0] = rate;
            output[1] = currency;
            return output;
        }





    }
}