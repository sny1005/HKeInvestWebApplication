﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="trackProfitOrLoss.aspx.cs" Inherits="HKeInvestWebApplication.ClientOnly.trackProfitOrLoss" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Profit and Loss Tracking</h2>
    <div class =" form-group">
        
        <div>
        <asp:RadioButtonList ID="rbDisplayType" runat="server" RepeatDirection="Horizontal" AutoPostBack="True" CellPadding="5" CellSpacing="5" OnSelectedIndexChanged="rbDisplayType_SelectedIndexChanged">
            <asp:ListItem>All Securities</asp:ListItem>
            <asp:ListItem>One Type of Securities</asp:ListItem>
            <asp:ListItem>Individual Security</asp:ListItem>
        </asp:RadioButtonList>
        <br />
        </div>
         <div class =" col-md-4">
        <asp:DropDownList ID="ddlSecurityType" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlSecurityType_SelectedIndexChanged" Visible="False">
                <asp:ListItem Value="0">Security type</asp:ListItem>
                <asp:ListItem Value="bond">Bond</asp:ListItem>
                <asp:ListItem Value="stock">Stock</asp:ListItem>
                <asp:ListItem Value="unit trust">Unit Trust</asp:ListItem>
            </asp:DropDownList>
        <asp:Label runat="server" Text="Security Code" AssociatedControlID="txtSecurityCode" CssClass="col-md-4" ID="lblSecurityCode" Visible="False"></asp:Label>
        <asp:TextBox ID="txtSecurityCode" runat="server" CssClass="text-danger" MaxLength="4" OnTextChanged="txtSecurityCode_TextChanged" Visible="False"></asp:TextBox>
    </div>
        <div>
            <asp:Label runat="server" Text="View In:" AssociatedControlID="viewIn"></asp:Label><asp:RadioButtonList ID="viewIn" runat="server" AutoPostBack="True" OnSelectedIndexChanged="viewIn_SelectedIndexChanged" >
                <asp:ListItem>Absolute Dollar Amount</asp:ListItem>
                <asp:ListItem>Percentage</asp:ListItem>
            </asp:RadioButtonList>
        </div>

    </div>

    <div>
        <asp:GridView ID="gvIndividual" runat="server" CellPadding="5" AutoGenerateColumns="False" Visible="False" OnSelectedIndexChanged="gvIndividual_SelectedIndexChanged">
            <Columns>
                <asp:BoundField HeaderText="Type" ReadOnly="True" DataField="type" SortExpression="Type" />
                <asp:BoundField DataField="code" HeaderText="Code" ReadOnly="True" SortExpression="Code" />
                <asp:BoundField DataField="name" HeaderText="Name" ReadOnly="True" SortExpression="Name" />
                <asp:BoundField DataField="shares" HeaderText="Number of Shares" ReadOnly="True" SortExpression="Number of Shares" />
                <asp:BoundField DataField="buyAmount" HeaderText="Total Dollar Amount for Buying" DataFormatString="{0:n2}" ReadOnly="True" SortExpression="Total Dollar Amount for Buying"></asp:BoundField>
                <asp:BoundField DataField="sellAmount" HeaderText="Total Dollar Amount for Selling" DataFormatString="{0:n2}" ReadOnly="True" SortExpression="Total Dollar Amount for Selling" />
                <asp:BoundField DataField="fee" HeaderText="Total Fee Paid" DataFormatString="{0:n2}" ReadOnly="True" SortExpression="Total Fee Paid" />
                <asp:BoundField DataField="profit" HeaderText="Profit/Loss" DataFormatString="{0:n2}" ReadOnly="True" SortExpression="Profit/Loss" />
            </Columns>
        </asp:GridView>
    </div>
    <div>
         <asp:GridView ID="gvSecurity" runat="server" CellPadding="5" AutoGenerateColumns="False">
            <Columns>
                <asp:BoundField DataField="buyAmount" HeaderText="Total Dollar Amount for Buying" DataFormatString="{0:n2}" ReadOnly="True" SortExpression="Total Dollar Amount for Buying"></asp:BoundField>
                <asp:BoundField DataField="sellAmount" HeaderText="Total Dollar Amount for Selling" DataFormatString="{0:n2}" ReadOnly="True" SortExpression="Total Dollar Amount for Selling" />
                <asp:BoundField DataField="fee" HeaderText="Total Fee Paid" DataFormatString="{0:n2}" ReadOnly="True" SortExpression="Total Fee Paid" />
                <asp:BoundField DataField="profit" HeaderText="Profit/Loss" DataFormatString="{0:n2}" ReadOnly="True" SortExpression="Profit/Loss" />
            </Columns>
        </asp:GridView>

    </div>
</asp:Content>
