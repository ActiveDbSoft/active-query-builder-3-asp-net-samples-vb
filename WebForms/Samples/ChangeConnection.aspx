﻿<%@ Page Title="Change Connection Demo" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ChangeConnection.aspx.cs" Inherits="WebForms_Samples.Samples.ChangeConnection" %>
<%@ Register TagPrefix="AQB" Namespace="ActiveQueryBuilder.Web.WebForms" Assembly="ActiveQueryBuilder.Web.WebForms" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="row">
        <div class="col-md-12">
            <h1>Switch Database Connections Demo</h1>
            <p>Switching between different database connections at run time.</p>
            <div>
                <h3 style="display: inline">Connect to:</h3>
                <ASP:Button runat="server" OnClick="OnClick" Text="NorthwindXmlMetaData" />
                <ASP:Button runat="server" OnClick="OnClick" Text="Db2XmlMetaData" />
                <ASP:Button runat="server" OnClick="OnClick" Text="SQLite" />
            </div>
        </div>
        <div class="col-md-12">
            <!--Turn the UseDefaultTheme to False for not using the default theme. You will have to load the JQueryUI library then. -->
            <AQB:QueryBuilderControl ID="QueryBuilderControl1" runat="server" UseDefaultTheme="false" />
            <div class="qb-ui-layout">
                <div class="qb-ui-layout__top">
                    <div class="qb-ui-layout__left">
                        <div class="qb-ui-structure-tabs">
                            <div class="qb-ui-structure-tabs__tab">
                                <input type="radio" id="tree-tab" name="qb-tabs" checked />
                                <label class="ui-widget-header qb-widget-header" for="tree-tab">Database</label>
                                <div class="qb-ui-structure-tabs__content">
                                    <AQB:ObjectTreeView runat="server" ID="ObjectTreeView1" />
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="qb-ui-layout__right">
                        <AQB:SubQueryNavigationBar runat="server" ID="SubQueryNavigationBar1" />
                        <AQB:Canvas runat="server" ID="Canvas1" />
                        <AQB:StatusBar runat="server" ID="StatusBar1" />
                        <AQB:Grid runat="server" ID="Grid1" />
                    </div>
                </div>
                <div class="qb-ui-layout__bottom">
                    <AQB:SqlEditor runat="server" ID="SqlEditor1" />
                </div>
            </div>
        </div>
    </div>

</asp:Content>
