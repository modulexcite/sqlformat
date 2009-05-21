﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Laan.SQL.Formatter.Web.Formatter" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Laan SQL Formatter (alpha)</title>
    <link href="/Content/prettify.css" type="text/css" rel="stylesheet" />
    <style type="text/css">
        
    html, body {
      margin: 0;
      padding: 0;
    } 

    </style>
    <script type="text/javascript" src="/Scripts/jquery-1.3.2.min-vsdoc.js"></script>
    <script type="text/javascript" src="/Scripts/prettify.js"></script>

    <script type="text/javascript">
        $(document).ready(function() { prettyPrint(); });
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div>
        Input SQL
        <asp:TextBox class="code prettyprint lang-sql" ID="sqlInput" runat="server" style="width: 100%; height:180px" TextMode="MultiLine"></asp:TextBox>
    </div>
    <div style="text-align:right">
        <asp:Button ID="Button1" runat="server" Text="Convert" onclick="btnConvert_Click" />
    </div>
    <div>
        Output SQL
        <pre class="code prettyprint" style="width: 100%">
            <asp:Repeater ID="sqlOutput" runat="server">
<ItemTemplate>
<%# Container.DataItem %></ItemTemplate></asp:Repeater>     
        </pre>
    </div>

</asp:Content>